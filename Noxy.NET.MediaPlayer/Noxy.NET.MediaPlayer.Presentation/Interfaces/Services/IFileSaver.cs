namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Lets the user pick where a file goes and writes the content there.
public interface IFileSaver
{
    // False when the user cancelled the dialog: nothing was written and that is not an error. The content is read to its end and not disposed.
    Task<bool> Save(string suggestedName, Stream content);
}
