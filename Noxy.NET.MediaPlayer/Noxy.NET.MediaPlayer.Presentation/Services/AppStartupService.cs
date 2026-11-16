using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class AppStartupService(ISessionService serviceSession, ISyncService serviceSync, IMediaHubService serviceHub, IFileReconciler serviceFileReconciler, ILogger<AppStartupService> logger) : IAppStartupService
{
    private bool _subscribed;

    public async Task Start()
    {
        await RepairFiles();
        Subscribe();

        try
        {
            if (!await serviceSession.TryRestore()) return;

            await serviceSync.SyncLibrary();
            await serviceSync.SyncPlaylistList();
            await serviceHub.Start();
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
        _subscribed = true;
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
