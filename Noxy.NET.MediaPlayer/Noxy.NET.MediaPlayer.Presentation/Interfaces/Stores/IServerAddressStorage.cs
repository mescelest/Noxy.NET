namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

public interface IServerAddressStorage
{
    // The address of the API that is in use, or null when none is chosen.
    Task<string?> GetAddress();

    // Saves the address normalized (an absolute http or https address without a trailing slash) when it is new, and makes it the one in use. Throws ArgumentException for anything else.
    Task SetAddress(string address);

    // Stops using the address in use. The saved addresses stay.
    Task ClearAddress();

    // Every saved address, in the order they were added.
    Task<List<string>> GetAddressList();

    // Replaces a saved address in place, and follows it when it is the one in use. Throws ArgumentException when the saved address is unknown or the new one is invalid, and InvalidOperationException when another saved address already has it.
    Task ReplaceAddress(string oldAddress, string newAddress);

    // Removes a saved address, and stops using it when it was the one in use.
    Task RemoveAddress(string address);
}
