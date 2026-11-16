using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class AppStartupService(ISessionService serviceSession, ISyncService serviceSync, IMediaHubService serviceHub, ILogger<AppStartupService> logger) : IAppStartupService
{
    public async Task Start()
    {
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
}
