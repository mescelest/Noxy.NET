using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

// Records what it was asked to do and lets a test raise what a real player would: the end of a file, a change, a failure.
public sealed class FakeAudioPlayer : IAudioPlayer
{
    // Every call in order, as the method name, with the path or value where there is one.
    public List<string> CallList { get; } = [];

    // The path of the file that is loaded now, null after Stop.
    public string? LoadedPath { get; private set; }

    public double Volume { get; private set; } = 1;

    public TimeSpan Position { get; set; }

    public TimeSpan Duration { get; set; }

    public bool IsPlaying { get; set; }

    // When set, Load raises Failed with this text instead of loading.
    public string? LoadFailure { get; set; }

    public event Action? Changed;

    public event Action? Ended;

    public event Action<string>? Failed;

    public void RaiseEnded()
    {
        IsPlaying = false;
        Ended?.Invoke();
    }

    public void RaiseChanged()
    {
        Changed?.Invoke();
    }

    public void RaiseFailed(string reason)
    {
        IsPlaying = false;
        Failed?.Invoke(reason);
    }

    public Task Load(string path)
    {
        CallList.Add($"Load {path}");

        if (LoadFailure is not null)
        {
            LoadedPath = null;
            Failed?.Invoke(LoadFailure);

            return Task.CompletedTask;
        }

        LoadedPath = path;
        Position = TimeSpan.Zero;
        IsPlaying = false;

        return Task.CompletedTask;
    }

    public Task Play()
    {
        CallList.Add("Play");
        IsPlaying = LoadedPath is not null;
        Changed?.Invoke();

        return Task.CompletedTask;
    }

    public Task Pause()
    {
        CallList.Add("Pause");
        IsPlaying = false;
        Changed?.Invoke();

        return Task.CompletedTask;
    }

    public Task Seek(TimeSpan position)
    {
        CallList.Add($"Seek {position}");
        Position = position;
        Changed?.Invoke();

        return Task.CompletedTask;
    }

    public Task SetVolume(double volume)
    {
        CallList.Add($"SetVolume {volume}");
        Volume = volume;

        return Task.CompletedTask;
    }

    public Task Stop()
    {
        CallList.Add("Stop");
        LoadedPath = null;
        IsPlaying = false;
        Position = TimeSpan.Zero;

        return Task.CompletedTask;
    }
}
