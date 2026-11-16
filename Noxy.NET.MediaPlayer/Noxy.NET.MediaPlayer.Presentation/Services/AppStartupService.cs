using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class AppStartupService(ISessionService serviceSession, ISyncService serviceSync, ISettingService serviceSetting, IMetadataSyncService serviceMetadataSync, IMediaHubService serviceHub, IFileReconciler serviceFileReconciler, ILogger<AppStartupService> logger) : IAppStartupService
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
            // Independent of each other, so they wait on the network together and the hub starts after the slowest, not the sum.
            await Task.WhenAll(SyncSubject(nameof(ResponseSetting), serviceSetting.SyncSettingList), SyncSubject(nameof(ResponseArtist), serviceMetadataSync.SyncArtistList), SyncSubject(nameof(ResponseAlbum), serviceMetadataSync.SyncAlbumList),
                SyncSubject(nameof(ResponseGenre), serviceMetadataSync.SyncGenreList));
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
