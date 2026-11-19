namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

public interface IServerAddressStorage
{
    // The saved address of the API, or null when none is saved yet.
    Task<string?> GetAddress();

    // Saves the address normalized (an absolute http or https address without a trailing slash). Throws ArgumentException for anything else.
    Task SetAddress(string address);

    Task ClearAddress();
}
