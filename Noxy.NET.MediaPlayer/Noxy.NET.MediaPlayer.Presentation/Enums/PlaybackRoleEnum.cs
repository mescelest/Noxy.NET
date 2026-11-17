namespace Noxy.NET.MediaPlayer.Presentation.Enums;

public enum PlaybackRoleEnum
{
    // No live session, or not connected: this device plays by itself.
    Local,

    // This device is the active device: it plays and reports.
    Active,

    // Another device is active: this one shows its queue and state and sends it commands.
    Controller
}
