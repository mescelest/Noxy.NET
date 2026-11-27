namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Keeps a tag write from replacing the file the player is decoding: the write copies the file and swaps the copy over it, which breaks a decode that is under way. The player says which media file it uses; a write asks first.
public interface IFileUseService
{
    // Raised when the media file in use changes, so a write that was held back can be tried again.
    event Action? Changed;

    // Marks the media file the player is loading or has loaded (null: none). Waits first for a write that is already replacing that file.
    Task Use(Guid? mediaFileID);

    // True when the write may go ahead (every true is followed by EndWrite); false while the player uses the file.
    bool TryBeginWrite(Guid mediaFileID);

    void EndWrite(Guid mediaFileID);
}
