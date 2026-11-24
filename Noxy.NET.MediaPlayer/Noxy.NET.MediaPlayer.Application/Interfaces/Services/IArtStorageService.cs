namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IArtStorageService
{
    // Written to a temporary name and moved into place, so a reader never sees half a file. Replaces a picture that is there.
    Task SaveAlbumArt(Guid albumID, byte[] jpeg, CancellationToken cancellationToken = default);

    // The caller owns the stream. Null when the album has no picture.
    Stream? OpenAlbumArt(Guid albumID);

    // Safe to repeat: a picture that is already gone is left alone.
    void DeleteAlbumArt(Guid albumID);
}
