namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

public interface IArtService
{
    // Raised when an album's picture changed in the metadata sync, so what shows it can ask again.
    event Action? Changed;

    // A data:image/jpeg;base64 address for the album's picture, or null when the album has none, it cannot be had now, or it is not held. An album with no art time never causes a request; a picture is downloaded once for each version and kept on the device.
    Task<string?> GetAlbumArtUri(Guid albumID);
}
