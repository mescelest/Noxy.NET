namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// The only contract that knows about a sound device. One file at a time, played from a path in the file store. The platform implementations live in the App.
public interface IAudioPlayer
{
    // The file's position or length changed, or it started or stopped playing. At most about once a second while playing.
    event Action? Changed;

    // The loaded file played to its end.
    event Action? Ended;

    // The file could not be opened or decoded. The text says why and holds no path.
    event Action<string>? Failed;

    TimeSpan Position { get; }

    TimeSpan Duration { get; }

    bool IsPlaying { get; }

    // Replaces whatever was loaded. A path from IFileStore.GetPath. Does not start playing.
    Task Load(string path);

    Task Play();

    Task Pause();

    Task Seek(TimeSpan position);

    // 0 to 1.
    Task SetVolume(double volume);

    // Stops and releases the file, so it can be deleted.
    Task Stop();
}
