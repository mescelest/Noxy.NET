using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeMediaRepository(FakeStore store) : IMediaRepository
{
    private readonly List<MediaFile> _pendingMediaFileList = [];
    private readonly List<MediaUploadJob> _pendingJobAddList = [];
    private readonly List<Guid> _pendingJobRemovalList = [];
    private readonly List<MediaFile> _pendingMediaFileUpdateList = [];
    private readonly List<Guid> _pendingMediaFileRemovalList = [];

    public void Flush()
    {
        store.MediaFileList.AddRange(_pendingMediaFileList);
        _pendingMediaFileList.Clear();

        foreach (MediaFile updated in _pendingMediaFileUpdateList)
        {
            int index = store.MediaFileList.FindIndex(x => x.ID == updated.ID);
            if (index >= 0) store.MediaFileList[index] = updated;
        }

        _pendingMediaFileUpdateList.Clear();

        store.MediaFileList.RemoveAll(x => _pendingMediaFileRemovalList.Contains(x.ID));

        // The database cascade: a removed song leaves every playlist.
        foreach (Playlist playlist in store.PlaylistList)
        {
            playlist.EntryList.RemoveAll(x => _pendingMediaFileRemovalList.Contains(x.MediaFileID));
        }

        _pendingMediaFileRemovalList.Clear();

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
        MediaFile? found = store.MediaFileList.FirstOrDefault(x => x.ID == id && x.UserID == userID);

        // A copy, like the database hands back, so an edit that is never committed changes nothing.
        return Task.FromResult(found is null ? null : Copy(found));
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
        return Task.FromResult(store.MediaFileList.Where(x => x.UserID == userID).OrderByDescending(x => x.TimeCreated).Skip(skip).Take(take).Select(Copy).ToList());
    }

    public Task<List<MediaFile>> GetMediaFileList(Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaFileList.Where(x => x.UserID == userID).OrderByDescending(x => x.TimeCreated).Select(Copy).ToList());
    }

    public Task<List<MediaFileStamp>> GetMediaFileStampList(Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaFileList.Where(x => x.UserID == userID).Select(x => new MediaFileStamp(x.ID, x.TimeModified)).ToList());
    }

    public void UpdateMediaFile(MediaFile mediaFile)
    {
        _pendingMediaFileUpdateList.Add(Copy(mediaFile));
    }

    public void RemoveMediaFile(MediaFile mediaFile)
    {
        _pendingMediaFileRemovalList.Add(mediaFile.ID);
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
        return Task.FromResult(next is null ? null : Copy(next));
    }

    public Task<bool> MarkMediaUploadJobProcessing(Guid id, CancellationToken cancellationToken = default)
    {
        if (store.LoseJobClaim) return Task.FromResult(false);

        MediaUploadJob? job = store.MediaUploadJobList.FirstOrDefault(x => x.ID == id && x.Status == MediaUploadJobStatus.Pending);
        if (job is null) return Task.FromResult(false);

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

    private static MediaFile Copy(MediaFile file)
    {
        MediaFile copy = new()
        {
            ID = file.ID,
            TimeCreated = file.TimeCreated,
            TimeModified = file.TimeModified,
            FilePath = file.FilePath,
            ContentType = file.ContentType,
            SizeBytes = file.SizeBytes,
            ContentHash = file.ContentHash,
            UserID = file.UserID
        };

        if (file.Metadata is not null)
        {
            copy.Metadata = new()
            {
                ID = file.Metadata.ID,
                TimeCreated = file.Metadata.TimeCreated,
                Title = file.Metadata.Title,
                Artist = file.Metadata.Artist,
                Album = file.Metadata.Album,
                AlbumArtist = file.Metadata.AlbumArtist,
                Subtitle = file.Metadata.Subtitle,
                Comment = file.Metadata.Comment,
                Genre = file.Metadata.Genre,
                TrackNumber = file.Metadata.TrackNumber,
                DiscNumber = file.Metadata.DiscNumber,
                Year = file.Metadata.Year,
                Duration = file.Metadata.Duration,
                MediaFileID = file.Metadata.MediaFileID
            };
        }

        return copy;
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
