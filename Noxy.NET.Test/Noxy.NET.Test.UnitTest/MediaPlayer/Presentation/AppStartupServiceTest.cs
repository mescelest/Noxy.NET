using System.Net;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class AppStartupServiceTest
{
    private readonly FakeSessionService _serviceSession = new();
    private readonly FakeSyncService _serviceSync = new();
    private readonly FakeMediaHubService _serviceHub = new();
    private readonly FakeLogger<AppStartupService> _logger = new();
    private readonly AppStartupService _serviceStartup;

    public AppStartupServiceTest()
    {
        _serviceStartup = new(_serviceSession, _serviceSync, _serviceHub, _logger);
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
}
