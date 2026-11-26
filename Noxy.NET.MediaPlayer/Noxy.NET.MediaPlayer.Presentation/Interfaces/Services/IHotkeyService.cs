namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Turns a pressed key into the call on IPlaybackControlService that the HotkeyCatalog binds it to. It also listens to the IMediaKeyService, when the platform has one.
public interface IHotkeyService
{
    // False for a key that has no binding.
    Task<bool> Press(string key);
}
