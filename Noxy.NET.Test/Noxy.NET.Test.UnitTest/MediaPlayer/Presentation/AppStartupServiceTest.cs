using System.Net;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class AppStartupServiceTest
{
    private readonly FakeSessionService _serviceSession = new();
    private readonly FakeSyncService _serviceSync = new();
    private readonly FakeSettingService _serviceSetting = new();
    private readonly FakeMetadataSyncService _serviceMetadataSync = new();
    private readonly FakeMediaHubService _serviceHub = new();
    private readonly FakeFileReconciler _serviceFileReconciler = new();
    private readonly FakeFileTagService _serviceFileTag = new();
    private readonly FakeDownloadService _serviceDownload = new();
    private readonly FakeQueueService _serviceQueue = new();
    private readonly FakePlaybackReporterService _serviceReporter = new();
    private readonly FakeRemoteCommandHandlerService _serviceCommandHandler = new();
    private readonly FakeLocalDataOwnerService _serviceLocalDataOwner = new();
    private readonly FakePlayerService _servicePlayer = new();
    private readonly FakeDelay _delay = new() { IsManual = true };
    private readonly FakeLogger<AppStartupService> _logger = new();
    private readonly AppStartupService _serviceStartup;

    public AppStartupServiceTest()
    {
        _serviceStartup = new(_serviceSession, _serviceSync, _serviceSetting, _serviceMetadataSync, _serviceHub, _serviceFileReconciler, _serviceFileTag, _serviceDownload, _serviceQueue, _serviceReporter, _serviceCommandHandler, _serviceLocalDataOwner,
            _servicePlayer, _delay, _logger);
    }

    [Fact]
    public async Task Start_LoadsTheLocalQueue_StartsTheReporterAndTheCommandHandler_AndNeverStartsPlayback()
    {
        List<string> startedBeforeTheHub = [];
        _serviceHub.OnStart = () =>
        {
            if (_serviceReporter.StartCount > 0) startedBeforeTheHub.Add("reporter");
            if (_serviceCommandHandler.StartCount > 0) startedBeforeTheHub.Add("handler");
            if (_serviceQueue.LoadLocalCopyCount > 0) startedBeforeTheHub.Add("copy");
        };

        await _serviceStartup.Start();

        Assert.Equal(1, _serviceQueue.LoadLocalCopyCount);
        Assert.Equal(1, _serviceReporter.StartCount);
        Assert.Equal(1, _serviceCommandHandler.StartCount);
        Assert.Equal(["reporter", "handler", "copy"], startedBeforeTheHub);
        Assert.Empty(_serviceQueue.ApplyList);
        Assert.Empty(_serviceQueue.StartSourceList);
    }

    [Fact]
    public async Task Start_ChecksTheFiles_AndWithTheSettingOffWritesNothing()
    {
        _serviceSetting.UpdateFileMetadata = false;

        await _serviceStartup.Start();
        await WaitUntil(() => _serviceFileTag.CheckCount == 1);

        Assert.Equal(0, _serviceFileTag.UpdateOutdatedCount);
    }

    [Fact]
    public async Task Start_WithTheSettingOn_UpdatesTheOutdatedFilesAfterTheCheck()
    {
        _serviceSetting.UpdateFileMetadata = true;

        await _serviceStartup.Start();
        await WaitUntil(() => _serviceFileTag.UpdateOutdatedCount == 1);

        Assert.Equal(1, _serviceFileTag.CheckCount);
    }

    [Fact]
    public async Task SyncChanged_ChecksTheFiles_AndWithTheSettingOnUpdatesThem()
    {
        await _serviceStartup.Start();
        await WaitUntil(() => _serviceFileTag.CheckCount == 1);
        _serviceSetting.UpdateFileMetadata = true;

        _serviceSync.RaiseChanged();
        await WaitUntil(() => _serviceFileTag.UpdateOutdatedCount == 1);

        Assert.Equal(2, _serviceFileTag.CheckCount);
    }

    [Fact]
    public async Task DownloadChanged_ChecksTheFiles()
    {
        await _serviceStartup.Start();
        await WaitUntil(() => _serviceFileTag.CheckCount == 1);

        _serviceDownload.RaiseChanged();
        await WaitUntil(() => _serviceFileTag.CheckCount == 2);

        Assert.Equal(0, _serviceFileTag.UpdateOutdatedCount);
    }

    [Fact]
    public async Task EventsDuringARun_CauseOneMoreRun_NotAnOverlap()
    {
        _serviceFileTag.CheckGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await _serviceStartup.Start();
        await WaitUntil(() => _serviceFileTag.CheckCount == 1);

        _serviceSync.RaiseChanged();
        _serviceSync.RaiseChanged();
        _serviceDownload.RaiseChanged();
        _serviceFileTag.CheckGate.SetResult();
        await WaitUntil(() => _serviceFileTag.CheckCount == 2);
        await Task.Delay(100, TestContext.Current.CancellationToken);

        Assert.Equal(2, _serviceFileTag.CheckCount);
        Assert.Equal(1, _serviceFileTag.MaxActiveCount);
    }

    [Fact]
    public async Task AFailingCheckOrUpdate_IsLogged_NotThrown()
    {
        _serviceFileTag.Failure = new IOException("The disk is not readable.");

        await _serviceStartup.Start();
        await WaitUntil(() => _logger.EntryList.Any(x => x.Level == LogLevel.Error));

        Assert.Equal(1, _serviceFileTag.CheckCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Start_LoadsTheStoredVolume_SignedInOrNot_SoTheSliderIsRightBeforeAnythingPlays(bool signedIn)
    {
        _serviceSession.RestoreSucceeds = signedIn;

        await _serviceStartup.Start();

        Assert.Single(_servicePlayer.CallList, "LoadVolume");
    }

    [Fact]
    public async Task Start_WhenNotSignedIn_StillLoadsTheLocalQueue_SoPlaybackNeverNeedsTheServer()
    {
        _serviceSession.RestoreSucceeds = false;

        await _serviceStartup.Start();

        Assert.Equal(1, _serviceQueue.LoadLocalCopyCount);
        Assert.Equal(1, _serviceReporter.StartCount);
        Assert.Equal(0, _serviceHub.StartCount);
    }

    [Fact]
    public async Task Start_WhenTheLocalQueueCannotBeLoaded_LogsIt_AndStillSyncs()
    {
        _serviceQueue.LocalCopyFailure = new IOException("The store is not readable.");

        await _serviceStartup.Start();

        Assert.Contains(_logger.EntryList, x => x.Level == LogLevel.Error);
        Assert.Equal(["SyncLibrary", "SyncPlaylistList"], _serviceSync.CallList);
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(10);
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, "Timed out waiting for the condition.");
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }
    }

    #region -- Offline start-up --

    [Fact]
    public async Task Start_WhenOffline_RetriesWithGrowingDelays_AndCapsAt60Seconds()
    {
        _serviceSession.RestoreResult = RestoreResultEnum.Offline;

        await _serviceStartup.Start();
        for (int round = 0; round < 6; round++)
        {
            await WaitUntil(() => _delay.PendingCount == 1);
            if (round == 5) _serviceSession.RestoreResult = RestoreResultEnum.NoSession;

            _delay.ReleaseAll();
            if (round < 5) await WaitUntil(() => _delay.DelayList.Count == round + 2);
        }

        await WaitUntil(() => !_serviceStartup.IsRetrying);
        Assert.Equal([5, 10, 20, 40, 60, 60], _delay.DelayList.Select(x => (int)x.TotalSeconds));
    }

    [Fact]
    public async Task Start_WhenOffline_StillRepairsTheFiles_LoadsTheLocalQueue_AndNeitherSyncsNorConnects()
    {
        _serviceSession.RestoreResult = RestoreResultEnum.Offline;

        await _serviceStartup.Start();

        Assert.Equal(1, _serviceFileReconciler.RepairCount);
        Assert.Equal(1, _serviceQueue.LoadLocalCopyCount);
        Assert.Equal(1, _serviceReporter.StartCount);
        Assert.Empty(_serviceSync.CallList);
        Assert.Equal(0, _serviceHub.StartCount);
        Assert.Equal(0, _serviceLocalDataOwner.EnsureOwnerCount);
        Assert.True(_serviceSession.HasSession);
        await WaitUntil(() => _delay.PendingCount == 1);
    }

    [Fact]
    public async Task Start_WhenTheRetrySucceeds_RunsTheRestOfStartUpOnce_AndStops()
    {
        _serviceSession.RestoreResult = RestoreResultEnum.Offline;
        await _serviceStartup.Start();
        await WaitUntil(() => _delay.PendingCount == 1);
        _serviceSession.RestoreResult = RestoreResultEnum.SignedIn;

        _delay.ReleaseAll();
        await WaitUntil(() => _serviceHub.StartCount == 1);
        await WaitUntil(() => !_serviceStartup.IsRetrying);

        Assert.Equal(1, _serviceLocalDataOwner.EnsureOwnerCount);
        Assert.Equal(["SyncLibrary", "SyncPlaylistList"], _serviceSync.CallList);
        Assert.Single(_delay.DelayList);
        Assert.Equal(2, _serviceSession.RestoreCount);
    }

    [Theory]
    [InlineData(RestoreResultEnum.Rejected)]
    [InlineData(RestoreResultEnum.NoSession)]
    public async Task Start_WhenTheRetryIsRejectedOrFindsNoSession_StopsRetrying_WithoutConnecting(RestoreResultEnum answer)
    {
        _serviceSession.RestoreResult = RestoreResultEnum.Offline;
        await _serviceStartup.Start();
        await WaitUntil(() => _delay.PendingCount == 1);
        _serviceSession.RestoreResult = answer;

        _delay.ReleaseAll();
        await WaitUntil(() => !_serviceStartup.IsRetrying);
        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.Equal(0, _serviceHub.StartCount);
        Assert.Empty(_serviceSync.CallList);
        Assert.Single(_delay.DelayList);
    }

    [Fact]
    public async Task RetryNow_TriesAtOnce_WithoutWaitingForTheDelay()
    {
        _serviceSession.RestoreResult = RestoreResultEnum.Offline;
        await _serviceStartup.Start();
        await WaitUntil(() => _delay.PendingCount == 1);
        _serviceSession.RestoreResult = RestoreResultEnum.SignedIn;

        await _serviceStartup.RetryNow();
        await WaitUntil(() => _serviceHub.StartCount == 1);

        Assert.Single(_delay.DelayList);
    }

    [Fact]
    public async Task RetryNow_WhenNoRetryIsUnderWay_DoesNothing()
    {
        await _serviceStartup.Start();

        await _serviceStartup.RetryNow();

        Assert.Equal(1, _serviceSession.RestoreCount);
        Assert.Empty(_delay.DelayList);
    }

    [Fact]
    public async Task IsRetrying_IsTrueWhileWaiting_AndChangedIsRaised()
    {
        int changed = 0;
        _serviceStartup.Changed += () => Interlocked.Increment(ref changed);
        Assert.False(_serviceStartup.IsRetrying);
        _serviceSession.RestoreResult = RestoreResultEnum.Offline;

        await _serviceStartup.Start();
        await WaitUntil(() => _serviceStartup.IsRetrying);
        Assert.True(changed >= 1);

        _serviceSession.RestoreResult = RestoreResultEnum.NoSession;
        _delay.ReleaseAll();
        await WaitUntil(() => !_serviceStartup.IsRetrying);

        Assert.True(changed >= 2);
    }

    [Fact]
    public async Task ARetryThatThrows_CountsAsOffline_AndTheLoopGoesOn()
    {
        _serviceSession.RestoreResult = RestoreResultEnum.Offline;
        await _serviceStartup.Start();
        await WaitUntil(() => _delay.PendingCount == 1);
        _serviceSession.RestoreException = new InvalidOperationException("secret detail");

        _delay.ReleaseAll();
        await WaitUntil(() => _delay.DelayList.Count == 2);

        Assert.Contains(_logger.EntryList, x => x.Level == LogLevel.Error);
        Assert.DoesNotContain(_logger.EntryList, x => x.Message.Contains("secret"));
    }

    #endregion -- Offline start-up --

    [Fact]
    public async Task HasRestored_IsFalseUntilTheFirstRestoreFinished_ThenTrueForEveryAnswer()
    {
        Assert.False(_serviceStartup.HasRestored);
        int changed = 0;
        _serviceStartup.Changed += () => changed++;
        _serviceSession.RestoreResult = RestoreResultEnum.NoSession;

        await _serviceStartup.Start();

        Assert.True(_serviceStartup.HasRestored);
        Assert.Equal(1, changed);
    }

    [Fact]
    public async Task HasRestored_IsAlsoTrue_WhenTheRestoreThrew()
    {
        _serviceSession.RestoreException = new InvalidOperationException("broken");

        await _serviceStartup.Start();

        Assert.True(_serviceStartup.HasRestored);
    }

    [Fact]
    public async Task Connect_RunsTheRestOfStartUp_WithoutRestoringTheSessionAgain()
    {
        List<string> order = [];
        _serviceLocalDataOwner.OnEnsureOwner = () => order.Add("owner");

        await _serviceStartup.Connect();

        Assert.Equal(0, _serviceSession.RestoreCount);
        Assert.Equal(1, _serviceLocalDataOwner.EnsureOwnerCount);
        Assert.Equal(["owner"], order);
        Assert.Equal(1, _serviceHub.StartCount);
        Assert.Contains("SyncLibrary", _serviceSync.CallList);
        Assert.True(_serviceStartup.HasRestored);
    }

    [Fact]
    public async Task Connect_WhileTheRetryLoopWaits_MeansTheLoopDoesNotConnectAgain()
    {
        _serviceSession.RestoreResult = RestoreResultEnum.Offline;
        await _serviceStartup.Start();
        await WaitUntil(() => _delay.PendingCount == 1);

        await _serviceStartup.Connect();
        _serviceSession.RestoreResult = RestoreResultEnum.SignedIn;
        _delay.ReleaseAll();
        await WaitUntil(() => !_serviceStartup.IsRetrying);
        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.Equal(1, _serviceHub.StartCount);
        Assert.Equal(1, _serviceLocalDataOwner.EnsureOwnerCount);
    }

    [Fact]
    public async Task AfterTheSessionEnded_ASecondSignIn_ConnectsAgain()
    {
        await _serviceStartup.Connect();
        Assert.Equal(1, _serviceHub.StartCount);
        _serviceSession.AccessToken = null;
        _serviceSession.RaiseChanged();
        _serviceSession.AccessToken = "token-3";

        await _serviceStartup.Connect();

        Assert.Equal(2, _serviceHub.StartCount);
    }

    [Fact]
    public async Task WhenTheSessionEnds_ThePlayerIsPaused_AndTheQueueIsKept()
    {
        await _serviceStartup.Start();
        _serviceSession.AccessToken = null;

        _serviceSession.RaiseChanged();

        await WaitUntil(() => _servicePlayer.CallList.Contains("Pause"));
        Assert.DoesNotContain("Clear", _servicePlayer.CallList);
    }

    [Fact]
    public async Task WhenTheSessionBegins_ThePlayerIsLeftAlone()
    {
        await _serviceStartup.Start();

        _serviceSession.RaiseChanged();
        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.DoesNotContain("Pause", _servicePlayer.CallList);
    }

    [Fact]
    public async Task Start_EnsuresTheOwner_AfterTheSessionIsRestored_AndBeforeAnySync()
    {
        List<string> syncedWhenEnsuring = ["not run"];
        _serviceLocalDataOwner.OnEnsureOwner = () => syncedWhenEnsuring = [.. _serviceSync.CallList];

        await _serviceStartup.Start();

        Assert.Equal(1, _serviceLocalDataOwner.EnsureOwnerCount);
        Assert.Empty(syncedWhenEnsuring);
    }

    [Fact]
    public async Task Start_WhenTheSessionCannotBeRestored_DoesNotEnsureAnOwner()
    {
        _serviceSession.RestoreSucceeds = false;

        await _serviceStartup.Start();

        Assert.Equal(0, _serviceLocalDataOwner.EnsureOwnerCount);
    }

    [Fact]
    public async Task Start_RepairsTheFilesFirst_BeforeAnythingIsSynced()
    {
        List<string> calledWhenRepairing = ["not run"];
        _serviceFileReconciler.OnRepair = () => calledWhenRepairing = [.. _serviceSync.CallList];

        await _serviceStartup.Start();

        Assert.Equal(1, _serviceFileReconciler.RepairCount);
        Assert.Empty(calledWhenRepairing);
    }

    [Fact]
    public async Task Start_RepairsEvenWhenTheSessionCannotBeRestored()
    {
        _serviceSession.RestoreSucceeds = false;

        await _serviceStartup.Start();

        Assert.Equal(1, _serviceFileReconciler.RepairCount);
    }

    [Fact]
    public async Task Start_WhenTheRepairFails_LogsIt_AndStillSyncs()
    {
        _serviceFileReconciler.OnRepair = () => throw new IOException("The disk is not readable.");

        await _serviceStartup.Start();

        Assert.Contains(_logger.EntryList, x => x.Level == LogLevel.Error);
        Assert.Equal(["SyncLibrary", "SyncPlaylistList"], _serviceSync.CallList);
    }

    [Fact]
    public async Task Start_ForwardsEveryLibraryDiffToTheReconciler()
    {
        await _serviceStartup.Start();
        LibraryDiff diff = new([], [Guid.CreateVersion7()]);

        _serviceSync.RaiseLibraryDiff(diff);

        Assert.Equal([diff], _serviceFileReconciler.ApplyList);
    }

    [Fact]
    public async Task Start_CalledTwice_ForwardsEachLibraryDiffOnlyOnce()
    {
        await _serviceStartup.Start();
        await _serviceStartup.Start();

        _serviceSync.RaiseLibraryDiff(new([], [Guid.CreateVersion7()]));

        Assert.Single(_serviceFileReconciler.ApplyList);
    }

    [Fact]
    public async Task Start_WhenSignedIn_SyncsBothListsThenConnectsTheHub()
    {
        List<string> syncedWhenTheHubStarted = [];
        _serviceHub.OnStart = () => syncedWhenTheHubStarted.AddRange(_serviceSync.CallList);

        await _serviceStartup.Start();

        Assert.Equal(["SyncLibrary", "SyncPlaylistList"], syncedWhenTheHubStarted);
        Assert.Equal(1, _serviceHub.StartCount);
    }

    [Fact]
    public async Task Start_WhenTheStoredTokenIsRejected_SyncsNothing_AndDoesNotConnect()
    {
        _serviceSession.RestoreSucceeds = false;

        await _serviceStartup.Start();

        Assert.Empty(_serviceSync.CallList);
        Assert.Equal(0, _serviceHub.StartCount);
    }

    [Fact]
    public async Task Start_WithNoServerAddress_DoesNothing_AndDoesNotThrow()
    {
        _serviceSession.RestoreException = new ServerAddressMissingException();

        await _serviceStartup.Start();

        Assert.Empty(_serviceSync.CallList);
        Assert.Equal(0, _serviceHub.StartCount);
    }

    [Fact]
    public async Task Start_WhenOffline_DoesNotThrow_AndStillTriesTheHub()
    {
        _serviceSync.IsOffline = true;

        await _serviceStartup.Start();

        Assert.Equal(["SyncLibrary", "SyncPlaylistList"], _serviceSync.CallList);
        Assert.Equal(1, _serviceHub.StartCount);
    }

    [Fact]
    public async Task Start_WhenTheServerAnswersWithAnError_LogsIt_DoesNotThrow_AndDoesNotConnect()
    {
        _serviceSync.SyncLibraryException = new ApiException(HttpStatusCode.InternalServerError, "Broken.");

        await _serviceStartup.Start();

        Assert.Equal(0, _serviceHub.StartCount);
        Assert.Contains(_logger.EntryList, x => x.Level == LogLevel.Error);
    }

    [Fact]
    public async Task Start_WhenTheRestoreFails_LogsIt_AndDoesNotThrow()
    {
        _serviceSession.RestoreException = new ApiException(HttpStatusCode.InternalServerError, "Broken.");

        await _serviceStartup.Start();

        Assert.Empty(_serviceSync.CallList);
        Assert.Contains(_logger.EntryList, x => x.Level == LogLevel.Error);
    }

    #region -- The settings and the metadata lists --

    [Fact]
    public async Task Start_SyncsTheNewSubjectsAfterTheLibraryAndPlaylists_ThenConnectsTheHub()
    {
        List<string> syncedWhenSettingsSynced = [];
        _serviceSetting.OnSyncSettingList = () => syncedWhenSettingsSynced.AddRange(_serviceSync.CallList);
        List<string> syncedWhenTheHubStarted = [];
        _serviceHub.OnStart = () => syncedWhenTheHubStarted.AddRange(_serviceMetadataSync.CallList);

        await _serviceStartup.Start();

        Assert.Equal(["SyncLibrary", "SyncPlaylistList"], syncedWhenSettingsSynced);
        Assert.Equal(["SyncArtistList", "SyncAlbumList", "SyncGenreList"], _serviceMetadataSync.CallList);
        Assert.Equal(["SyncArtistList", "SyncAlbumList", "SyncGenreList"], syncedWhenTheHubStarted);
        Assert.Equal([false], _serviceSetting.SyncRequestList);
        Assert.Equal(1, _serviceHub.StartCount);
    }

    [Fact]
    public async Task Start_WhenOneNewSyncFails_StillRunsTheRest_AndConnectsTheHub()
    {
        _serviceSetting.SyncSettingListException = new ApiException(HttpStatusCode.InternalServerError, "Broken.");
        _serviceMetadataSync.SyncArtistListException = new ApiException(HttpStatusCode.InternalServerError, "Broken.");

        await _serviceStartup.Start();

        Assert.Equal(["SyncArtistList", "SyncAlbumList", "SyncGenreList"], _serviceMetadataSync.CallList);
        Assert.Equal(1, _serviceHub.StartCount);
        Assert.Equal(2, _logger.EntryList.Count(x => x.Level == LogLevel.Error));
    }

    [Fact]
    public async Task Start_WhenTheSessionCannotBeRestored_SyncsNoneOfTheNewSubjects()
    {
        _serviceSession.RestoreSucceeds = false;

        await _serviceStartup.Start();

        Assert.Empty(_serviceSetting.SyncRequestList);
        Assert.Empty(_serviceMetadataSync.CallList);
    }

    #endregion -- The settings and the metadata lists --
}
