using Windows.Media.Playback;
using Windows.Storage;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using WindowsMediaPlayer = Windows.Media.Playback.MediaPlayer;
using WindowsMediaSource = Windows.Media.Core.MediaSource;

// ReSharper disable once CheckNamespace
namespace Noxy.NET.MediaPlayer.App.Services;

public class AudioPlayer : IAudioPlayer
{
    private const int ChangedIntervalMilliseconds = 1000;
    private const string DecodeFailure = "The file could not be played.";

    private readonly Lock _lock = new();
    private readonly Timer _timer;
    private WindowsMediaPlayer? _player;
    private WindowsMediaSource? _source;
    private double _volume = 1;

    public AudioPlayer()
    {
        _timer = new(_ => Changed?.Invoke(), null, Timeout.Infinite, Timeout.Infinite);
    }

    public event Action? Changed;

    public event Action? Ended;

    public event Action<string>? Failed;

    public TimeSpan Position => _player?.PlaybackSession.Position ?? TimeSpan.Zero;

    public TimeSpan Duration => _player?.PlaybackSession.NaturalDuration ?? TimeSpan.Zero;

    public bool IsPlaying => _player?.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;

    public async Task Load(string path)
    {
        await Stop();

        try
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(path);
            WindowsMediaSource source = WindowsMediaSource.CreateFromStorageFile(file);
            WindowsMediaPlayer player = new() { Source = new MediaPlaybackItem(source), AutoPlay = false, Volume = _volume };
            // The keys belong to the window-level controls (MediaKeyService), which exist whether or not a file is loaded.
            player.CommandManager.IsEnabled = false;
            player.MediaEnded += (_, _) => OnEnded();
            player.MediaFailed += (_, _) => OnFailed();
            player.MediaOpened += (_, _) => Changed?.Invoke();
            player.PlaybackSession.PlaybackStateChanged += (_, _) => OnPlaybackStateChanged();

            lock (_lock)
            {
                _source = source;
                _player = player;
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            // The text carries no path: it may end up in a log.
            Failed?.Invoke(DecodeFailure);
        }
    }

    public Task Play()
    {
        _player?.Play();

        return Task.CompletedTask;
    }

    public Task Pause()
    {
        _player?.Pause();

        return Task.CompletedTask;
    }

    public Task Seek(TimeSpan position)
    {
        if (_player is not null) _player.PlaybackSession.Position = position;
        Changed?.Invoke();

        return Task.CompletedTask;
    }

    public Task SetVolume(double volume)
    {
        _volume = Math.Clamp(volume, 0, 1);
        if (_player is not null) _player.Volume = _volume;

        return Task.CompletedTask;
    }

    // Disposing the player and the source lets go of the file handle, so the file can be deleted right after.
    public Task Stop()
    {
        WindowsMediaPlayer? player;
        WindowsMediaSource? source;
        lock (_lock)
        {
            player = _player;
            source = _source;
            _player = null;
            _source = null;
        }

        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        player?.Pause();
        player?.Dispose();
        source?.Dispose();

        return Task.CompletedTask;
    }

    private void OnPlaybackStateChanged()
    {
        // The position is reported about once a second while playing, not on every sample.
        _timer.Change(IsPlaying ? 0 : Timeout.Infinite, IsPlaying ? ChangedIntervalMilliseconds : Timeout.Infinite);
        Changed?.Invoke();
    }

    private void OnEnded()
    {
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        Ended?.Invoke();
    }

    private void OnFailed()
    {
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        Failed?.Invoke(DecodeFailure);
    }
}
