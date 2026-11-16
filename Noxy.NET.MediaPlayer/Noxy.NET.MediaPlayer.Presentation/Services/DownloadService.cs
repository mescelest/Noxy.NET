using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class DownloadService(IApiClient clientApi, IFileStore storeFile, IHashService serviceHash, IMetadataStore storeMetadata, IDelay delay, ILogger<DownloadService> logger) : IDownloadService
{
    private const string HashMismatchError = "The downloaded file did not match its hash.";
    private const string InterruptedError = "The download was interrupted.";
    private const string NotOnServerError = "The server no longer has this media file.";
    private const string DiskFullError = "The device has no room for this file.";
    private const string UnexpectedError = "The download failed.";

    private readonly Lock _lock = new();
    private readonly Dictionary<Guid, (CancellationTokenSource Source, Task<bool> Task)> _jobMap = [];
    private readonly SemaphoreSlim _slots = new(Constants.MaxParallelDownloads, Constants.MaxParallelDownloads);

    // The bulk download, all guarded by _lock. The active set holds the media files queued or running in it; the workers pull from the queue until it is empty.
    private readonly Queue<Guid> _bulkQueue = new();
    private readonly HashSet<Guid> _bulkActiveSet = [];
    private int _bulkWorkerCount;
    private int _bulkTotal;
    private int _bulkFinished;
    private int _bulkFailed;
    private long _bulkBytesDone;
    private DownloadStopReason? _bulkStoppedReason;

    public event Action? Changed;

    public Task<bool> EnsureOnDevice(Guid mediaFileID)
    {
        lock (_lock)
        {
            if (_jobMap.TryGetValue(mediaFileID, out (CancellationTokenSource Source, Task<bool> Task) existing)) return existing.Task;

            // Task.Run, so the job cannot finish and remove itself before it is in the map.
            CancellationTokenSource source = new();
            Task<bool> task = Task.Run(() => Run(mediaFileID, source.Token));
            _jobMap[mediaFileID] = (source, task);

            return task;
        }
    }

    public Task Enqueue(List<Guid> mediaFileIDList)
    {
        foreach (Guid id in mediaFileIDList) _ = EnsureOnDevice(id);

        return Task.CompletedTask;
    }

    public async Task QueueMissing(List<Guid> mediaFileIDList)
    {
        List<ResponseMediaFile> neededList = [];
        foreach (Guid id in mediaFileIDList.Distinct())
        {
            ResponseMediaFile? media = await storeMetadata.GetMediaFile(id);
            if (media is not null && !await IsOnDevice(media)) neededList.Add(media);
        }

        int workersToStart;
        lock (_lock)
        {
            // A new bulk starts when the last one has nothing queued or running.
            if (_bulkActiveSet.Count == 0) ResetBulk();

            foreach (ResponseMediaFile media in neededList)
            {
                if (!_bulkActiveSet.Add(media.ID)) continue;

                _bulkQueue.Enqueue(media.ID);
                _bulkTotal++;
            }

            workersToStart = Math.Min(Constants.MaxParallelDownloads - _bulkWorkerCount, _bulkQueue.Count);
            _bulkWorkerCount += workersToStart;
        }

        if (neededList.Count > 0) Changed?.Invoke();

        for (int i = 0; i < workersToStart; i++) _ = Task.Run(BulkWorker);
    }

    public DownloadProgress GetProgress()
    {
        lock (_lock) return new(_bulkTotal, _bulkFinished, _bulkFailed, _bulkBytesDone, _bulkStoppedReason);
    }

    public async Task Cancel(Guid mediaFileID)
    {
        Task<bool>? task = null;
        lock (_lock)
        {
            if (_bulkQueue.Contains(mediaFileID))
            {
                List<Guid> kept = _bulkQueue.Where(x => x != mediaFileID).ToList();
                _bulkQueue.Clear();
                foreach (Guid id in kept) _bulkQueue.Enqueue(id);

                _bulkActiveSet.Remove(mediaFileID);
                _bulkTotal--;
            }

            if (_jobMap.TryGetValue(mediaFileID, out (CancellationTokenSource Source, Task<bool> Task) job))
            {
                job.Source.Cancel();
                task = job.Task;
            }
        }

        if (task is not null) await task;
    }

    // One of at most MaxParallelDownloads workers: takes the next queued media file, checks there is room for it, and downloads it.
    private async Task BulkWorker()
    {
        while (true)
        {
            Guid id;
            lock (_lock)
            {
                if (!_bulkQueue.TryDequeue(out id))
                {
                    _bulkWorkerCount--;
                    return;
                }
            }

            try
            {
                await DownloadForBulk(id);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "A bulk download ended with an unexpected error.");
                await EndInBulk(id, null, false);
            }
        }
    }

    private async Task DownloadForBulk(Guid id)
    {
        ResponseMediaFile? media = await storeMetadata.GetMediaFile(id);
        if (media is null)
        {
            await EndInBulk(id, null, false);
            return;
        }

        StorageEstimate estimate = await storeFile.GetEstimate();
        if (estimate.FreeBytes < media.SizeBytes + Constants.StorageReserveBytes)
        {
            StopBulk(id);
            return;
        }

        bool onDevice = await EnsureOnDevice(id);
        await EndInBulk(id, media, onDevice);
    }

    // Out of room: the media file that was about to start and every one still queued are given up on and stay NotDownloaded.
    private void StopBulk(Guid current)
    {
        lock (_lock)
        {
            _bulkStoppedReason = DownloadStopReason.StorageLow;
            _bulkActiveSet.Remove(current);
            foreach (Guid id in _bulkQueue) _bulkActiveSet.Remove(id);

            _bulkQueue.Clear();
        }

        Changed?.Invoke();
    }

    // A media file that ended not on the device counts as failed when its state says Failed; a cancelled or vanished one leaves the bulk instead.
    private async Task EndInBulk(Guid id, ResponseMediaFile? media, bool onDevice)
    {
        bool failed = !onDevice && media is not null && (await storeMetadata.GetFileStateList()).Any(x => x.MediaFileID == id && x.State == FileSyncState.Failed);

        lock (_lock)
        {
            _bulkActiveSet.Remove(id);

            if (onDevice && media is not null)
            {
                _bulkFinished++;
                _bulkBytesDone += media.SizeBytes;
            }
            else if (failed) _bulkFailed++;
            else _bulkTotal--;
        }

        Changed?.Invoke();
    }

    private void ResetBulk()
    {
        _bulkTotal = 0;
        _bulkFinished = 0;
        _bulkFailed = 0;
        _bulkBytesDone = 0;
        _bulkStoppedReason = null;
    }

    // Never throws: whatever goes wrong ends in the media file's state.
    private async Task<bool> Run(Guid id, CancellationToken token)
    {
        try
        {
            ResponseMediaFile? media = await storeMetadata.GetMediaFile(id);
            if (media is null) return false;
            if (await IsOnDevice(media)) return true;

            try
            {
                await _slots.WaitAsync(token);
            }
            catch (OperationCanceledException)
            {
                // Cancelled while waiting for a free place: it never started, so there is nothing to undo.
                return false;
            }

            try
            {
                return await Download(media, token);
            }
            finally
            {
                _slots.Release();
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "A download ended with an unexpected error.");
            return false;
        }
        finally
        {
            lock (_lock) _jobMap.Remove(id);
        }
    }

    private async Task<bool> IsOnDevice(ResponseMediaFile media)
    {
        FileState? state = (await storeMetadata.GetFileStateList()).FirstOrDefault(x => x.MediaFileID == media.ID);

        return state is { State: FileSyncState.OnDevice }
               && string.Equals(state.VerifiedHash, media.ContentHash, StringComparison.OrdinalIgnoreCase)
               && await storeFile.GetPath(media.ID) is not null;
    }

    private async Task<bool> Download(ResponseMediaFile media, CancellationToken token)
    {
        Guid id = media.ID;
        await SetState(new(id, FileSyncState.Downloading, null, null));

        // The ETag of the bytes in the partial file, set as soon as a response arrives so that a download cut off mid-stream can still resume with If-Range.
        StrongBox<string?> eTag = new();
        string error = UnexpectedError;

        try
        {
            for (int attempt = 1; attempt <= Constants.MaxDownloadAttempts; attempt++)
            {
                try
                {
                    if (await TryDownload(media, eTag, token)) return true;

                    error = HashMismatchError;
                }
                catch (StorageFullException)
                {
                    return await Fail(id, DiskFullError);
                }
                catch (ApiException exception) when (exception.Status is HttpStatusCode.NotFound or HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
                {
                    return await Fail(id, exception.Status == HttpStatusCode.NotFound ? NotOnServerError : exception.Title);
                }
                catch (Exception exception) when (!token.IsCancellationRequested)
                {
                    // A range the server cannot serve means the partial file is longer than the file: start again.
                    if (exception is ApiException { Status: HttpStatusCode.RequestedRangeNotSatisfiable })
                    {
                        await storeFile.DiscardPartial(id);
                        eTag.Value = null;
                    }

                    logger.LogWarning(exception, "Download attempt {Attempt} of {MaxAttempts} failed.", attempt, Constants.MaxDownloadAttempts);
                    error = InterruptedError;
                }

                if (attempt < Constants.MaxDownloadAttempts) await delay.Wait(TimeSpan.FromSeconds(Constants.RetryBaseDelaySeconds * Math.Pow(2, attempt - 1)), token);
            }

            return await Fail(id, error);
        }
        catch (Exception) when (token.IsCancellationRequested)
        {
            await storeFile.DiscardPartial(id);
            await storeMetadata.DeleteFileState(id);
            Changed?.Invoke();

            return false;
        }
    }

    // One request: resumes from the partial file when there is one the ETag vouches for, builds the partial file, and commits it only when its SHA-256 equals the library entry's contentHash.
    private async Task<bool> TryDownload(ResponseMediaFile media, StrongBox<string?> eTag, CancellationToken token)
    {
        Guid id = media.ID;
        long partial = await storeFile.GetPartialLength(id);
        if (partial > 0 && eTag.Value is null)
        {
            await storeFile.DiscardPartial(id);
            partial = 0;
        }

        using ApiFileResponse response = await clientApi.GetFile(id, partial > 0 ? partial : null, partial > 0 ? eTag.Value : null);
        eTag.Value = response.ETag ?? eTag.Value;

        // Cancelling closes the stream, which wakes a read that is waiting on the network.
        using CancellationTokenRegistration registration = token.Register(response.Dispose);

        // The server answered 200 to a ranged request: the file changed, so the bytes held are another version.
        if (partial > 0 && response.Status != HttpStatusCode.PartialContent) await storeFile.DiscardPartial(id);

        await storeFile.AppendPartial(id, response.Content);
        token.ThrowIfCancellationRequested();

        string hash;
        await using (Stream stream = await storeFile.OpenPartial(id)) hash = await serviceHash.Sha256Hex(stream, token);

        if (!string.Equals(hash, media.ContentHash, StringComparison.OrdinalIgnoreCase))
        {
            await storeFile.DiscardPartial(id);
            eTag.Value = null;
            return false;
        }

        await storeFile.Commit(id, media.ContentType);
        await SetState(new(id, FileSyncState.OnDevice, hash, null));

        return true;
    }

    private async Task<bool> Fail(Guid id, string error)
    {
        await storeFile.DiscardPartial(id);
        await SetState(new(id, FileSyncState.Failed, null, error));

        return false;
    }

    private async Task SetState(FileState state)
    {
        await storeMetadata.SaveFileState(state);
        Changed?.Invoke();
    }
}
