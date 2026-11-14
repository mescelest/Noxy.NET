using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeMediaRepository(FakeStore store) : IMediaRepository
{
    private readonly List<MediaFile> _pendingMediaFiles = [];

    public void Flush()
    {
        store.MediaFiles.AddRange(_pendingMediaFiles);
        _pendingMediaFiles.Clear();
    }

    public void AddMediaFile(MediaFile mediaFile)
    {
        _pendingMediaFiles.Add(mediaFile);
    }

    public Task<MediaFile?> FindMediaFile(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaFiles.FirstOrDefault(x => x.ID == id && x.UserID == userID));
    }

    public Task<List<MediaFile>> ListMediaFiles(Guid userID, int skip, int take, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaFiles.Where(x => x.UserID == userID).OrderByDescending(x => x.TimeCreated).Skip(skip).Take(take).ToList());
    }
}
