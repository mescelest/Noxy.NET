using System.Net;
using Microsoft.Extensions.Logging;
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
    private readonly FakeLogger<AppStartupService> _logger = new();
    private readonly AppStartupService _serviceStartup;

    public AppStartupServiceTest()
    {
        _serviceStartup = new(_serviceSession, _serviceSync, _serviceSetting, _serviceMetadataSync, _serviceHub, _serviceFileReconciler, _logger);
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
        LibraryDiff diff = new([], [], [Guid.CreateVersion7()]);

        _serviceSync.RaiseLibraryDiff(diff);

        Assert.Equal([diff], _serviceFileReconciler.ApplyList);
    }

    [Fact]
    public async Task Start_CalledTwice_ForwardsEachLibraryDiffOnlyOnce()
    {
        await _serviceStartup.Start();
        await _serviceStartup.Start();

        _serviceSync.RaiseLibraryDiff(new([], [], [Guid.CreateVersion7()]));

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
        List<string> orderList = [];
        List<string> syncedWhenSettingsSynced = [];
        _serviceSetting.OnSyncSettingList = () =>
        {
            syncedWhenSettingsSynced.AddRange(_serviceSync.CallList);
            orderList.Add("settings");
        };
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
