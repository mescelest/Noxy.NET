namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IAlbumArtService
{
    // True when a picture was stored. Does nothing when the album already has a picture, is not the user's, or the picture cannot be turned into a thumbnail: a picture is only ever put into an album that has none.
    Task<bool> Apply(Guid userID, Guid albumID, byte[] picture, CancellationToken cancellationToken = default);

    // The user's own choice: the picture becomes the album's, whether it had one or not, and every device is told. Throws MetadataItemNotFoundException for an unknown album or another user's, and InvalidMetadataOperationException when the bytes are not an image.
    Task Replace(Guid userID, Guid albumID, byte[] picture, CancellationToken cancellationToken = default);
}
