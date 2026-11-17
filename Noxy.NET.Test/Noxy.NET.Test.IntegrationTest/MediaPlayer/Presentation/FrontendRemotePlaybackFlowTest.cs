using System.Globalization;
using Microsoft.Extensions.Configuration;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FrontendRemotePlaybackFlowTest
{
    private static ApiFactory Factory(int? graceSeconds = null, double? idleMinutes = null)
    {
        Dictionary<string, string?> settings = [];
        if (graceSeconds is { } grace) settings[ConfigurationPath.Combine(ConfigRemote.SectionName, nameof(ConfigRemote.ActiveGraceSeconds))] = grace.ToString();
        if (idleMinutes is { } idle) settings[ConfigurationPath.Combine(ConfigRemote.SectionName, nameof(ConfigRemote.IdleTeardownMinutes))] = idle.ToString(CultureInfo.InvariantCulture);

        return new(settings);
    }

    private static ResponseQueue QueueOf(FrontendClient frontend)
    {
        return frontend.Queue.Queue.ToResponse();
    }

    private static List<Guid> MediaOf(List<QueueItem> itemList)
    {
        return itemList.Select(x => x.MediaFileID).ToList();
    }

    private static List<Guid> ItemOf(List<QueueItem> itemList)
    {
        return itemList.Select(x => x.ItemID).ToList();
    }

    // The player holds a volume as 0 to 1; a test compares it as the whole percent it came from, never as a double.
    private static int Percent(double volume)
    {
        return (int)Math.Round(volume * 100);
    }

    private static Task Until(Func<bool> condition, string what, int seconds = 15)
    {
        return FrontendClient.WaitFor(() => Task.FromResult(condition()), what, seconds);
    }

    // Both devices hold the same queue: the item playing and what is ahead.
    private static bool SameQueue(FrontendClient first, FrontendClient second)
    {
        ResponseQueue a = QueueOf(first);
        ResponseQueue b = QueueOf(second);

        return a.Current == b.Current && ItemOf(a.UserQueueList).SequenceEqual(ItemOf(b.UserQueueList)) && ItemOf(a.SourceList).SequenceEqual(ItemOf(b.SourceList));
    }

    // The laptop plays the first of three media files and is the active device; the phone joined afterwards and is a controller.
    private static async Task<(FrontendClient Laptop, FrontendClient Phone, List<ResponseMediaFile> MediaList, ApiAccount Account, string Email)> Session(ApiFactory factory, MediaApiClient api)
    {
        (ApiAccount account, string email) = await api.CreateAccount();
        List<ResponseMediaFile> mediaList = [];
        foreach (string title in (string[])["A", "B", "C"]) mediaList.Add(await api.UploadAndWait(account, title));

        FrontendClient laptop = await FrontendClient.CreateSignedIn(factory, email);
        await laptop.Control.StartLibrary(mediaList[0].ID, LibrarySortEnum.Title, false, string.Empty);
        await Until(() => laptop.Remote.Role == PlaybackRoleEnum.Active, "the laptop to become the active device");

        FrontendClient phone = await FrontendClient.CreateSignedIn(factory, email);
        await Until(() => phone.Remote.Role == PlaybackRoleEnum.Controller && SameQueue(laptop, phone), "the phone to hold the laptop's queue");

        return (laptop, phone, mediaList, account, email);
    }

    [Fact]
    public async Task ThePhoneSeesWhatTheLaptopPlaysAndItsQueue()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        (FrontendClient laptop, FrontendClient phone, List<ResponseMediaFile> mediaList, _, _) = await Session(factory, api);
        await using FrontendClient laptopOwner = laptop;
        await using FrontendClient phoneOwner = phone;

        Assert.Equal(mediaList[0].ID, QueueOf(phone).Current?.MediaFileID);
        Assert.Equal([mediaList[1].ID, mediaList[2].ID], MediaOf(QueueOf(phone).SourceList));
        await Until(() => phone.Remote.Remote?.State.IsPlaying == true && phone.Remote.Remote.State.Title == "A", "the phone to see the laptop's state");
        Assert.Equal(laptop.Remote.ActiveDeviceID, phone.Remote.ActiveDeviceID);
        Assert.False(phone.Audio.IsPlaying);
        Assert.Empty(phone.Audio.CallList);
    }

    [Fact]
    public async Task ThePhonePausesTheLaptop()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        (FrontendClient laptop, FrontendClient phone, _, _, _) = await Session(factory, api);
        await using FrontendClient laptopOwner = laptop;
        await using FrontendClient phoneOwner = phone;

        await phone.Control.Pause();

        await Until(() => !laptop.Audio.IsPlaying, "the laptop to pause");
        await Until(() => phone.Remote.Remote?.State.IsPlaying == false, "the phone to see the pause");
        Assert.Empty(phone.Audio.CallList);

        await phone.Control.Play();

        await Until(() => laptop.Audio.IsPlaying, "the laptop to play again");
    }

    [Fact]
    public async Task ThePhoneEditsTheLaptopsQueue_PlayNextRemoveMove_AndBothAgree()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        (FrontendClient laptop, FrontendClient phone, List<ResponseMediaFile> mediaList, ApiAccount account, _) = await Session(factory, api);
        await using FrontendClient laptopOwner = laptop;
        await using FrontendClient phoneOwner = phone;

        await phone.Control.PlayNext(mediaList[2].ID);
        await phone.Control.AddToQueue(mediaList[1].ID);
        await Until(() => SameQueue(laptop, phone) && QueueOf(laptop).UserQueueList.Count == 2, "both devices to hold the edits");
        Assert.Equal([mediaList[2].ID, mediaList[1].ID], MediaOf(QueueOf(laptop).UserQueueList));

        QueueItem last = QueueOf(phone).UserQueueList[1];
        await phone.Control.MoveInQueue(last.ItemID, null);
        await Until(() => SameQueue(laptop, phone) && MediaOf(QueueOf(laptop).UserQueueList).SequenceEqual([mediaList[1].ID, mediaList[2].ID]), "the move to reach both devices");

        await phone.Control.RemoveFromQueue(QueueOf(phone).UserQueueList[0].ItemID);
        await Until(() => SameQueue(laptop, phone) && QueueOf(laptop).UserQueueList.Count == 1, "the removal to reach both devices");
        Assert.Equal([mediaList[2].ID], MediaOf(QueueOf(laptop).UserQueueList));
        Assert.NotNull(account);
    }

    [Fact]
    public async Task ThePhoneRemovingTheCurrentItem_MakesTheLaptopSkip_AndItNeverEntersHistory()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        (FrontendClient laptop, FrontendClient phone, List<ResponseMediaFile> mediaList, _, _) = await Session(factory, api);
        await using FrontendClient laptopOwner = laptop;
        await using FrontendClient phoneOwner = phone;

        await phone.Control.RemoveFromQueue(QueueOf(phone).Current?.ItemID ?? throw new InvalidOperationException("No current item."));

        await Until(() => laptop.Player.Current?.MediaFileID == mediaList[1].ID && laptop.Audio.IsPlaying, "the laptop to skip to the next item");
        await Until(() => SameQueue(laptop, phone), "the phone to follow");
        Assert.DoesNotContain(mediaList[0].ID, MediaOf(QueueOf(laptop).HistoryList));
    }

    [Fact]
    public async Task SeekAndVolumeFromThePhone_ApplyOnTheLaptop_AndEveryDeviceAdoptsTheVolume()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        (FrontendClient laptop, FrontendClient phone, _, _, _) = await Session(factory, api);
        await using FrontendClient laptopOwner = laptop;
        await using FrontendClient phoneOwner = phone;

        await phone.Control.Seek(TimeSpan.FromSeconds(30));
        await phone.Control.SetVolume(40);

        await Until(() => laptop.Audio.Position == TimeSpan.FromSeconds(30), "the laptop to seek");
        await Until(() => Percent(laptop.Audio.Volume) == 40, "the laptop to take the volume");
        await Until(() => Percent(phone.Audio.Volume) == 40, "the phone to adopt the volume");
        Assert.Equal(40, await laptop.DeviceFile.GetVolume());
        await Until(() => Percent(phone.Audio.Volume) == 40 && phone.Remote.Remote?.State.Volume == 40, "the phone to see the volume");
        Assert.Equal(40, await phone.DeviceFile.GetVolume());
        Assert.False(phone.Audio.IsPlaying);
    }

    [Fact]
    public async Task ThePhoneStartingAPlaylistWhileTheLaptopIsActive_PlaysOnTheLaptop()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        (FrontendClient laptop, FrontendClient phone, List<ResponseMediaFile> mediaList, _, _) = await Session(factory, api);
        await using FrontendClient laptopOwner = laptop;
        await using FrontendClient phoneOwner = phone;
        ResponsePlaylist playlist = await laptop.Api.CreatePlaylist(new() { Name = "Mix" });
        ResponsePlaylistDetail detail = await laptop.Api.AddPlaylistEntryList(playlist.ID, new() { MediaFileIDList = [mediaList[2].ID, mediaList[1].ID] });
        await laptop.Sync.SyncPlaylistList();
        await laptop.Sync.SyncPlaylistDetail(playlist.ID);
        phone.Audio.CallList.Clear();

        await phone.Control.StartPlaylist(playlist.ID, detail.EntryList[0].ID);

        await Until(() => laptop.Player.Current?.ItemID == detail.EntryList[0].ID && laptop.Audio.IsPlaying, "the laptop to play the playlist");
        await Until(() => SameQueue(laptop, phone) && phone.Queue.Queue.Current?.ItemID == detail.EntryList[0].ID, "the phone to see the playlist queue");
        Assert.Empty(phone.Audio.CallList);
    }

    [Fact]
    public async Task TransferMovesPlaybackWithQueueAndPosition()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        (FrontendClient laptop, FrontendClient phone, List<ResponseMediaFile> mediaList, _, _) = await Session(factory, api);
        await using FrontendClient laptopOwner = laptop;
        await using FrontendClient phoneOwner = phone;
        laptop.Audio.Position = TimeSpan.FromSeconds(20);
        await laptop.Control.Next();
        laptop.Audio.Position = TimeSpan.FromSeconds(20);
        await Until(() => phone.Remote.Remote?.State.CurrentItemID == laptop.Player.Current?.ItemID, "the phone to see the second item");

        await phone.Control.Transfer(await phone.Device.GetDeviceID());

        await Until(() => phone.Remote.Role == PlaybackRoleEnum.Active && laptop.Remote.Role == PlaybackRoleEnum.Controller, "the roles to flip");
        Assert.False(laptop.Audio.IsPlaying);
        Assert.True(phone.Audio.IsPlaying);
        Assert.Equal(mediaList[1].ID, phone.Player.Current?.MediaFileID);
        Assert.Equal(TimeSpan.FromSeconds(20), phone.Audio.Position);
        await Until(() => SameQueue(laptop, phone), "both devices to hold the queue");
        Assert.Equal([mediaList[0].ID], MediaOf(QueueOf(phone).HistoryList));
    }

    [Fact]
    public async Task TheLaptopLosingTheHub_StillPlaysAndAdvancesItsQueue_AndOnReconnecting_WithNoOneElsePlaying_BecomesActiveAgainWithThatQueue()
    {
        await using ApiFactory factory = Factory(graceSeconds: 1);
        MediaApiClient api = new(factory);
        (FrontendClient laptop, FrontendClient phone, List<ResponseMediaFile> mediaList, _, _) = await Session(factory, api);
        await using FrontendClient laptopOwner = laptop;
        await using FrontendClient phoneOwner = phone;

        await laptop.Hub.Stop();
        Assert.Equal(PlaybackRoleEnum.Local, laptop.Remote.Role);
        laptop.Audio.RaiseEnded();
        await Until(() => laptop.Player.Current?.MediaFileID == mediaList[1].ID && laptop.Audio.IsPlaying, "the laptop to advance by itself");
        await Until(() => phone.Remote.Role == PlaybackRoleEnum.Local, "the phone to see nobody is active");

        await laptop.Hub.Start();

        await Until(() => laptop.Remote.Role == PlaybackRoleEnum.Active, "the laptop to become active again");
        await Until(() => phone.Remote.Role == PlaybackRoleEnum.Controller && phone.Queue.Queue.Current?.MediaFileID == mediaList[1].ID && SameQueue(laptop, phone), "the phone to hold the laptop's queue");
        Assert.True(laptop.Audio.IsPlaying);
    }

    [Fact]
    public async Task ADeviceReconnectingWhilePlaying_WhileAnotherPlays_IsRefused_AndPauses()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        (FrontendClient laptop, FrontendClient phone, List<ResponseMediaFile> mediaList, _, _) = await Session(factory, api);
        await using FrontendClient laptopOwner = laptop;
        await using FrontendClient phoneOwner = phone;
        await phone.Hub.Stop();
        await phone.Control.StartLibrary(mediaList[2].ID, LibrarySortEnum.Title, false, string.Empty);
        Assert.True(phone.Audio.IsPlaying);
        Assert.Equal(PlaybackRoleEnum.Local, phone.Remote.Role);

        await phone.Hub.Start();

        await Until(() => !phone.Audio.IsPlaying, "the refused phone to pause");
        await Until(() => phone.Remote.Role == PlaybackRoleEnum.Controller, "the phone to become a controller");
        Assert.True(laptop.Audio.IsPlaying);
        Assert.Equal(PlaybackRoleEnum.Active, laptop.Remote.Role);
        Assert.Equal(mediaList[0].ID, laptop.Player.Current?.MediaFileID);
    }

    [Fact]
    public async Task TheActiveDeviceDroppingForGood_ClearsTheActiveRecord_AndThePhoneFallsBackToLocal()
    {
        await using ApiFactory factory = Factory(graceSeconds: 1);
        MediaApiClient api = new(factory);
        (FrontendClient laptop, FrontendClient phone, List<ResponseMediaFile> mediaList, _, _) = await Session(factory, api);
        await using FrontendClient laptopOwner = laptop;
        await using FrontendClient phoneOwner = phone;

        await laptop.Hub.Stop();

        await Until(() => phone.Remote.Role == PlaybackRoleEnum.Local && phone.Remote.ActiveDeviceID is null, "the phone to fall back to local");
        Assert.True(phone.Control.CanContinue);
        Assert.Equal(mediaList[0].ID, phone.Queue.Queue.Current?.MediaFileID);

        await phone.Control.Play();

        await Until(() => phone.Audio.IsPlaying && phone.Remote.Role == PlaybackRoleEnum.Active, "the phone to play and become active");
    }

    [Fact]
    public async Task AfterTheSessionEnds_EachDeviceStillHasItsLocalCopy_AndContinueResumesIt()
    {
        await using ApiFactory factory = Factory(idleMinutes: 0.02);
        MediaApiClient api = new(factory);
        (FrontendClient laptop, FrontendClient phone, List<ResponseMediaFile> mediaList, _, _) = await Session(factory, api);
        await using FrontendClient laptopOwner = laptop;
        await using FrontendClient phoneOwner = phone;
        await phone.Control.Pause();
        await Until(() => !laptop.Audio.IsPlaying, "the laptop to pause");

        await Until(() => !phone.Remote.HasSession && !laptop.Remote.HasSession, "the session to end", 40);

        Assert.Equal(mediaList[0].ID, QueueOf(phone).Current?.MediaFileID);
        Assert.Equal(mediaList[0].ID, QueueOf(laptop).Current?.MediaFileID);
        await phone.Reporter.SaveNow();
        Assert.NotNull(await phone.QueueFile.Get());
        Assert.True(phone.Control.CanContinue);
        Assert.True(await phone.Control.Continue());
        Assert.True(phone.Audio.IsPlaying);
        Assert.Equal(mediaList[0].ID, phone.Player.Current?.MediaFileID);
    }

    [Fact]
    public async Task AnotherAccountsDevice_NeverSeesAnything()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        (FrontendClient laptop, FrontendClient phone, _, _, _) = await Session(factory, api);
        await using FrontendClient laptopOwner = laptop;
        await using FrontendClient phoneOwner = phone;
        (_, string otherEmail) = await api.CreateAccount();

        await using FrontendClient stranger = await FrontendClient.CreateSignedIn(factory, otherEmail);
        await Task.Delay(500, TestContext.Current.CancellationToken);

        Assert.Equal(PlaybackRoleEnum.Local, stranger.Remote.Role);
        Assert.Null(stranger.Remote.Remote);
        Assert.False(stranger.Remote.HasSession);
        Assert.Single(stranger.Remote.DeviceList);
        Assert.Equal(0, stranger.Queue.Queue.ItemCount);
    }

    [Fact]
    public async Task ALibraryBiggerThanTheQueueLimit_StartsAsAFullQueue_AndSyncsToTheOtherDevice()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await api.CreateAccount();
        await new ApiSeeder(factory).SeedMediaFileList(account.UserID, 20000);
        await using FrontendClient laptop = await FrontendClient.CreateSignedIn(factory, email);
        await using FrontendClient phone = await FrontendClient.CreateSignedIn(factory, email);
        List<ResponseMediaFile> library = await laptop.Store.GetMediaFileList();
        Assert.Equal(20000, library.Count);

        Assert.True(await laptop.Queue.StartSource(new() { Kind = QueueSourceKindEnum.Library, PlaylistID = null, Sort = LibrarySortEnum.Title, Descending = false, Search = string.Empty }, library.OrderBy(x => x.Title).First().ID, ShuffleModeEnum.Random));
        Assert.Equal(Constants.QueueMaxItemCount, laptop.Queue.Queue.ItemCount);
        bool? accepted = await laptop.Reporter.ClaimNow();

        Assert.True(accepted);
        await Until(() => phone.Queue.Queue.ItemCount == Constants.QueueMaxItemCount && phone.Remote.Role == PlaybackRoleEnum.Controller, "the phone to hold the full queue", 60);
        Assert.Equal(QueueOf(laptop).Current, QueueOf(phone).Current);
    }
}
