using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeMediaKeyService : IMediaKeyService
{
    public event Action? PlayPausePressed;

    public event Action? NextPressed;

    public event Action? PreviousPressed;

    public event Action? StopPressed;

    public List<MediaKeyState> StateList { get; } = [];

    public void SetState(MediaKeyState state)
    {
        StateList.Add(state);
    }

    public void RaisePlayPause()
    {
        PlayPausePressed?.Invoke();
    }

    public void RaiseNext()
    {
        NextPressed?.Invoke();
    }

    public void RaiseStop()
    {
        StopPressed?.Invoke();
    }

    public void RaisePrevious()
    {
        PreviousPressed?.Invoke();
    }
}
