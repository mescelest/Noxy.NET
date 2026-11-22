namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

public interface IDeviceResetService
{
    // Puts the small choices of this device back to their defaults (the sidebar is open again). The session, the saved server addresses and the downloaded data stay.
    Task ResetPreferences();

    // Signs out and removes everything this device holds: the library, playlists, settings, downloaded files, the saved queue, the saved server addresses and the preferences. Nothing is deleted on the server.
    Task DeleteAllData();
}
