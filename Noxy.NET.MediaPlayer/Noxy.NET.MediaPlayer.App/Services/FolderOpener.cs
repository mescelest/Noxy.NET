using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.MediaPlayer.App.Services;

public class FolderOpener : IFolderOpener
{
#if WINDOWS
    public bool IsSupported => true;

    // Explorer opens the folder with the file selected; its argument keeps the quotes around a path with spaces.
    public Task Reveal(string path)
    {
        try
        {
            using System.Diagnostics.Process? process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", $"/select,\"{Path.GetFullPath(path)}\"") { UseShellExecute = false });
        }
        catch (System.ComponentModel.Win32Exception exception)
        {
            throw new IOException("The file manager could not be started.", exception);
        }

        return Task.CompletedTask;
    }
#else
    // Android has no file manager to show a file in: the menu offers saving a copy instead.
    public bool IsSupported => false;

    public Task Reveal(string path)
    {
        throw new NotSupportedException("This platform cannot show a file in a file manager.");
    }
#endif
}
