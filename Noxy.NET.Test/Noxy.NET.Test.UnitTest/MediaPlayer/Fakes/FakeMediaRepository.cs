using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeMediaRepository(FakeStore store) : IMediaRepository
{
    private readonly List<MediaFile> _pendingMediaFileList = [];
    private readonly List<MediaUploadJob> _pendingJobAddList = [];
    private readonly List<Guid> _pendingJobRemovalList = [];

    public void Flush()
    {
        store.MediaFileList.AddRange(_pendingMediaFileList);
        _pendingMediaFileList.Clear();

        store.MediaUploadJobList.AddRange(_pendingJobAddList);
        _pendingJobAddList.Clear();

        store.MediaUploadJobList.RemoveAll(x => _pendingJobRemovalList.Contains(x.ID));
        _pendingJobRemovalList.Clear();
    }

    public void AddMediaFile(MediaFile mediaFile)
    {
        _pendingMediaFileList.Add(mediaFile);
    }

    public Task<MediaFile?> FindMediaFile(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaFileList.FirstOrDefault(x => x.ID == id && x.UserID == userID));
    }

    public Task<int> CountMediaFiles(Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaFileList.Count(x => x.UserID == userID));
    }

    public Task<long> SumMediaFileBytes(Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaFileList.Where(x => x.UserID == userID).Sum(x => x.SizeBytes));
    }

    public Task<List<MediaFile>> GetMediaFileList(Guid userID, int skip, int take, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaFileList.Where(x => x.UserID == userID).OrderByDescending(x => x.TimeCreated).Skip(skip).Take(take).ToList());
    }

    public void AddMediaUploadJob(MediaUploadJob job)
    {
        _pendingJobAddList.Add(job);
    }

    public void RemoveMediaUploadJob(MediaUploadJob job)
    {
        _pendingJobRemovalList.Add(job.ID);
    }

    public Task<int> CountMediaUploadJobs(Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaUploadJobList.Count(x => x.UserID == userID));
    }

    public Task<long> SumMediaUploadJobBytes(Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaUploadJobList.Where(x => x.UserID == userID).Sum(x => x.SizeBytes));
    }

    public Task<MediaUploadJob?> FindNextPendingMediaUploadJob(CancellationToken cancellationToken = default)
    {
        MediaUploadJob? next = store.MediaUploadJobList
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

        MediaUploadJob? job = store.MediaUploadJobList.FirstOrDefault(x => x.ID == id && x.Status == MediaUploadJobStatus.Pending);
        if (job == null) return Task.FromResult(false);

        job.Status = MediaUploadJobStatus.Processing;
        return Task.FromResult(true);
    }

    public Task<int> ResetProcessingMediaUploadJobs(CancellationToken cancellationToken = default)
    {
        List<MediaUploadJob> processing = store.MediaUploadJobList.Where(x => x.Status == MediaUploadJobStatus.Processing).ToList();
        foreach (MediaUploadJob job in processing)
        {
            job.Status = MediaUploadJobStatus.Pending;
        }

        return Task.FromResult(processing.Count);
    }

    public Task<List<string>> GetMediaUploadJobFileNameList(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaUploadJobList.Select(x => x.StagedFileName).ToList());
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
