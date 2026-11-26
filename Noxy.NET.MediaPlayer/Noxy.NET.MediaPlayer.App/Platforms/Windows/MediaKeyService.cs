using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Windows.Media;

// ReSharper disable once CheckNamespace
namespace Noxy.NET.MediaPlayer.App.Services;

// The system media transport controls of the app window: they live as long as the window, so the media keys reach the app whatever is loaded, also from the background. The overlay Windows shows reads what is set here.
public class MediaKeyService : IMediaKeyService
{
    private readonly Lock _lock = new();
    private SystemMediaTransportControls? _controls;
    private MediaKeyState _state = new(false, string.Empty, string.Empty);

    public event Action? PlayPausePressed;

    public event Action? NextPressed;

    public event Action? PreviousPressed;

    public event Action? StopPressed;

    // Called once the window exists. The interop gives the controls of a window, which an unpackaged app has no other way to get.
    public void Attach(Microsoft.UI.Xaml.Window window)
    {
        nint handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        SystemMediaTransportControls controls = SystemMediaTransportControlsInterop.GetForWindow(handle);
        controls.IsPlayEnabled = true;
        controls.IsPauseEnabled = true;
        controls.IsNextEnabled = true;
        controls.IsPreviousEnabled = true;
        controls.IsStopEnabled = true;
        controls.ButtonPressed += OnButtonPressed;
        controls.IsEnabled = true;

        lock (_lock)
        {
            _controls = controls;
        }

        Apply();
    }

    public void SetState(MediaKeyState state)
    {
        lock (_lock)
        {
            _state = state;
        }

        Apply();
    }

    private void Apply()
    {
        SystemMediaTransportControls? controls;
        MediaKeyState state;
        lock (_lock)
        {
            controls = _controls;
            state = _state;
        }

        if (controls is null) return;

        controls.PlaybackStatus = state.IsPlaying ? MediaPlaybackStatus.Playing : MediaPlaybackStatus.Paused;

        SystemMediaTransportControlsDisplayUpdater updater = controls.DisplayUpdater;
        updater.Type = MediaPlaybackType.Music;
        updater.MusicProperties.Title = state.Title;
        updater.MusicProperties.Artist = state.Artist;
        updater.Update();
    }

    // Raised off the UI thread.
    private void OnButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
    {
        switch (args.Button)
        {
            case SystemMediaTransportControlsButton.Play:
            case SystemMediaTransportControlsButton.Pause:
                PlayPausePressed?.Invoke();
                break;
            case SystemMediaTransportControlsButton.Next:
                NextPressed?.Invoke();
                break;
            case SystemMediaTransportControlsButton.Stop:
                StopPressed?.Invoke();
                break;
            case SystemMediaTransportControlsButton.Previous:
                PreviousPressed?.Invoke();
                break;
        }
    }
}
