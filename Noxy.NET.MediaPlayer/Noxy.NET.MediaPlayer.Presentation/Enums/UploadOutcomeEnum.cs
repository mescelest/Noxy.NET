namespace Noxy.NET.MediaPlayer.Presentation.Enums;

public enum UploadOutcomeEnum
{
    // A new file, now in the library (and in the playlist when it was an upload to one).
    Added,

    // The library already held the file and nothing was added anywhere.
    AlreadyInLibrary,

    // The library already held the file, and it was added to the playlist.
    AlreadyInLibraryAddedToPlaylist,

    // The file did not make it; the failure says why.
    Failed
}
