using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeMediaRepository(FakeStore store) : IMediaRepository
{
    private readonly List<MediaFile> _pendingMediaFiles = [];
    private readonly List<MediaUploadJob> _pendingJobAdds = [];
    private readonly List<Guid> _pendingJobRemovals = [];

    public void Flush()
    {
        store.MediaFiles.AddRange(_pendingMediaFiles);
        _pendingMediaFiles.Clear();

        store.MediaUploadJobs.AddRange(_pendingJobAdds);
        _pendingJobAdds.Clear();

        store.MediaUploadJobs.RemoveAll(x => _pendingJobRemovals.Contains(x.ID));
        _pendingJobRemovals.Clear();
    }

    public void AddMediaFile(MediaFile mediaFile)
    {
        _pendingMediaFiles.Add(mediaFile);
    }

    public Task<MediaFile?> FindMediaFile(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaFiles.FirstOrDefault(x => x.ID == id && x.UserID == userID));
    }

    public Task<int> CountMediaFiles(Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaFiles.Count(x => x.UserID == userID));
    }

    public Task<long> SumMediaFileBytes(Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaFiles.Where(x => x.UserID == userID).Sum(x => x.SizeBytes));
    }

    public Task<List<MediaFile>> ListMediaFiles(Guid userID, int skip, int take, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaFiles.Where(x => x.UserID == userID).OrderByDescending(x => x.TimeCreated).Skip(skip).Take(take).ToList());
    }

    public void AddMediaUploadJob(MediaUploadJob job)
    {
        _pendingJobAdds.Add(job);
    }

    public void RemoveMediaUploadJob(MediaUploadJob job)
    {
        _pendingJobRemovals.Add(job.ID);
    }

    public Task<int> CountMediaUploadJobs(Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaUploadJobs.Count(x => x.UserID == userID));
    }

    public Task<long> SumMediaUploadJobBytes(Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaUploadJobs.Where(x => x.UserID == userID).Sum(x => x.SizeBytes));
    }

    public Task<MediaUploadJob?> FindNextPendingMediaUploadJob(CancellationToken cancellationToken = default)
    {
        MediaUploadJob? next = store.MediaUploadJobs
            .Where(x => x.Status == MediaUploadJobStatus.Pending)
            .OrderBy(x => x.TimeCreated)
            .ThenBy(x => x.ID)
            .FirstOrDefault();

        // A copy, like the database hands back, so changing the store later doesn't change what the caller holds.
        return Task.FromResult(next == null ? null : Copy(next));
    }

    public Task<bool> MarkMediaUploadJobProcessing(Guid id, CancellationToken cancellationToken = default)
    {
        if (store.LoseJobClaim) return Task.FromResult(false);

        MediaUploadJob? job = store.MediaUploadJobs.FirstOrDefault(x => x.ID == id && x.Status == MediaUploadJobStatus.Pending);
        if (job == null) return Task.FromResult(false);

        job.Status = MediaUploadJobStatus.Processing;
        return Task.FromResult(true);
    }

    public Task<int> ResetProcessingMediaUploadJobs(CancellationToken cancellationToken = default)
    {
        List<MediaUploadJob> processing = store.MediaUploadJobs.Where(x => x.Status == MediaUploadJobStatus.Processing).ToList();
        foreach (MediaUploadJob job in processing)
        {
            job.Status = MediaUploadJobStatus.Pending;
        }

        return Task.FromResult(processing.Count);
    }

    public Task<List<string>> ListMediaUploadJobFileNames(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaUploadJobs.Select(x => x.StagedFileName).ToList());
    }

    private static MediaUploadJob Copy(MediaUploadJob job)
    {
        return new()
        {
            ID = job.ID,
            TimeCreated = job.TimeCreated,
            UserID = job.UserID,
            StagedFileName = job.StagedFileName,
            Title = job.Title,
            ContentType = job.ContentType,
            SizeBytes = job.SizeBytes,
            ContentHash = job.ContentHash,
            Status = job.Status
        };
    }
}
