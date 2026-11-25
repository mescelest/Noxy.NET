using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Lets the user choose a picture from the device, with the dialog of the platform and not the one of the web view.
public interface IPicturePicker
{
    // Null when the user cancelled the dialog, which is not an error. A picture larger than PresentationConstants.AlbumArtMaxBytes is not read: an IOException says so.
    Task<PickedPicture?> Pick();
}
