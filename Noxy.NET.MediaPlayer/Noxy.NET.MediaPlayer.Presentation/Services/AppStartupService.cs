using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class AppStartupService(
    ISessionService serviceSession,
    ISyncService serviceSync,
    ISettingService serviceSetting,
    IMetadataSyncService serviceMetadataSync,
    IMediaHubService serviceHub,
    IFileReconciler serviceFileReconciler,
    IQueueService serviceQueue,
    IPlaybackReporterService serviceReporter,
    IRemoteCommandHandlerService serviceCommandHandler,
    ILocalDataOwnerService serviceLocalDataOwner,
    IPlayerService servicePlayer,
    IDelay delay,
    ILogger<AppStartupService> logger) : IAppStartupService
{
    private bool _subscribed;
    private readonly Lock _retryLock = new();
    private CancellationTokenSource? _waitSource;
    private bool _loopRunning;
    private bool _isRetrying;
    private bool _hasRestored;
    private bool _connected;

    public event Action? Changed;

    public bool IsRetrying
    {
        get
        {
            lock (_retryLock) return _isRetrying;
        }
    }

    public bool HasRestored
    {
        get
        {
            lock (_retryLock) return _hasRestored;
        }
    }

    public async Task Start()
    {
        // A setting of this device, so it needs neither the session nor the server: the slider is right before the first media file plays.
        await servicePlayer.LoadVolume();
        await RepairFiles();
        Subscribe();
        await LoadLocalQueue();

        // The device registers by itself on every connection the hub makes; these two must be listening before the first one.
        serviceReporter.Start();
        serviceCommandHandler.Start();

        RestoreResultEnum result;
        try
        {
            result = await serviceSession.TryRestore();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Restoring the session failed.");
            SetRestored();
            return;
        }

        SetRestored();

        // NoSession and Rejected end here: the user signs in. Offline keeps what is on the device available and tries again by itself.
        if (result == RestoreResultEnum.SignedIn) await ConnectToServer();
        else if (result == RestoreResultEnum.Offline) StartRetryLoop();
    }

    public async Task Connect()
    {
        SetRestored();
        await ConnectToServer();
    }

    // Tries again now instead of at the end of the wait. Nothing to do when no retry is under way.
    public Task RetryNow()
    {
        CancellationTokenSource? source;
        lock (_retryLock) source = _waitSource;

        try
        {
            source?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // The wait ended by itself a moment ago.
        }

        return Task.CompletedTask;
    }

    // Signed in: the rest of start-up.
    private async Task ConnectToServer()
    {
        try
        {
            // Before any sync: a different account on this device must never see what the last one left.
            await serviceLocalDataOwner.EnsureOwner();

            await serviceSync.SyncLibrary();
            await serviceSync.SyncPlaylistList();
            // Independent of each other, so they wait on the network together and the hub starts after the slowest, not the sum.
            await Task.WhenAll(SyncSubject(nameof(ResponseSetting), serviceSetting.SyncSettingList), SyncSubject(nameof(ResponseArtist), serviceMetadataSync.SyncArtistList), SyncSubject(nameof(ResponseAlbum), serviceMetadataSync.SyncAlbumList),
                SyncSubject(nameof(ResponseGenre), serviceMetadataSync.SyncGenreList));
            await serviceHub.Start();
            lock (_retryLock) _connected = true;
        }
        catch (ServerAddressMissingException)
        {
            // First run: nothing to do until the user has entered a server address.
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Starting the app's connection to the server failed.");
        }
    }

    private bool IsConnected
    {
        get
        {
            lock (_retryLock) return _connected;
        }
    }

    private void StartRetryLoop()
    {
        lock (_retryLock)
        {
            if (_loopRunning) return;

            _loopRunning = true;
        }

        _ = RetryLoop();
    }

    // Waits 5, 10, 20, up to 60 seconds between tries. A server that answers ends it: signed in runs the rest of start-up, anything else (no session, rejected) leaves the user to sign in.
    private async Task RetryLoop()
    {
        int attempt = 0;
        try
        {
            while (true)
            {
                await WaitBeforeRetry(TimeSpan.FromSeconds(Math.Min(PresentationConstants.StartupRetryBaseSeconds * Math.Pow(2, attempt), PresentationConstants.StartupRetryMaxSeconds)));

                RestoreResultEnum result = await TryRestoreForRetry();
                switch (result)
                {
                    case RestoreResultEnum.Offline:
                        attempt++;
                        continue;
                    // A manual sign-in may have connected while the loop waited: the syncs and the hub must not run twice for it.
                    case RestoreResultEnum.SignedIn when !IsConnected:
                        await ConnectToServer();
                        break;
                    case RestoreResultEnum.NoSession:
                    case RestoreResultEnum.Rejected:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return;
            }
        }
        finally
        {
            lock (_retryLock) _loopRunning = false;

            SetRetrying(false);
        }
    }

    private async Task WaitBeforeRetry(TimeSpan time)
    {
        using CancellationTokenSource source = new();
        lock (_retryLock) _waitSource = source;

        SetRetrying(true);
        try
        {
            await delay.Wait(time, source.Token);
        }
        catch (OperationCanceledException)
        {
            // RetryNow: the wait is over.
        }
        finally
        {
            lock (_retryLock) _waitSource = null;

            SetRetrying(false);
        }
    }

    private async Task<RestoreResultEnum> TryRestoreForRetry()
    {
        try
        {
            return await serviceSession.TryRestore();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Restoring the session failed.");
            return RestoreResultEnum.Offline;
        }
    }

    private void SetRestored()
    {
        lock (_retryLock)
        {
            if (_hasRestored) return;

            _hasRestored = true;
        }

        Changed?.Invoke();
    }

    private void SetRetrying(bool value)
    {
        lock (_retryLock)
        {
            if (_isRetrying == value) return;

            _isRetrying = value;
        }

        Changed?.Invoke();
    }

    // Each of these has its own try, so one that fails does not skip the others. What failed is named, never a value.
    private async Task SyncSubject(string subject, Func<bool, Task> sync)
    {
        try
        {
            await sync(false);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Syncing {Subject} failed.", subject);
        }
    }

    // The queue this device had when it was last used, so "Continue" has something to resume. It never starts playing by itself.
    private async Task LoadLocalQueue()
    {
        try
        {
            await serviceQueue.LoadLocalCopy();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Loading the saved queue failed.");
        }
    }

    // Local only, so it runs first and does not depend on the network or on being signed in.
    private async Task RepairFiles()
    {
        try
        {
            await serviceFileReconciler.Repair();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Repairing the downloaded files failed.");
        }
    }

    // From here on every change to the library reaches the files on the device.
    private void Subscribe()
    {
        if (_subscribed) return;

        serviceSync.LibraryDiffApplied += OnLibraryDiffApplied;
        serviceSession.Changed += OnSessionChanged;
        _subscribed = true;
    }

    // A session that ends (sign out, or a refresh the server rejected) pauses what is playing: there is no player bar left to do it. The queue stays: the same account signing in again resumes it, and a different one wipes it.
    private void OnSessionChanged()
    {
        if (serviceSession.HasSession) return;

        lock (_retryLock) _connected = false;

        _ = PausePlayer();
    }

    private async Task PausePlayer()
    {
        try
        {
            await servicePlayer.Pause();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Pausing the player after the session ended failed.");
        }
    }

    private void OnLibraryDiffApplied(LibraryDiff diff)
    {
        _ = ApplyLibraryDiff(diff);
    }

    private async Task ApplyLibraryDiff(LibraryDiff diff)
    {
        try
        {
            await serviceFileReconciler.Apply(diff);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Updating the downloaded files after a library change failed.");
        }
    }
}
