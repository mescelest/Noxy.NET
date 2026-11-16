using System.Net;
using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class MediaHubRemoteTest
{
    private static readonly TimeSpan Short = TimeSpan.FromMilliseconds(400);

    private static QueueItem Item()
    {
        return new(Guid.CreateVersion7(), Guid.CreateVersion7());
    }

    private static RequestReportPlayerState Report(long revision, bool playing = true)
    {
        return new() { StateRevision = revision, IsPlaying = playing, PositionSeconds = 3, Volume = 50, Shuffle = ShuffleModeEnum.Off, Repeat = RepeatModeEnum.No };
    }

    private static RequestClaimActive ClaimRequest(QueueItem? current = null, bool playing = true, long revision = 1, int sourceCount = 2)
    {
        return new()
        {
            State = Report(revision, playing),
            Queue = new() { QueueRevision = 0, Source = null, HistoryList = [], Current = current ?? Item(), UserQueueList = [], SourceList = [.. Enumerable.Range(0, sourceCount).Select(_ => Item())] }
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

    [Fact]
    public async Task RegisterDevice_RegistersWithTheAuthenticatedUser_AndReturnsTheList()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient first = await HubClient.Connect(factory, user.Token, "Laptop");
        await first.Settle();

        await using HubClient second = await HubClient.Connect(factory, user.Token, "Phone");

        List<ResponseDevice> list = await second.Register();
        Assert.Equal(["Laptop", "Phone"], list.Select(x => x.Name).Order());
        List<ResponseDevice> pushed = await first.Next<List<ResponseDevice>>(MediaHubEventEnum.DeviceListChanged);
        while (pushed.Count != 2) pushed = await first.Next<List<ResponseDevice>>(MediaHubEventEnum.DeviceListChanged);

        Assert.Equal(2, pushed.Count);
    }

    [Fact]
    public async Task RegisterDevice_WithAnInvalidRequest_ThrowsHubException()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient client = await HubClient.Connect(factory, user.Token, "  ", register: false);

        await Assert.ThrowsAsync<HubException>(() => client.Register());
    }

    [Fact]
    public async Task RegisterDevice_WithASession_SendsTheCallerTheSnapshotAndTheState()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient active = await HubClient.Connect(factory, user.Token, "Laptop");
        QueueItem current = Item();
        Assert.True(await active.Claim(ClaimRequest(current)));

        await using HubClient joining = await HubClient.Connect(factory, user.Token, "Phone");

        ResponseQueue snapshot = await joining.Next<ResponseQueue>(MediaHubEventEnum.QueueSnapshot);
        ResponsePlayerState state = await joining.Next<ResponsePlayerState>(MediaHubEventEnum.PlayerStateChanged);
        Assert.Equal(current, snapshot.Current);
        Assert.Equal(active.DeviceID, state.ActiveDeviceID);
        Assert.True(state.IsPlaying);
    }

    [Fact]
    public async Task Disconnect_RemovesTheDevice()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient first = await HubClient.Connect(factory, user.Token, "Laptop");
        await using HubClient second = await HubClient.Connect(factory, user.Token, "Phone");
        await first.Settle();

        await second.Stop();

        List<ResponseDevice> list = await first.Next<List<ResponseDevice>>(MediaHubEventEnum.DeviceListChanged);
        while (list.Count != 1) list = await first.Next<List<ResponseDevice>>(MediaHubEventEnum.DeviceListChanged);

        Assert.Equal(first.DeviceID, Assert.Single(list).DeviceID);
    }

    [Fact]
    public async Task ClaimActive_Accepted_ReturnsTrue_AndRefused_ReturnsFalse()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient first = await HubClient.Connect(factory, user.Token, "Laptop");
        await using HubClient second = await HubClient.Connect(factory, user.Token, "Phone");

        Assert.True(await first.Claim(ClaimRequest()));
        Assert.False(await second.Claim(ClaimRequest()));
    }

    [Fact]
    public async Task TheHubAcceptsAClaimWithA20000ItemQueue()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient client = await HubClient.Connect(factory, user.Token);
        await using HubClient watcher = await HubClient.Connect(factory, user.Token, "Phone");
        await watcher.Settle();

        Assert.True(await client.Claim(ClaimRequest(sourceCount: 19_990)));

        ResponseQueue snapshot = await watcher.Next<ResponseQueue>(MediaHubEventEnum.QueueSnapshot);
        Assert.Equal(19_990, snapshot.SourceList.Count);
        Assert.Equal(19_990, (await watcher.RequestSnapshot())?.SourceList.Count);
    }

    [Fact]
    public async Task ReportState_FromTheActiveDevice_IsStoredAndPushed()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient active = await HubClient.Connect(factory, user.Token, "Laptop");
        await using HubClient watcher = await HubClient.Connect(factory, user.Token, "Phone");
        await active.Claim(ClaimRequest());
        await watcher.Settle();

        await active.Report(Report(2, false));

        ResponsePlayerState state = await watcher.Next<ResponsePlayerState>(MediaHubEventEnum.PlayerStateChanged);
        Assert.False(state.IsPlaying);
        Assert.Equal(2, state.StateRevision);
    }

    [Fact]
    public async Task ReportState_FromANonActiveDevice_IsIgnored()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient active = await HubClient.Connect(factory, user.Token, "Laptop");
        await using HubClient other = await HubClient.Connect(factory, user.Token, "Phone");
        await active.Claim(ClaimRequest());
        await active.Settle();

        await other.Report(Report(9, false));

        Assert.True(await active.ReceivesNone(MediaHubEventEnum.PlayerStateChanged, Short));
    }

    [Fact]
    public async Task ReportState_WithAnInvalidVolume_ThrowsHubException()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient active = await HubClient.Connect(factory, user.Token);
        await active.Claim(ClaimRequest());
        RequestReportPlayerState bad = new() { StateRevision = 2, IsPlaying = true, PositionSeconds = 1, Volume = 500, Shuffle = ShuffleModeEnum.Off, Repeat = RepeatModeEnum.No };

        await Assert.ThrowsAsync<HubException>(() => active.Report(bad));
    }

    [Fact]
    public async Task EditQueue_FromAnyDevice_AppliesAPlayNext()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        Guid mediaFileID = await UploadMediaFile(api, user);
        await using HubClient active = await HubClient.Connect(factory, user.Token, "Laptop");
        await using HubClient controller = await HubClient.Connect(factory, user.Token, "Phone");
        await active.Claim(ClaimRequest());
        await active.Settle();
        Guid itemID = Guid.CreateVersion7();

        await controller.Edit(new() { Edit = QueueEditEnum.PlayNext, ItemID = itemID, MediaFileID = mediaFileID });

        ResponseQueueDelta delta = await active.Next<ResponseQueueDelta>(MediaHubEventEnum.QueueChanged);
        Assert.Equal(QueueEditEnum.PlayNext, delta.Edit);
        Assert.Equal(itemID, delta.ItemID);
        Assert.Equal(itemID, Assert.Single((await controller.RequestSnapshot())?.UserQueueList ?? []).ItemID);
    }

    [Fact]
    public async Task EditQueue_ActiveOnlyKind_FromAnotherDevice_ThrowsHubException()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient active = await HubClient.Connect(factory, user.Token, "Laptop");
        await using HubClient other = await HubClient.Connect(factory, user.Token, "Phone");
        await active.Claim(ClaimRequest());

        await Assert.ThrowsAsync<HubException>(() => other.Edit(new() { Edit = QueueEditEnum.Advance }));
    }

    [Fact]
    public async Task EditQueue_WithNoSession_ThrowsHubException()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient client = await HubClient.Connect(factory, user.Token);

        await Assert.ThrowsAsync<HubException>(() => client.Edit(new() { Edit = QueueEditEnum.Remove, ItemID = Guid.CreateVersion7() }));
    }

    [Fact]
    public async Task EditQueue_WithAnInvalidEdit_ThrowsHubException()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient client = await HubClient.Connect(factory, user.Token);
        await client.Claim(ClaimRequest());

        await Assert.ThrowsAsync<HubException>(() => client.Edit(new() { Edit = QueueEditEnum.PlayNext }));
    }

    [Fact]
    public async Task RequestSnapshot_ReturnsTheQueue_OrNullWithNoSession()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient client = await HubClient.Connect(factory, user.Token);
        Assert.Null(await client.RequestSnapshot());
        QueueItem current = Item();
        await client.Claim(ClaimRequest(current));

        ResponseQueue? snapshot = await client.RequestSnapshot();

        Assert.Equal(current, snapshot?.Current);
    }

    [Fact]
    public async Task ACallFromAnUnregisteredConnection_IsRefused()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using HubClient client = await HubClient.Connect(factory, user.Token, register: false);

        await Assert.ThrowsAsync<HubException>(() => client.Claim(ClaimRequest()));
        await Assert.ThrowsAsync<HubException>(() => client.Report(Report(1)));
        await Assert.ThrowsAsync<HubException>(() => client.Edit(new() { Edit = QueueEditEnum.Advance }));
        await Assert.ThrowsAsync<HubException>(() => client.RequestSnapshot());
    }

    [Fact]
    public async Task AnotherAccountsDevice_NeverReceivesTheQueueOrTheState()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount one = await api.SignUp("a@example.com");
        ApiAccount two = await api.SignUp("b@example.com");
        await using HubClient active = await HubClient.Connect(factory, one.Token);
        await using HubClient stranger = await HubClient.Connect(factory, two.Token);
        await stranger.Settle();

        await active.Claim(ClaimRequest());

        Assert.True(await stranger.ReceivesNothing(Short));
        Assert.Null(await stranger.RequestSnapshot());
    }
}
