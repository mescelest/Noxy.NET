using Fluxor;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.Presentation.Features.Shell;

public class FeatureShellEffects(IDevicePreferenceStorage storageDevicePreference, ILogger<FeatureShellEffects> logger)
{
    private const string OpenText = "true";
    private const string ClosedText = "false";

    [EffectMethod]
    public async Task StoreSidebar(ActionShellSidebarSet action, IDispatcher dispatcher)
    {
        try
        {
            await storageDevicePreference.Set(Constants.SidebarOpenKey, action.SidebarOpen ? OpenText : ClosedText);
        }
        catch (Exception exception)
        {
            // The choice just does not survive a restart; the type only, as the preferences belong to the platform.
            logger.LogWarning("The sidebar choice could not be stored ({ExceptionType}).", exception.GetType().Name);
        }
    }

    // A value that is missing, unreadable or not one of the two means open.
    [EffectMethod]
    public async Task RestoreSidebar(ActionShellStarted action, IDispatcher dispatcher)
    {
        bool open = true;
        try
        {
            open = await storageDevicePreference.Get(Constants.SidebarOpenKey) != ClosedText;
        }
        catch (Exception exception)
        {
            logger.LogWarning("The sidebar choice could not be read ({ExceptionType}).", exception.GetType().Name);
        }

        dispatcher.Dispatch(new ActionShellSidebarRestored(open));
    }
}
