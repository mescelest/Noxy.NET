namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

// The album pictures this device keeps, one per album, each with the version (the album's art time) it was downloaded for.
public interface IArtCacheStore
{
    // Null when there is no copy for that version: none at all, or one of another version.
    Task<byte[]?> Get(Guid albumID, DateTime version);

    // Replaces any copy of the album, whatever its version.
    Task Save(Guid albumID, DateTime version, byte[] bytes);

    // An album with no copy is harmless.
    Task Delete(Guid albumID);

    // Every copy: the pictures belong to the account that was signed in, so a wipe of the local data takes them too.
    Task Clear();
}
