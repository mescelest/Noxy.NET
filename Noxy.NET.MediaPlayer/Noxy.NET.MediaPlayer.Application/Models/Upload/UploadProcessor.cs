using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Models.Upload;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Models.Upload;

public class UploadProcessor(
    IUnitOfWorkFactory factoryUnitOfWork,
    IFileStorageService serviceFileStorage,
    IMediaTagReadingService serviceMediaTagReading,
    IMetadataResolutionService serviceMetadataResolution,
    IAlbumArtService serviceAlbumArt,
    IPublisher publisher,
    IOptions<ConfigStorage> configStorage,
    ILogger<UploadProcessor> logger) : IUploadProcessor
{
    private const int MaxTitleLength = 255;
    private const int MaxSubtitleLength = 256;
    private const int MaxCommentLength = 2048;
    private const int MaxTagNumber = 9999;

    public async Task<bool> ProcessNext(CancellationToken cancellationToken = default)
    {
        EntityMediaUploadJob job;

        await using (IUnitOfWork claim = factoryUnitOfWork.Create())
        {
            EntityMediaUploadJob? next = await claim.Media.FindNextPendingMediaUploadJob(cancellationToken);
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


    private async Task<ResponseUploadResult> Complete(EntityMediaUploadJob job, CancellationToken cancellationToken)
    {
        await EnsureNotInLibrary(job, cancellationToken);
        await EnsureRoomInLibrary(job, cancellationToken);

        serviceFileStorage.Promote(job.StagedFileName);

        MediaTagSet tags = ReadTags(job, cancellationToken);

        // One commit, so a crash can't leave a library entry without its job being consumed, or the other way round. Another upload may create the same artist first, and then the whole thing runs again and reuses it.
        (ResponseUploadResult result, MetadataChangeSet changeSet) = await factoryUnitOfWork.RunWithKeyConflictRetry(async uow =>
        {
            ResolvedMetadata resolved = await serviceMetadataResolution.Resolve(uow, job.UserID, tags, cancellationToken);

            EntityMediaFile file = new()
            {
                FilePath = job.StagedFileName,
                ContentType = job.ContentType,
                SizeBytes = job.SizeBytes,
                ContentHash = job.ContentHash,
                UserID = job.UserID,
                AlbumID = resolved.Album?.ID,
                Album = resolved.Album
            };
            file.Metadata = new()
            {
                Title = ChooseTitle(job, tags),
                Subtitle = Cut(tags.Subtitle, MaxSubtitleLength),
                Comment = Cut(tags.Comment, MaxCommentLength),
                TrackNumber = ToNumber(tags.TrackNumber),
                DiscNumber = ToNumber(tags.DiscNumber),
                Year = ToNumber(tags.Year),
                Duration = tags.Duration,
                MediaFileID = file.ID
            };
            file.ArtistLinkList = [.. resolved.ArtistList.Select((x, index) => new EntityMediaFileArtist { MediaFileID = file.ID, ArtistID = x.ID, Position = index, Artist = x })];
            file.GenreLinkList = [.. resolved.GenreList.Select((x, index) => new EntityMediaFileGenre { MediaFileID = file.ID, GenreID = x.ID, Position = index, Genre = x })];

            uow.Media.AddMediaFile(file);
            foreach (EntityMediaFileArtist link in file.ArtistLinkList) uow.Metadata.AddMediaFileArtist(link);
            foreach (EntityMediaFileGenre link in file.GenreLinkList) uow.Metadata.AddMediaFileGenre(link);
            uow.Media.RemoveMediaUploadJob(job);

            // The items the file brings, new ones included, are told to the devices before the file.
            MetadataChangeSet itemChangeSet = new();
            await uow.AddItemCountList(itemChangeSet, job.UserID, file, MediaFileItemSet.Empty, cancellationToken);
            await uow.Commit(cancellationToken);

            ResponseUploadResult uploadResult = new()
            {
                JobID = job.ID,
                Title = job.Title,
                Succeeded = true,
                Media = file.ToResponse()
            };

            return (uploadResult, itemChangeSet);
        }, cancellationToken);

        await publisher.PublishItemList(job.UserID, changeSet, cancellationToken);

        // After the commit, so the file is in the library whatever happens to the picture.
        await OfferPicture(job.UserID, result, tags.Picture, cancellationToken);
        return result;
    }

    // A failed picture step never fails an upload: the album just has no picture.
    private async Task OfferPicture(Guid userID, ResponseUploadResult result, byte[]? picture, CancellationToken cancellationToken)
    {
        if (picture is null || result.Media?.AlbumID is not { } albumID) return;

        try
        {
            await serviceAlbumArt.Apply(userID, albumID, picture, cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Could not store the picture of album {AlbumID}", albumID);
        }
    }

    // The tags are best effort: a file whose tags cannot be read is still a library entry, titled from its file name.
    private MediaTagSet ReadTags(EntityMediaUploadJob job, CancellationToken cancellationToken)
    {
        string path = serviceFileStorage.CopyToTemporary(job.StagedFileName);
        try
        {
            return serviceMediaTagReading.Read(path);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Could not read the tags of upload job {JobID}; using the file name", job.ID);
            return MediaTagSet.Empty;
        }
        finally
        {
            serviceFileStorage.DeleteTemporary(path);
        }
    }

    // The title the user sent wins, then the embedded title, then the file name.
    private static string ChooseTitle(EntityMediaUploadJob job, MediaTagSet tags)
    {
        if (job.TitleWasSent) return job.Title;

        return Cut(tags.Title, MaxTitleLength) ?? job.Title;
    }

    // Trimmed and cut to the column length, or null when nothing is left.
    private static string? Cut(string? value, int maxLength)
    {
        string trimmed = value?.Trim() ?? string.Empty;

        return trimmed.Length == 0 ? null : trimmed.CutAt(maxLength);
    }

    // The tag formats use zero for "not set", and a value outside the range an edit accepts is not a real one.
    private static int? ToNumber(int? value)
    {
        return value is > 0 and <= MaxTagNumber ? value : null;
    }

    // Jobs are worked one after the other, so a file sent twice in one go finds the first one already in the library.
    private async Task EnsureNotInLibrary(EntityMediaUploadJob job, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        if (await uow.Media.FindMediaFileByHash(job.UserID, job.ContentHash, cancellationToken) is { } existing) throw new DuplicateMediaException(existing);
    }

    private async Task EnsureRoomInLibrary(EntityMediaUploadJob job, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        if (await uow.Media.CountMediaFiles(job.UserID, cancellationToken) >= configStorage.Value.MaxFilesPerUser) throw new LibraryFullException();
        if (configStorage.Value.MaxBytesPerUser == 0) return;

        long used = await uow.Media.SumMediaFileBytes(job.UserID, cancellationToken);
        if (used + job.SizeBytes > configStorage.Value.MaxBytesPerUser) throw new LibraryFullException();
    }

    private async Task<ResponseUploadResult?> Fail(EntityMediaUploadJob job, Exception exception, CancellationToken cancellationToken)
    {
        UploadFailureReasonEnum reason = exception switch
        {
            LibraryFullException => UploadFailureReasonEnum.LibraryFull,
            DuplicateMediaException => UploadFailureReasonEnum.Duplicate,
            FileNotFoundException => UploadFailureReasonEnum.FileMissing,
            _ => UploadFailureReasonEnum.Unknown
        };

        if (reason == UploadFailureReasonEnum.Unknown) logger.LogError(exception, "Upload job {JobID} failed", job.ID);
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
            Reason = reason,
            // A duplicate names the file the library already holds, so an upload to a playlist can still put it there.
            Media = exception is DuplicateMediaException duplicate ? duplicate.ExistingFile.ToResponse() : null
        };
    }

    // Each deletion stands alone, so one failing can't stop the other.
    private void DeleteQuietly(Action delete, EntityMediaUploadJob job)
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
