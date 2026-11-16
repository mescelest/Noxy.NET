using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Notifications.Remote;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Remote;

public class LiveSessionServiceTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly FakeStore _store = new();
    private readonly ConfigRemote _config = new() { ActiveGraceSeconds = 10, IdleTeardownMinutes = 10, HistoryLimit = 50, MaxQueueLength = 100, TransferTimeoutSeconds = 5 };
    private readonly FakeUnitOfWorkFactory _factory;
    private readonly DeviceRegistryService _registry;
    private readonly LiveSessionService _service;

    public LiveSessionServiceTest()
    {
        _registry = new(_publisher, Options.Create(_config));
        _factory = new(_store);
        _service = new(_registry, Options.Create(_config), _time, _publisher, _factory, NullLogger<LiveSessionService>.Instance);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private async Task<Guid> Device(Guid? userID = null)
    {
        Guid deviceID = Guid.CreateVersion7();
        await _registry.Register(userID ?? UserID, new() { DeviceID = deviceID, Name = "Device" }, $"c-{deviceID}");
        return deviceID;
    }

    private static QueueItem Item()
    {
        return new(Guid.CreateVersion7(), Guid.CreateVersion7());
    }

    private static RequestReportPlayerState Report(long revision, bool playing = true, double position = 5)
    {
        return new() { StateRevision = revision, IsPlaying = playing, PositionSeconds = position, Volume = 40, Shuffle = ShuffleModeEnum.Off, Repeat = RepeatModeEnum.No };
    }

    private static ResponseQueue QueueOf(QueueItem? current, params QueueItem[] sourceList)
    {
        return new() { QueueRevision = 0, Source = null, HistoryList = [], Current = current, UserQueueList = [], SourceList = [.. sourceList] };
    }

    private static RequestClaimActive ClaimRequest(bool playing = true, long revision = 1, ResponseQueue? queue = null)
    {
        return new() { State = Report(revision, playing), Queue = queue ?? QueueOf(Item(), Item()) };
    }

    private Guid SeedMedia(Guid? userID = null)
    {
        EntityMediaFile file = new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new string('a', 64), UserID = userID ?? UserID };
        _store.MediaFileList.Add(file);
        return file.ID;
    }

    private async Task<Guid> ActiveSession(bool playing = true, ResponseQueue? queue = null)
    {
        Guid deviceID = await Device();
        Assert.True(await _service.Claim(UserID, deviceID, ClaimRequest(playing, 1, queue)));
        _publisher.PublishedList.Clear();
        return deviceID;
    }

    private static QueueEdit PlayNext(Guid mediaFileID)
    {
        return new() { Edit = QueueEditEnum.PlayNext, ItemID = Guid.CreateVersion7(), MediaFileID = mediaFileID };
    }

    #region -- Claim --

    [Fact]
    public async Task Claim_WithNoSession_CreatesOne_StoresTheQueueAndState()
    {
        Guid deviceID = await Device();
        QueueItem current = Item();
        QueueItem next = Item();

        bool accepted = await _service.Claim(UserID, deviceID, ClaimRequest(true, 7, QueueOf(current, next)));

        Assert.True(accepted);
        Assert.Equal(deviceID, _service.GetActiveDeviceID(UserID));
        ResponseQueue? queue = _service.GetQueue(UserID);
        Assert.Equal(current, queue?.Current);
        Assert.Equal([next], queue?.SourceList);
        ResponsePlayerState? state = _service.GetState(UserID);
        Assert.Equal(deviceID, state?.ActiveDeviceID);
        Assert.True(state?.IsPlaying);
        Assert.Equal(7, state?.StateRevision);
        Assert.Equal(40, state?.Volume);
        Assert.Equal(_time.Now.UtcDateTime, state?.ServerTime);
    }

    [Fact]
    public async Task Claim_PublishesTheStateTheSnapshotAndTheDeviceList()
    {
        Guid deviceID = await Device();
        _publisher.PublishedList.Clear();

        await _service.Claim(UserID, deviceID, ClaimRequest());

        Assert.Equal(UserID, Assert.Single(_publisher.GetList<NotificationPlayerStateChanged>()).UserID);
        Assert.Equal(UserID, Assert.Single(_publisher.GetList<NotificationQueueSnapshot>()).UserID);
        NotificationDeviceListChanged list = Assert.Single(_publisher.GetList<NotificationDeviceListChanged>());
        Assert.Equal(deviceID, Assert.Single(list.DeviceList, x => x.IsActive).DeviceID);
    }

    [Fact]
    public async Task Claim_WhileAnotherOnlineDeviceIsActiveAndPlaying_IsRefused_AndChangesNothing()
    {
        Guid first = await ActiveSession(true);
        Guid second = await Device();
        ResponseQueue before = _service.GetQueue(UserID) ?? throw new InvalidOperationException("No queue.");
        _publisher.PublishedList.Clear();

        bool accepted = await _service.Claim(UserID, second, ClaimRequest(true, 1, QueueOf(Item(), Item())));

        Assert.False(accepted);
        Assert.Equal(first, _service.GetActiveDeviceID(UserID));
        Assert.Equal(before.Current, _service.GetQueue(UserID)?.Current);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Claim_WhileTheActiveDeviceIsPaused_IsAccepted_AndItsQueueReplacesTheSessions()
    {
        await ActiveSession(false);
        Guid second = await Device();
        QueueItem current = Item();

        bool accepted = await _service.Claim(UserID, second, ClaimRequest(true, 1, QueueOf(current)));

        Assert.True(accepted);
        Assert.Equal(second, _service.GetActiveDeviceID(UserID));
        Assert.Equal(current, _service.GetQueue(UserID)?.Current);
    }

    [Fact]
    public async Task Claim_WhileTheActiveDeviceIsOffline_OrInItsGracePeriod_IsAccepted()
    {
        Guid first = await ActiveSession(true);
        await _registry.Disconnect($"c-{first}");
        Guid second = await Device();

        bool accepted = await _service.Claim(UserID, second, ClaimRequest());

        Assert.True(accepted);
        Assert.Equal(second, _service.GetActiveDeviceID(UserID));
    }

    [Fact]
    public async Task Claim_FromAnUnregisteredDevice_IsRefused()
    {
        bool accepted = await _service.Claim(UserID, Guid.CreateVersion7(), ClaimRequest());

        Assert.False(accepted);
        Assert.Null(_service.GetQueue(UserID));
        Assert.Null(_service.GetActiveDeviceID(UserID));
    }

    [Fact]
    public async Task TwoClaimsAtOnce_LeaveExactlyOneActiveDevice()
    {
        for (int round = 0; round < 50; round++)
        {
            Guid userID = Guid.CreateVersion7();
            Guid one = await Device(userID);
            Guid two = await Device(userID);

            bool[] results = await Task.WhenAll(Task.Run(() => _service.Claim(userID, one, ClaimRequest()), Token), Task.Run(() => _service.Claim(userID, two, ClaimRequest()), Token));

            Assert.Single(results, x => x);
            Guid? active = _service.GetActiveDeviceID(userID);
            Assert.True(active == one || active == two);
        }
    }

    [Fact]
    public async Task Claim_ResetsTheStateRevision()
    {
        Guid first = await ActiveSession(false);
        await _service.Report(UserID, first, Report(50, false));
        Guid second = await Device();

        await _service.Claim(UserID, second, ClaimRequest(true, 1));

        Assert.True(await _service.Report(UserID, second, Report(2)));
    }

    [Fact]
    public async Task Claim_RaisesTheQueueRevision_SoAClientSeesTheReplacement()
    {
        Guid first = await ActiveSession(false);
        await _service.Edit(UserID, first, PlayNext(SeedMedia()), Token);
        long before = _service.GetQueue(UserID)?.QueueRevision ?? 0;
        Guid second = await Device();

        await _service.Claim(UserID, second, ClaimRequest(true, 1));

        Assert.True(_service.GetQueue(UserID)?.QueueRevision > before);
    }

    [Fact]
    public async Task Claim_WithAQueueLongerThanTheLimit_Throws_AndChangesNothing()
    {
        Guid deviceID = await Device();
        ResponseQueue queue = QueueOf(null, [.. Enumerable.Range(0, _config.MaxQueueLength + 1).Select(_ => Item())]);

        await Assert.ThrowsAsync<InvalidQueueEditException>(() => _service.Claim(UserID, deviceID, ClaimRequest(true, 1, queue)));

        Assert.Null(_service.GetQueue(UserID));
    }

    #endregion -- Claim --

    #region -- Report --

    [Fact]
    public async Task Report_FromTheActiveDevice_StoresIt_AndPublishesOnce()
    {
        Guid deviceID = await ActiveSession();
        _time.Now = _time.Now.AddSeconds(3);

        bool accepted = await _service.Report(UserID, deviceID, Report(2, false, 12.5));

        Assert.True(accepted);
        ResponsePlayerState? state = _service.GetState(UserID);
        Assert.False(state?.IsPlaying);
        Assert.Equal(12.5, state?.PositionSeconds);
        Assert.Equal(_time.Now.UtcDateTime, state?.ServerTime);
        Assert.Single(_publisher.GetList<NotificationPlayerStateChanged>());
    }

    [Fact]
    public async Task Report_FromAnotherDevice_IsRejected_AndPublishesNothing()
    {
        await ActiveSession();
        Guid other = await Device();
        _publisher.PublishedList.Clear();

        bool accepted = await _service.Report(UserID, other, Report(5));

        Assert.False(accepted);
        Assert.Empty(_publisher.PublishedList);
        Assert.Equal(1, _service.GetState(UserID)?.StateRevision);
    }

    [Fact]
    public async Task Report_WithALowerOrEqualRevision_IsDropped()
    {
        Guid deviceID = await ActiveSession();
        await _service.Report(UserID, deviceID, Report(5, true, 50));
        _publisher.PublishedList.Clear();

        Assert.False(await _service.Report(UserID, deviceID, Report(5, true, 99)));
        Assert.False(await _service.Report(UserID, deviceID, Report(4, true, 99)));

        Assert.Equal(50, _service.GetState(UserID)?.PositionSeconds);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Report_WithNoSession_IsRejected()
    {
        Guid deviceID = await Device();

        Assert.False(await _service.Report(UserID, deviceID, Report(1)));
    }

    #endregion -- Report --

    #region -- Edit --

    [Fact]
    public async Task Edit_AnyDevice_PlayNextAddToQueueRemoveMove_IsAppliedAndPublishedAsADelta()
    {
        await ActiveSession();
        Guid controller = await Device();
        Guid mediaFileID = SeedMedia();
        QueueEdit playNext = PlayNext(mediaFileID);

        ResponseQueueDelta? delta = await _service.Edit(UserID, controller, playNext, Token);

        Assert.NotNull(delta);
        Assert.Equal(QueueEditEnum.PlayNext, delta.Edit);
        Assert.Equal(playNext.ItemID, Assert.Single(_service.GetQueue(UserID)?.UserQueueList ?? []).ItemID);
        NotificationQueueChanged pushed = Assert.Single(_publisher.GetList<NotificationQueueChanged>());
        Assert.Equal(UserID, pushed.UserID);
        Assert.Equal(delta.QueueRevision, pushed.Delta.QueueRevision);

        ResponseQueueDelta? added = await _service.Edit(UserID, controller, new() { Edit = QueueEditEnum.AddToQueue, ItemID = Guid.CreateVersion7(), MediaFileID = SeedMedia() }, Token);
        ResponseQueueDelta? moved = await _service.Edit(UserID, controller, new() { Edit = QueueEditEnum.Move, ItemID = playNext.ItemID, AfterItemID = added?.ItemID }, Token);
        ResponseQueueDelta? removed = await _service.Edit(UserID, controller, new() { Edit = QueueEditEnum.Remove, ItemID = added?.ItemID }, Token);

        Assert.Equal([2, 3, 4], new[] { added?.QueueRevision, moved?.QueueRevision, removed?.QueueRevision }.Select(x => (int)(x ?? 0)));
    }

    [Fact]
    public async Task Edit_ActiveOnlyKinds_FromAnotherDevice_AreRejected()
    {
        await ActiveSession();
        Guid other = await Device();
        long revision = _service.GetQueue(UserID)?.QueueRevision ?? 0;
        _publisher.PublishedList.Clear();

        foreach (QueueEdit edit in new List<QueueEdit>
                 {
                     new() { Edit = QueueEditEnum.Advance },
                     new() { Edit = QueueEditEnum.Previous, Item = Item() },
                     new() { Edit = QueueEditEnum.RegenerateSource, ItemList = [] },
                     new() { Edit = QueueEditEnum.RemoveSourceItem, ItemID = Guid.CreateVersion7() },
                     new() { Edit = QueueEditEnum.InsertSourceItem, Item = Item() },
                     new() { Edit = QueueEditEnum.ReplaceSource, Source = new() { Kind = QueueSourceKindEnum.Library, PlaylistID = null, Sort = LibrarySortEnum.Title, Descending = false, Search = string.Empty }, ItemList = [] }
                 })
        {
            await Assert.ThrowsAsync<InvalidQueueEditException>(() => _service.Edit(UserID, other, edit, Token));
        }

        Assert.Equal(revision, _service.GetQueue(UserID)?.QueueRevision);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Edit_AdvanceFromTheActiveDevice_Works()
    {
        QueueItem next = Item();
        Guid active = await ActiveSession(true, QueueOf(Item(), next));

        ResponseQueueDelta? delta = await _service.Edit(UserID, active, new() { Edit = QueueEditEnum.Advance, Item = next }, Token);

        Assert.NotNull(delta);
        Assert.Equal(next, _service.GetQueue(UserID)?.Current);
    }

    [Fact]
    public async Task Edit_WithNoSession_ReturnsNull()
    {
        Guid deviceID = await Device();

        Assert.Null(await _service.Edit(UserID, deviceID, PlayNext(SeedMedia()), Token));
        Assert.Empty(_publisher.GetList<NotificationQueueChanged>());
    }

    [Fact]
    public async Task Edit_WithAMediaFileOfAnotherUser_Throws404_AndChangesNothing()
    {
        Guid active = await ActiveSession();
        Guid foreign = SeedMedia(Guid.CreateVersion7());
        long revision = _service.GetQueue(UserID)?.QueueRevision ?? 0;

        await Assert.ThrowsAsync<MediaNotFoundException>(() => _service.Edit(UserID, active, PlayNext(foreign), Token));
        await Assert.ThrowsAsync<MediaNotFoundException>(() => _service.Edit(UserID, active, PlayNext(Guid.CreateVersion7()), Token));

        Assert.Equal(revision, _service.GetQueue(UserID)?.QueueRevision);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Edit_OfAGoneItem_PublishesNothing()
    {
        Guid active = await ActiveSession();

        ResponseQueueDelta? delta = await _service.Edit(UserID, active, new() { Edit = QueueEditEnum.Remove, ItemID = Guid.CreateVersion7() }, Token);

        Assert.Null(delta);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Edit_PastTheQueueLimit_Throws()
    {
        QueueItem[] sourceList = [.. Enumerable.Range(0, _config.MaxQueueLength).Select(_ => Item())];
        Guid active = await ActiveSession(true, QueueOf(null, sourceList));

        await Assert.ThrowsAsync<InvalidQueueEditException>(() => _service.Edit(UserID, active, PlayNext(SeedMedia()), Token));
    }

    #endregion -- Edit --

    #region -- Dropped active device and teardown --

    [Fact]
    public async Task TheActiveDeviceDropping_ClearsTheActiveRecordAfterTheGracePeriod_AndKeepsTheQueue()
    {
        Guid active = await ActiveSession();
        QueueItem? current = _service.GetQueue(UserID)?.Current;
        await _registry.Disconnect($"c-{active}");
        _publisher.PublishedList.Clear();

        _time.Advance(TimeSpan.FromSeconds(9));
        Assert.Equal(active, _service.GetActiveDeviceID(UserID));
        _time.Advance(TimeSpan.FromSeconds(2));

        Assert.Null(_service.GetActiveDeviceID(UserID));
        Assert.False(_service.GetState(UserID)?.IsPlaying);
        Assert.Equal(current, _service.GetQueue(UserID)?.Current);
        Assert.Null(Assert.Single(_publisher.GetList<NotificationPlayerStateChanged>()).State.ActiveDeviceID);
    }

    [Fact]
    public async Task TheActiveDeviceRegisteringAgainInsideTheGracePeriod_StaysActive()
    {
        Guid active = await ActiveSession();
        await _registry.Disconnect($"c-{active}");
        _time.Advance(TimeSpan.FromSeconds(4));
        await _registry.Register(UserID, new() { DeviceID = active, Name = "Device" }, "c-new");

        _time.Advance(TimeSpan.FromSeconds(20));

        Assert.Equal(active, _service.GetActiveDeviceID(UserID));
    }

    [Fact]
    public async Task TeardownNeverRunsWhileTheActiveDeviceIsOnlineAndPlaying()
    {
        await ActiveSession(true);

        _time.Advance(TimeSpan.FromMinutes(45));

        Assert.NotNull(_service.GetQueue(UserID));
        Assert.Empty(_publisher.GetList<NotificationSessionEnded>());
    }

    [Fact]
    public async Task TeardownStartsWhenPaused_AndFiresAfterTenMinutes_PublishingSessionEnded()
    {
        Guid active = await ActiveSession(true);
        _time.Advance(TimeSpan.FromMinutes(30));
        await _service.Report(UserID, active, Report(2, false));

        _time.Advance(TimeSpan.FromMinutes(9));
        Assert.NotNull(_service.GetQueue(UserID));
        _time.Advance(TimeSpan.FromMinutes(2));

        Assert.Null(_service.GetQueue(UserID));
        Assert.Null(_service.GetState(UserID));
        Assert.Equal(UserID, Assert.Single(_publisher.GetList<NotificationSessionEnded>()).UserID);
    }

    [Fact]
    public async Task TeardownStartsWhenTheActiveDeviceGoesOfflinePastItsGracePeriod()
    {
        Guid active = await ActiveSession(true);
        await _registry.Disconnect($"c-{active}");

        _time.Advance(TimeSpan.FromMinutes(9));
        Assert.NotNull(_service.GetQueue(UserID));
        _time.Advance(TimeSpan.FromMinutes(2));

        Assert.Null(_service.GetQueue(UserID));
        Assert.Single(_publisher.GetList<NotificationSessionEnded>());
    }

    [Theory]
    [InlineData("report")]
    [InlineData("edit")]
    [InlineData("touch")]
    public async Task AnEditCommandOrReport_ResetsTheIdleTimer(string kind)
    {
        Guid active = await ActiveSession(false);
        _time.Advance(TimeSpan.FromMinutes(9));

        switch (kind)
        {
            case "report":
                await _service.Report(UserID, active, Report(2, false));
                break;
            case "edit":
                await _service.Edit(UserID, active, PlayNext(SeedMedia()), Token);
                break;
            case "touch":
                _service.Touch(UserID);
                break;
            default:
                throw new InvalidOperationException(kind);
        }

        _time.Advance(TimeSpan.FromMinutes(9));
        Assert.NotNull(_service.GetQueue(UserID));
        _time.Advance(TimeSpan.FromMinutes(2));
        Assert.Null(_service.GetQueue(UserID));
    }

    [Fact]
    public async Task AfterTeardown_AClaimCreatesAFreshSession()
    {
        Guid active = await ActiveSession(false);
        _time.Advance(TimeSpan.FromMinutes(11));
        Assert.Null(_service.GetQueue(UserID));
        QueueItem current = Item();

        bool accepted = await _service.Claim(UserID, active, ClaimRequest(true, 1, QueueOf(current)));

        Assert.True(accepted);
        Assert.Equal(current, _service.GetQueue(UserID)?.Current);
    }

    [Fact]
    public async Task ASessionOfAnotherAccount_IsNeverVisible()
    {
        await ActiveSession();
        Guid other = Guid.CreateVersion7();
        Guid otherDevice = await Device(other);

        Assert.Null(_service.GetQueue(other));
        Assert.Null(_service.GetState(other));
        Assert.Null(_service.GetActiveDeviceID(other));
        Assert.False(await _service.Report(other, otherDevice, Report(9)));
        Assert.Null(await _service.Edit(other, otherDevice, PlayNext(SeedMedia(other)), Token));
    }

    #endregion -- Dropped active device and teardown --

    #region -- Review fixes --

    [Fact]
    public async Task Edit_ActiveOnlyKind_WhenTheActiveDeviceChangesDuringTheMediaCheck_IsRejected()
    {
        Guid first = await ActiveSession(false);
        Guid second = await Device();
        Guid mediaFileID = SeedMedia();
        QueueItem item = new(Guid.CreateVersion7(), mediaFileID);
        _factory.OnCreate = () => Assert.True(_service.Claim(UserID, second, ClaimRequest(false, 1)).GetAwaiter().GetResult());
        QueueEdit edit = new() { Edit = QueueEditEnum.RegenerateSource, ItemList = [item] };

        await Assert.ThrowsAsync<InvalidQueueEditException>(() => _service.Edit(UserID, first, edit, Token));

        Assert.Equal(second, _service.GetActiveDeviceID(UserID));
        Assert.DoesNotContain(item, _service.GetQueue(UserID)?.SourceList ?? []);
    }

    [Fact]
    public async Task Edit_ReplaceSourceKeepingTheUserQueue_CountsTheWholeQueueAgainstTheLimit()
    {
        Guid mediaFileID = SeedMedia();
        ResponseQueue queue = QueueOf(null);
        queue.UserQueueList.AddRange(Enumerable.Range(0, 60).Select(_ => Item()));
        Guid active = await ActiveSession(true, queue);
        QueueItem[] itemList = [.. Enumerable.Range(0, 60).Select(_ => new QueueItem(Guid.CreateVersion7(), mediaFileID))];
        QueueEdit edit = new() { Edit = QueueEditEnum.ReplaceSource, Source = new() { Kind = QueueSourceKindEnum.Library, PlaylistID = null, Sort = LibrarySortEnum.Title, Descending = false, Search = string.Empty }, ItemList = [.. itemList], KeepUserQueue = true };

        await Assert.ThrowsAsync<InvalidQueueEditException>(() => _service.Edit(UserID, active, edit, Token));
    }

    [Fact]
    public async Task Edit_RegenerateSource_CountsTheWholeQueueAgainstTheLimit()
    {
        Guid mediaFileID = SeedMedia();
        ResponseQueue queue = QueueOf(null, [.. Enumerable.Range(0, 10).Select(_ => Item())]);
        queue.UserQueueList.AddRange(Enumerable.Range(0, 60).Select(_ => Item()));
        Guid active = await ActiveSession(true, queue);
        QueueItem[] itemList = [.. Enumerable.Range(0, 60).Select(_ => new QueueItem(Guid.CreateVersion7(), mediaFileID))];

        await Assert.ThrowsAsync<InvalidQueueEditException>(() => _service.Edit(UserID, active, new() { Edit = QueueEditEnum.RegenerateSource, ItemList = [.. itemList] }, Token));
    }

    [Fact]
    public async Task Claim_WithAListThatIsNull_Throws_InsteadOfFailingWithANullReference()
    {
        Guid deviceID = await Device();
        ResponseQueue queue = new() { QueueRevision = 0, Source = null, HistoryList = null!, Current = null, UserQueueList = [], SourceList = [] };

        await Assert.ThrowsAsync<InvalidQueueEditException>(() => _service.Claim(UserID, deviceID, ClaimRequest(true, 1, queue)));
    }

    #endregion -- Review fixes --
}
