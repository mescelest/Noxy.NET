using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

public interface IMediaRepository
{
    void AddMediaFile(MediaFile mediaFile);

    // Scoped to the owner: another user's file comes back as null, the same as a file that doesn't exist.
    Task<MediaFile?> FindMediaFile(Guid id, Guid userID, CancellationToken cancellationToken = default);

    // Newest first.
    Task<List<MediaFile>> ListMediaFiles(Guid userID, int skip, int take, CancellationToken cancellationToken = default);
}
