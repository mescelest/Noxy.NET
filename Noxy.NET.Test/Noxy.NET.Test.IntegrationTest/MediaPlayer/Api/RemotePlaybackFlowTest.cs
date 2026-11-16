using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class RemotePlaybackFlowTest
{
    private static readonly TimeSpan Short = TimeSpan.FromMilliseconds(500);

    private static ApiFactory Factory(int? graceSeconds = null, double? idleMinutes = null, int? transferSeconds = null)
    {
        Dictionary<string, string?> settings = [];
        if (graceSeconds is { } grace) settings[ConfigurationPath.Combine(ConfigRemote.SectionName, nameof(ConfigRemote.ActiveGraceSeconds))] = grace.ToString();
        if (idleMinutes is { } idle) settings[ConfigurationPath.Combine(ConfigRemote.SectionName, nameof(ConfigRemote.IdleTeardownMinutes))] = idle.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (transferSeconds is { } transfer) settings[ConfigurationPath.Combine(ConfigRemote.SectionName, nameof(ConfigRemote.TransferTimeoutSeconds))] = transfer.ToString();

        return new(settings);
    }

    private static QueueItem Item()
    {
        return new(Guid.CreateVersion7(), Guid.CreateVersion7());
    }

    private static RequestReportPlayerState Report(long revision, bool playing = true, double position = 3)
    {
        return new() { StateRevision = revision, IsPlaying = playing, PositionSeconds = position, Volume = 50, Shuffle = ShuffleModeEnum.Off, Repeat = RepeatModeEnum.No };
    }

    private static RequestClaimActive ClaimRequest(QueueItem? current = null, bool playing = true, long revision = 1)
    {
        return new()
        {
            State = Report(revision, playing),
            Queue = new() { QueueRevision = 0, Source = null, HistoryList = [], Current = current ?? Item(), UserQueueList = [], SourceList = [Item(), Item()] }
        };
    }

    private static async Task<Guid> UploadMediaFile(MediaApiClient api, ApiAccount user)
    {
        HttpResponseMessage response = await api.Upload(user.Token, MediaApiClient.Mp3(301), "Media 1.mp3", "Media 1");
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        await api.WaitForMediaFileList(user.Token, 1);
        List<ResponseMediaFile> list = await api.GetMediaFileList(user.Token);
        return Assert.Single(list).ID;
    }

    private static string Json(ResponseQueue? queue)
    {
        return JsonSerializer.Serialize(queue, JsonSerializerOptions.Web);
    }

    #region -- Queue and state --

    [Fact]
    public async Task TheSecondDeviceSeesTheFirstDevicesQueueAndStateAfterItClaims()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient first = await HubClient.Connect(factory, user.Token, "Laptop");
        await using HubClient second = await HubClient.Connect(factory, user.Token, "Phone");
        await second.Settle();
        QueueItem current = Item();

        await first.Claim(ClaimRequest(current));

        ResponseQueue snapshot = await second.Next<ResponseQueue>(MediaHubEventEnum.QueueSnapshot);
        ResponsePlayerState state = await second.Next<ResponsePlayerState>(MediaHubEventEnum.PlayerStateChanged);
        Assert.Equal(current, snapshot.Current);
        Assert.Equal(first.DeviceID, state.ActiveDeviceID);
        Assert.Equal(first.DeviceID, Assert.Single(await second.Next<List<ResponseDevice>>(MediaHubEventEnum.DeviceListChanged), x => x.IsActive).DeviceID);
    }

    [Fact]
    public async Task APlayNextFromTheSecondDevice_ReachesTheFirstAsADelta_AndBothSnapshotsAgree()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        Guid mediaFileID = await UploadMediaFile(api, user);
        await using HubClient first = await HubClient.Connect(factory, user.Token, "Laptop");
        await using HubClient second = await HubClient.Connect(factory, user.Token, "Phone");
        await first.Claim(ClaimRequest());
        await first.Settle();
        Guid itemID = Guid.CreateVersion7();

        await second.Edit(new() { Edit = QueueEditEnum.PlayNext, ItemID = itemID, MediaFileID = mediaFileID });

        ResponseQueueDelta delta = await first.Next<ResponseQueueDelta>(MediaHubEventEnum.QueueChanged);
        Assert.Equal(itemID, delta.ItemID);
        Assert.Equal(Json(await first.RequestSnapshot()), Json(await second.RequestSnapshot()));
    }

    [Fact]
    public async Task TheActiveDevicesReplaceSource_IsSeenByTheOther()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        Guid mediaFileID = await UploadMediaFile(api, user);
        await using HubClient active = await HubClient.Connect(factory, user.Token, "Laptop");
        await using HubClient other = await HubClient.Connect(factory, user.Token, "Phone");
        await active.Claim(ClaimRequest());
        await other.Settle();
        QueueItem[] itemList = [new(Guid.CreateVersion7(), mediaFileID), new(Guid.CreateVersion7(), mediaFileID), new(Guid.CreateVersion7(), mediaFileID)];

        await active.Edit(new()
        {
            Edit = QueueEditEnum.ReplaceSource,
            Source = new() { Kind = QueueSourceKindEnum.Library, PlaylistID = null, Sort = LibrarySortEnum.Title, Descending = false, Search = string.Empty },
            ItemList = [.. itemList],
            CurrentItemID = itemList[0].ItemID
        });

        ResponseQueueDelta delta = await other.Next<ResponseQueueDelta>(MediaHubEventEnum.QueueChanged);
        Assert.Equal(QueueEditEnum.ReplaceSource, delta.Edit);
        ResponseQueue? snapshot = await other.RequestSnapshot();
        Assert.Equal(itemList[0], snapshot?.Current);
        Assert.Equal(itemList[1..], snapshot?.SourceList);
    }

    [Fact]
    public async Task AMissedRevision_IsRecoveredByRequestSnapshot()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        Guid mediaFileID = await UploadMediaFile(api, user);
        await using HubClient active = await HubClient.Connect(factory, user.Token, "Laptop");
        await using HubClient controller = await HubClient.Connect(factory, user.Token, "Phone");
        await active.Claim(ClaimRequest());
        await controller.Settle();

        await active.Edit(new() { Edit = QueueEditEnum.AddToQueue, ItemID = Guid.CreateVersion7(), MediaFileID = mediaFileID });
        await active.Edit(new() { Edit = QueueEditEnum.AddToQueue, ItemID = Guid.CreateVersion7(), MediaFileID = mediaFileID });
        ResponseQueueDelta first = await controller.Next<ResponseQueueDelta>(MediaHubEventEnum.QueueChanged);
        ResponseQueueDelta second = await controller.Next<ResponseQueueDelta>(MediaHubEventEnum.QueueChanged);

        // A client that only saw the second delta knows it missed one, and asks for the whole queue.
        Assert.Equal(first.QueueRevision + 1, second.QueueRevision);
        ResponseQueue? snapshot = await controller.RequestSnapshot();
        Assert.Equal(second.QueueRevision, snapshot?.QueueRevision);
        Assert.Equal(2, snapshot?.UserQueueList.Count);
    }

    #endregion -- Queue and state --

    #region -- Commands --

    [Fact]
    public async Task ACommandFromTheSecondDevice_ReachesTheActiveDeviceOnly()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient active = await HubClient.Connect(factory, user.Token, "Laptop");
        await using HubClient controller = await HubClient.Connect(factory, user.Token, "Phone");
        await using HubClient bystander = await HubClient.Connect(factory, user.Token, "Tablet");
        await active.Claim(ClaimRequest());
        await active.Settle();
        await controller.Settle();
        await bystander.Settle();

        CommandResultEnum result = await controller.SendCommand(new() { Command = PlayerCommandEnum.Seek, PositionSeconds = 12 });

        Assert.Equal(CommandResultEnum.Sent, result);
        RequestPlayerCommand received = await active.Next<RequestPlayerCommand>(MediaHubEventEnum.PlayerCommandReceived);
        Assert.Equal(PlayerCommandEnum.Seek, received.Command);
        Assert.Equal(12, received.PositionSeconds);
        Assert.True(await bystander.ReceivesNone(MediaHubEventEnum.PlayerCommandReceived, Short));
        Assert.True(await controller.ReceivesNone(MediaHubEventEnum.PlayerCommandReceived, Short));
    }

    [Fact]
    public async Task ACommandWithNoActiveDevice_IsAnsweredNoActiveDevice_AndAnInvalidOneIsInvalid()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient controller = await HubClient.Connect(factory, user.Token);

        Assert.Equal(CommandResultEnum.NoActiveDevice, await controller.SendCommand(new() { Command = PlayerCommandEnum.Play }));
        Assert.Equal(CommandResultEnum.Invalid, await controller.SendCommand(new() { Command = PlayerCommandEnum.Seek }));
    }

    #endregion -- Commands --

    #region -- Transfer --

    [Fact]
    public async Task TransferMovesTheActiveDevice_WithTheQueue()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient old = await HubClient.Connect(factory, user.Token, "Laptop");
        await using HubClient target = await HubClient.Connect(factory, user.Token, "Phone");
        QueueItem current = Item();
        await old.Claim(ClaimRequest(current));
        await old.Settle();
        await target.Settle();

        Assert.True(await old.Transfer(target.DeviceID));

        await old.Next(MediaHubEventEnum.PlayerTransferRequested);
        await old.Report(Report(2, false, 42));
        (ResponsePlayerState state, ResponseQueue queue) = await target.Next<(ResponsePlayerState, ResponseQueue)>(MediaHubEventEnum.PlayerStartRequested);
        Assert.Equal(42, state.PositionSeconds);
        Assert.Equal(current, queue.Current);
        Assert.True(await target.Claim(new() { State = Report(1, true, state.PositionSeconds), Queue = queue }));
        ResponsePlayerState moved = await old.Next<ResponsePlayerState>(MediaHubEventEnum.PlayerStateChanged);
        while (moved.ActiveDeviceID != target.DeviceID) moved = await old.Next<ResponsePlayerState>(MediaHubEventEnum.PlayerStateChanged);

        Assert.Equal(target.DeviceID, moved.ActiveDeviceID);
    }

    [Fact]
    public async Task ATransferWhoseTargetNeverClaims_PushesTransferFailed_AndTheOldDeviceStaysActive()
    {
        await using ApiFactory factory = Factory(transferSeconds: 1);
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient old = await HubClient.Connect(factory, user.Token, "Laptop");
        await using HubClient target = await HubClient.Connect(factory, user.Token, "Phone");
        await old.Claim(ClaimRequest());
        await old.Settle();

        Assert.True(await old.Transfer(target.DeviceID));
        await old.Next(MediaHubEventEnum.PlayerTransferRequested);
        await old.Report(Report(2, false, 42));
        await target.Next<(ResponsePlayerState, ResponseQueue)>(MediaHubEventEnum.PlayerStartRequested);

        await old.Next(MediaHubEventEnum.TransferFailed);
        await target.Next(MediaHubEventEnum.TransferFailed);
        Assert.Equal(old.DeviceID, (await target.Next<ResponsePlayerState>(MediaHubEventEnum.PlayerStateChanged)).ActiveDeviceID);
    }

    #endregion -- Transfer --

    #region -- Reconnecting, dropping and teardown --

    [Fact]
    public async Task AReconnectingPlayingDevice_IsRefusedWhileAnotherIsPlaying_AndAcceptedWhenNoneIs()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient winner = await HubClient.Connect(factory, user.Token, "Laptop");
        await using HubClient reconnecting = await HubClient.Connect(factory, user.Token, "Phone");
        await winner.Claim(ClaimRequest());

        Assert.False(await reconnecting.Claim(ClaimRequest()));

        await winner.Report(Report(2, false));
        Assert.True(await reconnecting.Claim(ClaimRequest()));
    }

    [Fact]
    public async Task TheActiveDeviceDropping_ClearsTheActiveRecordAfterTheGracePeriod_AndTheQueueSurvives()
    {
        await using ApiFactory factory = Factory(graceSeconds: 1);
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient active = await HubClient.Connect(factory, user.Token, "Laptop");
        await using HubClient watcher = await HubClient.Connect(factory, user.Token, "Phone");
        QueueItem current = Item();
        await active.Claim(ClaimRequest(current));
        await watcher.Settle();

        await active.Stop();

        ResponsePlayerState state = await watcher.Next<ResponsePlayerState>(MediaHubEventEnum.PlayerStateChanged);
        Assert.Null(state.ActiveDeviceID);
        Assert.False(state.IsPlaying);
        Assert.Equal(current, (await watcher.RequestSnapshot())?.Current);
    }

    [Fact]
    public async Task TheSessionEndsAfterTheIdleTime_AndEveryDeviceGetsSessionEnded_AndNothingIsReadableAfter()
    {
        await using ApiFactory factory = Factory(idleMinutes: 0.02);
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient active = await HubClient.Connect(factory, user.Token, "Laptop");
        await using HubClient other = await HubClient.Connect(factory, user.Token, "Phone");
        await active.Claim(ClaimRequest(playing: false));

        await active.Next(MediaHubEventEnum.SessionEnded);
        await other.Next(MediaHubEventEnum.SessionEnded);

        Assert.Null(await other.RequestSnapshot());
        Assert.True(await other.Claim(ClaimRequest()));
    }

    [Fact]
    public async Task ARestartedRegistry_IsRebuiltByTheDevicesRegisteringAgain()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        Guid deviceID = Guid.CreateVersion7();
        await using HubClient observer = await HubClient.Connect(factory, user.Token, "Phone");
        HubClient before = await HubClient.Connect(factory, user.Token, "Laptop", deviceID);
        await before.Stop();
        await before.DisposeAsync();

        await using HubClient after = await HubClient.Connect(factory, user.Token, "Laptop", deviceID);

        List<ResponseDevice> list = await after.Register();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, x => x.DeviceID == deviceID);
    }

    #endregion -- Reconnecting, dropping and teardown --

    #region -- Isolation and access --

    [Fact]
    public async Task AnotherAccountsDevice_NeverReceivesAnything()
    {
        await using ApiFactory factory = Factory();
        MediaApiClient api = new(factory);
        ApiAccount one = await api.SignUp("a@example.com");
        ApiAccount two = await api.SignUp("b@example.com");
        await using HubClient active = await HubClient.Connect(factory, one.Token, "Laptop");
        await using HubClient controller = await HubClient.Connect(factory, one.Token, "Phone");
        await using HubClient stranger = await HubClient.Connect(factory, two.Token, "Stranger");
        await stranger.Settle();

        await active.Claim(ClaimRequest());
        await controller.SendCommand(new() { Command = PlayerCommandEnum.Pause });
        await active.Report(Report(2, false));

        Assert.True(await stranger.ReceivesNothing(Short));
        Assert.Equal(CommandResultEnum.NoActiveDevice, await stranger.SendCommand(new() { Command = PlayerCommandEnum.Play }));
        Assert.False(await stranger.Transfer(active.DeviceID));
        Assert.Null(await stranger.RequestSnapshot());
    }

    [Fact]
    public async Task AnAnonymousHubConnection_IsRejected()
    {
        await using ApiFactory factory = Factory();

        await Assert.ThrowsAnyAsync<Exception>(() => HubClient.Connect(factory, null));
    }

    #endregion -- Isolation and access --
}
