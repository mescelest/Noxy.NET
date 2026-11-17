using System.Text.Json;
using Microsoft.Data.Sqlite;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FrontendQueueFlowTest
{
    private static ResponseQueue QueueOf(FrontendClient frontend)
    {
        return frontend.Queue.Queue.ToResponse();
    }

    private static List<Guid> MediaOf(List<QueueItem> itemList)
    {
        return itemList.Select(x => x.MediaFileID).ToList();
    }

    private static Task<bool> IsPlaying(FrontendClient frontend, Guid mediaFileID)
    {
        return Task.FromResult(frontend.Player.Current?.MediaFileID == mediaFileID && frontend.Audio.IsPlaying);
    }

    // A playlist of the media files, synced to the frontend with its detail.
    private static async Task<ResponsePlaylistDetail> Playlist(FrontendClient frontend, string name, params Guid[] mediaFileIDList)
    {
        ResponsePlaylist playlist = await frontend.Api.CreatePlaylist(new() { Name = name });
        ResponsePlaylistDetail detail = await frontend.Api.AddPlaylistEntryList(playlist.ID, new() { MediaFileIDList = [.. mediaFileIDList] });
        await frontend.Sync.SyncPlaylistList();
        await frontend.Sync.SyncPlaylistDetail(playlist.ID);

        return detail;
    }

    [Fact]
    public async Task StartLibrary_BuildsTheQueueFromTheSyncedLibrary_AndPlays()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await api.CreateAccount();
        ResponseMediaFile a = await api.UploadAndWait(account, "A");
        ResponseMediaFile b = await api.UploadAndWait(account, "B");
        ResponseMediaFile c = await api.UploadAndWait(account, "C");
        await using FrontendClient frontend = await FrontendClient.CreateSignedIn(factory, email);

        await frontend.Control.StartLibrary(b.ID, LibrarySortEnum.Title, false, string.Empty);

        ResponseQueue queue = QueueOf(frontend);
        Assert.Equal(b.ID, queue.Current?.MediaFileID);
        Assert.Equal([c.ID], MediaOf(queue.SourceList));
        Assert.Equal(await frontend.FileStore.GetPath(b.ID), frontend.Audio.LoadedPath);
        Assert.True(frontend.Audio.IsPlaying);
        Assert.NotNull(a);
    }

    [Fact]
    public async Task ShuffleOn_PlaysTheClickedMediaFileFirst_AndToggleOffContinuesInOrderAfterIt()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await api.CreateAccount();
        List<ResponseMediaFile> mediaList = [];
        foreach (string title in (string[])["A", "B", "C", "D", "E"]) mediaList.Add(await api.UploadAndWait(account, title));
        await using FrontendClient frontend = await FrontendClient.CreateSignedIn(factory, email);
        await frontend.Control.SetShuffle(ShuffleModeEnum.Random);

        await frontend.Control.StartLibrary(mediaList[1].ID, LibrarySortEnum.Title, false, string.Empty);

        Assert.Equal(mediaList[1].ID, QueueOf(frontend).Current?.MediaFileID);
        Assert.Equal(4, QueueOf(frontend).SourceList.Count);
        Assert.Equal(ShuffleModeEnum.Random, frontend.Player.Shuffle);

        await frontend.Control.SetShuffle(ShuffleModeEnum.Off);

        Assert.Equal(mediaList[1].ID, QueueOf(frontend).Current?.MediaFileID);
        Assert.Equal([mediaList[2].ID, mediaList[3].ID, mediaList[4].ID], MediaOf(QueueOf(frontend).SourceList));
    }

    [Fact]
    public async Task AMediaFileUploadedOnTheServer_JoinsASortedLibraryQueueAtItsSortPlace()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await api.CreateAccount();
        ResponseMediaFile a = await api.UploadAndWait(account, "A");
        ResponseMediaFile c = await api.UploadAndWait(account, "C");
        ResponseMediaFile e = await api.UploadAndWait(account, "E");
        await using FrontendClient frontend = await FrontendClient.CreateSignedIn(factory, email);
        await frontend.Control.StartLibrary(c.ID, LibrarySortEnum.Title, false, string.Empty);

        // B sorts before the playing item and is not added; D sorts after it and joins at its place.
        ResponseMediaFile b = await api.UploadAndWait(account, "B");
        ResponseMediaFile d = await api.UploadAndWait(account, "D");

        await FrontendClient.WaitFor(() => Task.FromResult(MediaOf(QueueOf(frontend).SourceList).SequenceEqual([d.ID, e.ID])), "the uploaded media file to join the queue");
        Assert.Equal(c.ID, QueueOf(frontend).Current?.MediaFileID);
        Assert.DoesNotContain(b.ID, MediaOf(QueueOf(frontend).SourceList));
        Assert.NotNull(a);
    }

    [Fact]
    public async Task PlayNextAndAddToQueue_PlayBeforeTheRestOfThePlaylist_InTheRightOrder()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await api.CreateAccount();
        ResponseMediaFile p = await api.UploadAndWait(account, "P");
        ResponseMediaFile q = await api.UploadAndWait(account, "Q");
        ResponseMediaFile r = await api.UploadAndWait(account, "R");
        ResponseMediaFile x = await api.UploadAndWait(account, "X");
        ResponseMediaFile y = await api.UploadAndWait(account, "Y");
        await using FrontendClient frontend = await FrontendClient.CreateSignedIn(factory, email);
        ResponsePlaylistDetail playlist = await Playlist(frontend, "Mix", p.ID, q.ID, r.ID);
        await frontend.Control.StartPlaylist(playlist.ID, playlist.EntryList[0].ID);

        await frontend.Control.AddToQueue(y.ID);
        await frontend.Control.PlayNext(x.ID);

        Assert.Equal([x.ID, y.ID], MediaOf(QueueOf(frontend).UserQueueList));
        foreach (Guid expected in (Guid[])[x.ID, y.ID, q.ID, r.ID])
        {
            frontend.Audio.RaiseEnded();
            await FrontendClient.WaitFor(() => IsPlaying(frontend, expected), "the next item to play");
        }

        Assert.Empty(QueueOf(frontend).UserQueueList);
        Assert.Equal([p.ID, x.ID, y.ID, q.ID], MediaOf(QueueOf(frontend).HistoryList));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task KeepQueuedItems_KeepsTheUserQueueAcrossStartingAnotherPlaylist_OnlyWhenTheSettingIsOn(bool keep)
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await api.CreateAccount();
        ResponseMediaFile a = await api.UploadAndWait(account, "A");
        ResponseMediaFile b = await api.UploadAndWait(account, "B");
        ResponseMediaFile c = await api.UploadAndWait(account, "C");
        ResponseMediaFile queued = await api.UploadAndWait(account, "Queued");
        await using FrontendClient frontend = await FrontendClient.CreateSignedIn(factory, email);
        await frontend.SettingService.Update(SettingKey.KeepQueuedItems, JsonSerializer.SerializeToElement(keep));
        ResponsePlaylistDetail first = await Playlist(frontend, "First", a.ID, b.ID);
        ResponsePlaylistDetail second = await Playlist(frontend, "Second", c.ID, b.ID);
        await frontend.Control.StartPlaylist(first.ID, first.EntryList[0].ID);
        await frontend.Control.AddToQueue(queued.ID);

        await frontend.Control.StartPlaylist(second.ID, second.EntryList[0].ID);

        ResponseQueue queue = QueueOf(frontend);
        Assert.Equal(c.ID, queue.Current?.MediaFileID);
        Assert.Equal(keep ? [queued.ID] : [], MediaOf(queue.UserQueueList));
        Assert.Equal([b.ID], MediaOf(queue.SourceList));
    }

    [Fact]
    public async Task ADeletedPlayingMediaFile_FinishesThenPreviousSkipsIt()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await api.CreateAccount();
        ResponseMediaFile a = await api.UploadAndWait(account, "A");
        ResponseMediaFile b = await api.UploadAndWait(account, "B");
        ResponseMediaFile c = await api.UploadAndWait(account, "C");
        await using FrontendClient frontend = await FrontendClient.CreateSignedIn(factory, email);
        await frontend.Control.StartLibrary(a.ID, LibrarySortEnum.Title, false, string.Empty);
        await frontend.Control.Next();
        Assert.Equal(b.ID, frontend.Player.Current?.MediaFileID);

        HttpResponseMessage response = await api.Delete(account.Token, b.ID);
        response.EnsureSuccessStatusCode();
        await FrontendClient.WaitFor(async () => (await frontend.Store.GetMediaFileList()).All(x => x.ID != b.ID), "the delete push");

        // It plays on to its end, then the next one follows and the deleted one never entered history.
        Assert.Equal(b.ID, frontend.Player.Current?.MediaFileID);
        Assert.True(frontend.Audio.IsPlaying);
        frontend.Audio.RaiseEnded();
        await FrontendClient.WaitFor(() => IsPlaying(frontend, c.ID), "the next media file");
        Assert.Equal([a.ID], MediaOf(QueueOf(frontend).HistoryList));

        await frontend.Control.Previous();

        Assert.Equal(a.ID, frontend.Player.Current?.MediaFileID);
    }

    [Fact]
    public async Task TheLocalCopyResumesAfterARestart()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await api.CreateAccount();
        ResponseMediaFile a = await api.UploadAndWait(account, "A");
        ResponseMediaFile b = await api.UploadAndWait(account, "B");
        string folder = Path.Combine(Path.GetTempPath(), $"frontend-restart-{Guid.CreateVersion7()}");
        try
        {
            FrontendClient first = FrontendClient.Create(factory, null, folder);
            await using (first)
            {
                await first.Session.SignIn(email, "a-long-enough-password");
                await first.Startup.Start();
                await first.Control.StartLibrary(a.ID, LibrarySortEnum.Title, false, string.Empty);
                await first.Control.Pause();
                await first.Queue.SaveLocalCopy(33);
                await first.Hub.Stop();
            }

            await using FrontendClient restarted = FrontendClient.Create(factory, first.SessionStorage, folder);
            await restarted.Startup.Start();

            Assert.Equal(a.ID, restarted.Queue.Queue.Current?.MediaFileID);
            Assert.True(restarted.Control.CanContinue);
            Assert.False(restarted.Audio.IsPlaying);

            Assert.True(await restarted.Control.Continue());

            Assert.True(restarted.Audio.IsPlaying);
            Assert.Equal(TimeSpan.FromSeconds(33), restarted.Audio.Position);
            Assert.Equal([b.ID], MediaOf(QueueOf(restarted).SourceList));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            try
            {
                if (Directory.Exists(folder)) Directory.Delete(folder, true);
            }
            catch (IOException)
            {
                // A save that was still finishing when the test ended can hold the file for a moment; a temp folder is not worth failing a test for.
            }
        }
    }
}
