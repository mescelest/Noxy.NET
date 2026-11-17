namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

public interface IDeviceService
{
    // Made once on first use and then kept.
    Task<Guid> GetDeviceID();

    // The stored name, else the machine's name.
    Task<string> GetName();

    // Trims the name. Throws ArgumentException for an empty name or one over 50 characters.
    Task SetName(string name);
}
