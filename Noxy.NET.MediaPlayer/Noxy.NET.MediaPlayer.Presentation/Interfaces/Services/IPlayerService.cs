using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Plays from the file store only: a media file that is on the device is played with no network call, one that is missing is downloaded first. The same media file can be in the queue twice.
public interface IPlayerService
{
    // Raised when the queue, the current entry, the play state or the position changed.
    event Action? Changed;

    PlayerQueueEntry? Current { get; }

    List<PlayerQueueEntry> QueueList { get; }

    bool IsPlaying { get; }

    TimeSpan Position { get; }

    TimeSpan Duration { get; }

    double Volume { get; }

    // True while the current entry is being downloaded before it can play.
    bool IsWaitingForDownload { get; }

    // Clears the queue and plays that media file.
    Task Play(Guid mediaFileID);

    // Replaces the queue and plays from the start index (clamped to the list).
    Task PlayList(List<Guid> mediaFileIDList, int startIndex);

    // Adds to the end of the queue. Does not start playing.
    Task Enqueue(Guid mediaFileID);

    // The current entry: the next playable one plays. Any other entry just leaves the queue.
    Task Remove(int index);

    Task Pause();

    // Does nothing when nothing is loaded.
    Task Resume();

    Task Seek(TimeSpan position);

    // At the end of the queue: nothing happens.
    Task Next();

    // At the start of the queue: the current entry starts again.
    Task Previous();

    // Clamped to 0 to 1.
    Task SetVolume(double volume);

    // Stops, releases the file and empties the queue.
    Task Clear();
}
