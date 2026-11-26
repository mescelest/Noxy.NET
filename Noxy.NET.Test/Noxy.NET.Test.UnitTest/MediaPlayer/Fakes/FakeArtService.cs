using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeArtService : IArtService
{
    public event Action? Changed;

    // The picture of each album; an album that is not in the map, or maps to null, has none to give.
    public Dictionary<Guid, byte[]?> PictureMap { get; } = [];

    // The albums whose picture was asked for, in order.
    public List<Guid> RequestList { get; } = [];

    public Task<string?> GetAlbumArtUri(Guid albumID)
    {
        return Task.FromResult(PictureMap.GetValueOrDefault(albumID) is { } bytes ? $"data:image/jpeg;base64,{Convert.ToBase64String(bytes)}" : null);
    }

    public Task<byte[]?> GetAlbumArt(Guid albumID)
    {
        RequestList.Add(albumID);

        return Task.FromResult(PictureMap.GetValueOrDefault(albumID));
    }

    public void RaiseChanged()
    {
        Changed?.Invoke();
    }
}
