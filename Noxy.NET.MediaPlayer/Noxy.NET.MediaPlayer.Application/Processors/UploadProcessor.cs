using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Processors;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Processors;

public class UploadProcessor(IUnitOfWorkFactory factoryUnitOfWork, IFileStorageService serviceFileStorage, IPublisher publisher, IOptions<ConfigStorage> configStorage, ILogger<UploadProcessor> logger) : IUploadProcessor
{
    public async Task<bool> ProcessNext(CancellationToken cancellationToken = default)
    {
        MediaUploadJob job;

        await using (IUnitOfWork claim = factoryUnitOfWork.Create())
        {
            MediaUploadJob? next = await claim.Media.FindNextPendingMediaUploadJob(cancellationToken);
            if (next is null) return false;

            // Someone else got there first: nothing to do for this one, but there may be more.
            if (!await claim.Media.MarkMediaUploadJobProcessing(next.ID, cancellationToken)) return true;

            job = next;
        }

        ResponseUploadResult? result;
        try
        {
            result = await Complete(job, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Shutting down: the job stays Processing and is picked up again after the restart.
            throw;
        }
        catch (Exception ex)
        {
            result = await Fail(job, ex, cancellationToken);
        }

        // No result means the job could not be consumed and will be tried again, so there is nothing final to report yet.
        if (result is not null) await PublishResult(job.UserID, result, cancellationToken);
        return true;
    }

    public async Task Recover(CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        int reset = await uow.Media.ResetProcessingMediaUploadJobs(cancellationToken);
        HashSet<string> jobFileNameSet = [.. await uow.Media.GetMediaUploadJobFileNameList(cancellationToken)];

        int orphans = 0;
        foreach (string staged in serviceFileStorage.GetStagedFileNameList().Where(x => !jobFileNameSet.Contains(x)))
        {
            serviceFileStorage.DeleteStaged(staged);
            orphans++;
        }

        logger.LogInformation("Upload recovery: {Reset} interrupted jobs queued again, {Orphans} orphaned staged files deleted", reset, orphans);
    }

    private async Task<ResponseUploadResult> Complete(MediaUploadJob job, CancellationToken cancellationToken)
    {
        await EnsureRoomInLibrary(job, cancellationToken);

        serviceFileStorage.Promote(job.StagedFileName);

        MediaFile file = new()
        {
            FilePath = job.StagedFileName,
            ContentType = job.ContentType,
            SizeBytes = job.SizeBytes,
            ContentHash = job.ContentHash,
            UserID = job.UserID
        };
        file.Metadata = new()
        {
            Title = job.Title,
            MediaFileID = file.ID
        };

        // One commit, so a crash can't leave a library entry without its job being consumed, or the other way round.
        await using IUnitOfWork uow = factoryUnitOfWork.Create();
        uow.Media.AddMediaFile(file);
        uow.Media.RemoveMediaUploadJob(job);
        await uow.Commit(cancellationToken);

        return new()
        {
            JobID = job.ID,
            Title = job.Title,
            Succeeded = true,
            Media = file.ToResponse()
        };
    }

    private async Task EnsureRoomInLibrary(MediaUploadJob job, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        if (await uow.Media.CountMediaFiles(job.UserID, cancellationToken) >= configStorage.Value.MaxFilesPerUser) throw new LibraryFullException();
        if (configStorage.Value.MaxBytesPerUser == 0) return;

        long used = await uow.Media.SumMediaFileBytes(job.UserID, cancellationToken);
        if (used + job.SizeBytes > configStorage.Value.MaxBytesPerUser) throw new LibraryFullException();
    }

    private async Task<ResponseUploadResult?> Fail(MediaUploadJob job, Exception exception, CancellationToken cancellationToken)
    {
        UploadFailureReason reason = exception switch
        {
            LibraryFullException => UploadFailureReason.LibraryFull,
            FileNotFoundException => UploadFailureReason.FileMissing,
            _ => UploadFailureReason.Unknown
        };

        if (reason == UploadFailureReason.Unknown) logger.LogError(exception, "Upload job {JobID} failed", job.ID);
        else logger.LogInformation("Upload job {JobID} was rejected: {Reason}", job.ID, reason);

        // The job goes first. If that fails it stays queued with its files, so the next start tries it again.
        try
        {
            await using IUnitOfWork uow = factoryUnitOfWork.Create();
            uow.Media.RemoveMediaUploadJob(job);
            await uow.Commit(cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Could not remove upload job {JobID}, so it stays queued", job.ID);
            return null;
        }

        // The name is unique, so a library file with it can only be one this job promoted before it failed.
        DeleteQuietly(() => serviceFileStorage.DeleteStaged(job.StagedFileName), job);
        DeleteQuietly(() => serviceFileStorage.Delete(job.StagedFileName), job);

        return new()
        {
            JobID = job.ID,
            Title = job.Title,
            Succeeded = false,
            Reason = reason
        };
    }

    // Each deletion stands alone, so one failing can't stop the other.
    private void DeleteQuietly(Action delete, MediaUploadJob job)
    {
        try
        {
            delete();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not delete a file of upload job {JobID}", job.ID);
        }
    }

    private ValueTask PublishResult(Guid userID, ResponseUploadResult result, CancellationToken cancellationToken)
    {
        return result.Succeeded ? publisher.Publish(new NotificationUploadSucceeded(userID, result), cancellationToken) : publisher.Publish(new NotificationUploadFailed(userID, result), cancellationToken);
    }
}
