using Noxy.NET.MediaPlayer.Application.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeArtStorageService : IArtStorageService
{
    public Dictionary<Guid, byte[]> AlbumArtList { get; } = [];
    public List<Guid> DeletedAlbumIDList { get; } = [];
    public Exception? Failure { get; set; }

    public Task SaveAlbumArt(Guid albumID, byte[] jpeg, CancellationToken cancellationToken = default)
    {
        if (Failure is not null) throw Failure;

        AlbumArtList[albumID] = jpeg;
        return Task.CompletedTask;
    }

    public Stream? OpenAlbumArt(Guid albumID)
    {
        return AlbumArtList.TryGetValue(albumID, out byte[]? bytes) ? new MemoryStream(bytes) : null;
    }

    public void DeleteAlbumArt(Guid albumID)
    {
        DeletedAlbumIDList.Add(albumID);
        AlbumArtList.Remove(albumID);
    }
}
