using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

public interface IMediaRepository
{
    void AddMediaFile(MediaFile mediaFile);

    // Scoped to the owner: another user's file comes back as null, the same as a file that doesn't exist.
    Task<MediaFile?> FindMediaFile(Guid id, Guid userID, CancellationToken cancellationToken = default);

    Task<int> CountMediaFiles(Guid userID, CancellationToken cancellationToken = default);

    Task<long> SumMediaFileBytes(Guid userID, CancellationToken cancellationToken = default);

    // Newest first.
    Task<List<MediaFile>> GetMediaFileList(Guid userID, int skip, int take, CancellationToken cancellationToken = default);

    void AddMediaUploadJob(MediaUploadJob job);

    // Tracked, so it commits together with whatever else the unit of work holds.
    void RemoveMediaUploadJob(MediaUploadJob job);

    Task<int> CountMediaUploadJobs(Guid userID, CancellationToken cancellationToken = default);

    Task<long> SumMediaUploadJobBytes(Guid userID, CancellationToken cancellationToken = default);

    // Oldest first across all users.
    Task<MediaUploadJob?> FindNextPendingMediaUploadJob(CancellationToken cancellationToken = default);

    // Runs immediately, and is true only when the job was still pending.
    Task<bool> MarkMediaUploadJobProcessing(Guid id, CancellationToken cancellationToken = default);

    // Runs immediately.
    Task<int> ResetProcessingMediaUploadJobs(CancellationToken cancellationToken = default);

    Task<List<string>> GetMediaUploadJobFileNameList(CancellationToken cancellationToken = default);
}
