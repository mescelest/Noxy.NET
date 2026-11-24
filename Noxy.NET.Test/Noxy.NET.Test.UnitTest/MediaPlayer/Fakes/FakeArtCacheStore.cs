using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeArtCacheStore : IArtCacheStore
{
    public Dictionary<Guid, (DateTime Version, byte[] Bytes)> CopyList { get; } = [];
    public List<Guid> DeletedList { get; } = [];
    public int SaveCount { get; private set; }
    public int ClearCount { get; private set; }

    public Task<byte[]?> Get(Guid albumID, DateTime version)
    {
        return Task.FromResult(CopyList.TryGetValue(albumID, out (DateTime Version, byte[] Bytes) copy) && copy.Version == version ? copy.Bytes : null);
    }

    public Task Save(Guid albumID, DateTime version, byte[] bytes)
    {
        SaveCount++;
        CopyList[albumID] = (version, bytes);
        return Task.CompletedTask;
    }

    public Task Delete(Guid albumID)
    {
        DeletedList.Add(albumID);
        CopyList.Remove(albumID);
        return Task.CompletedTask;
    }

    public Task Clear()
    {
        ClearCount++;
        CopyList.Clear();
        return Task.CompletedTask;
    }
}
