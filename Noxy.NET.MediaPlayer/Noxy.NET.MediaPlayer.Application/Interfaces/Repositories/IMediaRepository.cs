using Noxy.NET.MediaPlayer.Application.Models.Media;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

public interface IMediaRepository
{
    void AddMediaFile(EntityMediaFile mediaFile);

    // Scoped to the owner: another user's file comes back as null, the same as a file that doesn't exist.
    Task<EntityMediaFile?> FindMediaFile(Guid id, Guid userID, CancellationToken cancellationToken = default);

    Task<int> CountMediaFiles(Guid userID, CancellationToken cancellationToken = default);

    Task<long> SumMediaFileBytes(Guid userID, CancellationToken cancellationToken = default);

    // Newest first.
    Task<List<EntityMediaFile>> GetMediaFileList(Guid userID, int skip, int take, CancellationToken cancellationToken = default);

    // The whole library, newest first.
    Task<List<EntityMediaFile>> GetMediaFileList(Guid userID, CancellationToken cancellationToken = default);

    // Only the ID and modified time of each file, which is all the library ETag needs.
    Task<List<MediaFileStamp>> GetMediaFileStampList(Guid userID, CancellationToken cancellationToken = default);

    // Saves the file row and its metadata row, together with whatever else the unit of work holds.
    void UpdateMediaFile(EntityMediaFile mediaFile);

    // Tracked, so it commits with the rest of the unit of work. The metadata row goes with the file.
    void RemoveMediaFile(EntityMediaFile mediaFile);

    void AddMediaUploadJob(EntityMediaUploadJob job);

    // Tracked, so it commits together with whatever else the unit of work holds.
    void RemoveMediaUploadJob(EntityMediaUploadJob job);

    Task<int> CountMediaUploadJobs(Guid userID, CancellationToken cancellationToken = default);

    Task<long> SumMediaUploadJobBytes(Guid userID, CancellationToken cancellationToken = default);

    // Oldest first across all users.
    Task<EntityMediaUploadJob?> FindNextPendingMediaUploadJob(CancellationToken cancellationToken = default);

    // Runs immediately, and is true only when the job was still pending.
    Task<bool> MarkMediaUploadJobProcessing(Guid id, CancellationToken cancellationToken = default);

    // Runs immediately.
    Task<int> ResetProcessingMediaUploadJobs(CancellationToken cancellationToken = default);

    Task<List<string>> GetMediaUploadJobFileNameList(CancellationToken cancellationToken = default);
}
