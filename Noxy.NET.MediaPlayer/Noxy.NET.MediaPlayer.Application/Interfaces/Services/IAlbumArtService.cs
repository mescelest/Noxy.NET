namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IAlbumArtService
{
    // True when a picture was stored. Does nothing when the album already has a picture, is not the user's, or the picture cannot be turned into a thumbnail: a picture is only ever put into an album that has none.
    Task<bool> Apply(Guid userID, Guid albumID, byte[] picture, CancellationToken cancellationToken = default);

    // A song moved to the album of the same title with other album artists keeps the look of its album: copies the picture of the first album into the second when the first has one and the second has none, and tells every device. True when copied. Does nothing for an unknown album, another user's, or a picture that cannot be read.
    Task<bool> Inherit(Guid userID, Guid fromAlbumID, Guid toAlbumID, CancellationToken cancellationToken = default);

    // The user's own choice: the picture becomes the album's, whether it had one or not, and every device is told. Throws MetadataItemNotFoundException for an unknown album or another user's, and InvalidMetadataOperationException when the bytes are not an image.
    Task Replace(Guid userID, Guid albumID, byte[] picture, CancellationToken cancellationToken = default);
}
