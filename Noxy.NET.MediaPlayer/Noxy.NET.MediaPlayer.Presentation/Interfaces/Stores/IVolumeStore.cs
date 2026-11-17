namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

public interface IVolumeStore
{
    // The volume of this device, 0 to 100. A new device holds 100.
    Task<int> GetVolume();

    Task SetVolume(int volume);
}
