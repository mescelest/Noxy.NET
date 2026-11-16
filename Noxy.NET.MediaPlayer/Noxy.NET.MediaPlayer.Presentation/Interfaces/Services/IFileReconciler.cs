using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Keeps the files on the device in line with the library: a media file that left the library or changed its bytes loses its file, and a start-up repair makes the stored states tell the truth about what is on the disk.
public interface IFileReconciler
{
    // A removed media file loses its file, partial file, state and running download. A media file with a new hash loses its file and partial file and its running download, and goes back to NotDownloaded. An added media file changes nothing.
    Task Apply(LibraryDiff diff);

    // For start-up. Every partial file is deleted (none is resumed across a restart), and so is a state Downloading left by an earlier run. A state OnDevice whose file is missing becomes NotDownloaded. A held file is kept only when its media file is in the library and its state is OnDevice with the library's current hash; any other held file is deleted. Never reports a missing file as available.
    Task Repair();
}
