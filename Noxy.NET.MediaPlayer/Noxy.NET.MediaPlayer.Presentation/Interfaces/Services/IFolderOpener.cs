namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Shows a file in the file manager of the platform, where there is one.
public interface IFolderOpener
{
    // False where the platform has no file manager to show a file in (Android): the menu offers saving a copy instead.
    bool IsSupported { get; }

    // Opens the folder of the file with the file selected. Throws IOException when the file manager cannot be started.
    Task Reveal(string path);
}
