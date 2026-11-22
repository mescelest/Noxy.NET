using Fluxor;
using Noxy.NET.MediaPlayer.Presentation.Features.Shell;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class DeviceResetService(
    ISessionService serviceSession,
    ILocalDataOwnerService serviceLocalDataOwner,
    IDevicePreferenceStorage storageDevicePreference,
    IServerAddressStorage storageServerAddress,
    ILocalQueueStore storeLocalQueue,
    IDispatcher dispatcher) : IDeviceResetService
{
    public async Task ResetPreferences()
    {
        await storageDevicePreference.Remove(PresentationConstants.SidebarOpenKey);

        // The restored action is not stored again, so the removed key stays removed.
        dispatcher.Dispatch(new ActionShellSidebarRestored(true));
    }

    public async Task DeleteAllData()
    {
        // The session first, so that nothing syncs into the data while it is removed.
        await serviceSession.SignOut();

        await serviceLocalDataOwner.WipeLocalData();
        await storeLocalQueue.Clear();

        await storageServerAddress.ClearAddress();
        foreach (string address in await storageServerAddress.GetAddressList()) await storageServerAddress.RemoveAddress(address);

        await ResetPreferences();
    }
}
