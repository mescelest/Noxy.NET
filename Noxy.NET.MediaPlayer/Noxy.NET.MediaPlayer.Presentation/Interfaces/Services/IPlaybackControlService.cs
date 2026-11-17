using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// What every screen calls to play. A device that is a controller sends each action to the active device (a queue edit to the server) and changes nothing here until the pushed result arrives; a device that is the active one, or plays alone, does it on its own player and queue. When nobody is active the action runs here.
public interface IPlaybackControlService
{
    // A transfer this device asked for did not work.
    event Action? TransferFailed;

    // True when a saved queue is here to resume: this device is not a controller (it plays by itself, or it is still the session's active device after a restart), nothing plays, and the queue has a current item.
    bool CanContinue { get; }

    Task Play();

    Task Pause();

    // A controller sends one command after the changes of a drag have stopped for Constants.RemoteDeferMilliseconds; the local player changes at once on a device that is not a controller.
    Task Seek(TimeSpan position);

    Task Next();

    Task Previous();

    // 0 to 100. Deferred like Seek on a controller.
    Task SetVolume(int volume);

    Task SetShuffle(ShuffleModeEnum shuffle);

    Task SetRepeat(RepeatModeEnum repeat);

    // Ignored while no queue is loaded.
    Task PlayNext(Guid mediaFileID);

    Task AddToQueue(Guid mediaFileID);

    Task RemoveFromQueue(Guid itemID);

    // The item takes its place after the anchor; no anchor puts it at the front of the user queue.
    Task MoveInQueue(Guid itemID, Guid? afterItemID);

    // A click in the queue panel.
    Task JumpTo(Guid itemID, bool keepSkipped);

    Task StartLibrary(Guid mediaFileID, LibrarySortEnum sort, bool descending, string search);

    Task StartPlaylist(Guid playlistID, Guid? entryID = null);

    // Moves playback to a device of the account (a controller may name itself: "play here"). Throws InvalidOperationException, with the reason, when offline, with no live session, or when the server does not start it. Moving to the device that already plays does nothing.
    Task Transfer(Guid deviceID);

    // Resumes the saved queue at its saved position. False, with nothing done, when there is nothing to resume.
    Task<bool> Continue();
}
