using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Presentation.Enums;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Plays the current item of the queue (IQueueService) from the file store only: a media file that is on the device is played with no network call, one that is missing is downloaded first. The same media file can be in the queue twice.
public interface IPlayerService
{
    // Raised when the queue, the current item, the play state or the position changed.
    event Action? Changed;

    // The queue's current item; null when nothing is loaded.
    QueueItem? Current { get; }

    LiveQueue Queue { get; }

    bool IsPlaying { get; }

    // No current item: the player has nothing to play.
    bool IsDisabled { get; }

    TimeSpan Position { get; }

    TimeSpan Duration { get; }

    double Volume { get; }

    // True while the current item is being downloaded before it can play.
    bool IsWaiting { get; }

    // The modes that apply to the queue: the playing playlist's own when it has one, else the user's setting.
    ShuffleModeEnum Shuffle { get; }

    RepeatModeEnum Repeat { get; }

    // The playlist the queue plays, null for the Library or an empty queue.
    Guid? PlaylistID { get; }

    // Set by Seek so that a seek counts as a change to report; the reporter clears it.
    bool PositionJumped { get; set; }

    // What the file of an item is like, or Failed when it could not be played.
    FileSyncStateEnum StateOf(QueueItem item);

    // Builds the queue from the Library (the search and sort as the screen shows them) in the effective shuffle mode, starting at that media file, and plays it.
    Task StartLibrary(Guid mediaFileID, LibrarySortEnum sort, bool descending, string search);

    // The same for a playlist, from the entry (the first one when null). Reports the playlist as played once it starts; offline that is ignored.
    Task StartPlaylist(Guid playlistID, Guid? startEntryID = null);

    // Plays the queue's current item from the start (Continue, a transfer, a jump). Does nothing without one.
    Task PlayCurrent();

    Task Pause();

    // Does nothing when nothing is loaded.
    Task Resume();

    Task Seek(TimeSpan position);

    // Jumps to an item of the queue and plays it: one ahead (the user queue or the source part) is advanced to, one behind (history) is stepped back to. The items skipped on the way are dropped, or kept in history when keepSkipped is set. An item that is not in the queue does nothing.
    Task JumpTo(Guid itemID, bool keepSkipped);

    // The next playable item: the user queue first, then the source part. At the end the repeat mode decides; with no repeat nothing happens.
    Task Next();

    // Past 3 seconds, or while the current media file is deleted, the current item starts again; otherwise the last played item plays.
    Task Previous();

    // Reads the volume stored on this device, once, so Volume is right before anything has played. Does nothing after the user set a volume on this device.
    Task LoadVolume();

    // Clamped to 0 to 1. Applied at once and kept on the device.
    Task SetVolume(double volume);

    // Clamped to 0 to 1. Applied at once but not saved: for each step of a drag, with SetVolume called once at the end.
    Task PreviewVolume(double volume);

    // Writes the playing playlist's own mode (the global setting for the Library) and builds the source part again from the current item. Throws when the server cannot be reached or refuses; the mode is unchanged then.
    Task SetShuffle(ShuffleModeEnum shuffle);

    Task SetRepeat(RepeatModeEnum repeat);

    // Stops, releases the file and empties the queue and its local copy.
    Task Clear();
}
