using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Helpers;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Gates;
using Noxy.NET.MediaPlayer.Application.Interfaces.Notifiers;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Interfaces.Signals;
using Noxy.NET.MediaPlayer.Application.Interfaces.Validators;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Application.Streams;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Requests;
using Noxy.NET.MediaPlayer.Domain.Requests.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class MediaService(
    IUnitOfWorkFactory factoryUnitOfWork,
    IMediaFileValidator validatorMediaFile,
    IFileStorageService serviceFileStorage,
    IMediaTagWriter writerMediaTag,
    IUploadSignal signalUpload,
    IOptions<ConfigStorage> configStorage,
    IOptions<ConfigUpload> configUpload,
    IUploadGate gateUpload,
    IMediaNotifier notifierMedia,
    IPlaylistNotifier notifierPlaylist,
    IETagService serviceETag,
    TimeProvider time,
    ILogger<MediaService> logger) : IMediaService
{
    public async Task<ResponseUploadAccepted> Enqueue(Guid userID, string fileName, string? title, Stream content, CancellationToken cancellationToken = default)
    {
        // One upload per user at a time, held until its job is saved, so the checks below see every earlier upload.
        using IDisposable lease = gateUpload.TryEnter(userID) ?? throw new UploadInProgressException();

        // The body can't be rewound, so the header is read for the check and then replayed in front of the rest.
        byte[] header = new byte[IMediaFileValidator.HeaderSize];
        int headerLength = await content.ReadAtLeastAsync(header, header.Length, false, cancellationToken);
        ValidatedMediaFile file = validatorMediaFile.Validate(fileName, header.AsSpan(0, headerLength));

        long? remainingBytes = await EnsureRoomToEnqueue(userID, cancellationToken);

        StoredFile staged = await Stage(new PrefixedStream(header.AsMemory(0, headerLength), content), file.Extension, remainingBytes, cancellationToken);

        MediaUploadJob job = new()
        {
            UserID = userID,
            StagedFileName = staged.FileName,
            Title = MediaTitle.Resolve(title, fileName),
            ContentType = file.ContentType,
            SizeBytes = staged.SizeBytes,
            ContentHash = staged.ContentHash
        };

        try
        {
            await using IUnitOfWork uow = factoryUnitOfWork.Create();
            uow.Media.AddMediaUploadJob(job);
            await uow.Commit(cancellationToken);
        }
        catch
        {
            // Don't leave a file in staging that no job points to.
            serviceFileStorage.DeleteStaged(staged.FileName);
            throw;
        }

        signalUpload.Notify();
        return new() { JobID = job.ID };
    }

    public async Task<List<ResponseMediaFile>> GetMediaFileList(Guid userID, int skip, int take, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        List<MediaFile> fileList = await uow.Media.GetMediaFileList(userID, skip, take, cancellationToken);
        return [.. fileList.Select(x => x.ToResponse())];
    }

    public async Task<MediaDownload> Open(Guid userID, Guid id, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        MediaFile mediaFile = await uow.Media.FindMediaFile(id, userID, cancellationToken) ?? throw new MediaNotFoundException();

        try
        {
            return new(serviceFileStorage.OpenRead(mediaFile.FilePath), mediaFile.ContentType, FileNameOf(mediaFile), mediaFile.ContentHash);
        }
        catch (FileNotFoundException)
        {
            throw new MediaNotFoundException();
        }
    }

    public async Task<MediaDownload> OpenWithTags(Guid userID, Guid id, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        MediaFile mediaFile = await uow.Media.FindMediaFile(id, userID, cancellationToken) ?? throw new MediaNotFoundException();

        string path = CopyToTemporary(mediaFile);
        try
        {
            if (mediaFile.Metadata is null || TryWriteTags(path, mediaFile.Metadata))
            {
                return new(serviceFileStorage.OpenTemporary(path), mediaFile.ContentType, FileNameOf(mediaFile), mediaFile.ContentHash);
            }

            // A failed write can leave the copy half written, so serve a fresh one.
            serviceFileStorage.DeleteTemporary(path);
            path = CopyToTemporary(mediaFile);

            return new(serviceFileStorage.OpenTemporary(path), mediaFile.ContentType, FileNameOf(mediaFile), mediaFile.ContentHash);
        }
        catch
        {
            serviceFileStorage.DeleteTemporary(path);
            throw;
        }
    }

    public async Task<ResponseMediaFile> Update(Guid userID, Guid id, RequestUpdateMedia request, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        MediaFile file = await uow.Media.FindMediaFile(id, userID, cancellationToken) ?? throw new MediaNotFoundException();
        MediaFileMetadata metadata = file.Metadata ?? throw new MediaNotFoundException();

        bool changed = false;
        if (request.Title.HasValue)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Title.Value, nameof(request));

            changed |= Apply(request.Title.Value.Trim(), metadata.Title, value => metadata.Title = value);
        }

        changed |= ApplyText(request.Artist, metadata.Artist, value => metadata.Artist = value);
        changed |= ApplyText(request.Album, metadata.Album, value => metadata.Album = value);
        changed |= ApplyText(request.AlbumArtist, metadata.AlbumArtist, value => metadata.AlbumArtist = value);
        changed |= ApplyText(request.Subtitle, metadata.Subtitle, value => metadata.Subtitle = value);
        changed |= ApplyText(request.Comment, metadata.Comment, value => metadata.Comment = value);
        changed |= ApplyText(request.Genre, metadata.Genre, value => metadata.Genre = value);
        changed |= ApplyNumber(request.TrackNumber, metadata.TrackNumber, value => metadata.TrackNumber = value);
        changed |= ApplyNumber(request.DiscNumber, metadata.DiscNumber, value => metadata.DiscNumber = value);
        changed |= ApplyNumber(request.Year, metadata.Year, value => metadata.Year = value);

        // An edit that changes nothing leaves the modified time alone, so it can't change the library ETag or push anything.
        if (!changed) return file.ToResponse();

        file.TimeModified = time.GetUtcNow().UtcDateTime;
        uow.Media.UpdateMediaFile(file);
        await uow.Commit(cancellationToken);

        ResponseMediaFile response = file.ToResponse();
        await NotifyQuietly(() => notifierMedia.Updated(userID, response, cancellationToken), id);
        return response;
    }

    public async Task Delete(Guid userID, Guid id, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        MediaFile file = await uow.Media.FindMediaFile(id, userID, cancellationToken) ?? throw new MediaNotFoundException();

        // The media file leaves every playlist in the same commit, so no entry is ever left pointing at it.
        DateTime now = time.GetUtcNow().UtcDateTime;
        List<ResponsePlaylist> playlistUpdatedList = [];
        foreach (Playlist playlist in await uow.Playlist.GetPlaylistListByMediaFileID(userID, id, cancellationToken))
        {
            foreach (PlaylistEntry removed in playlist.EntryList.Where(x => x.MediaFileID == id))
            {
                uow.Playlist.RemovePlaylistEntry(removed);
            }

            playlist.EntryList.RemoveAll(x => x.MediaFileID == id);
            foreach (PlaylistEntry changed in playlist.RenumberEntryList())
            {
                uow.Playlist.UpdatePlaylistEntry(changed);
            }

            playlist.TimeModified = now;
            uow.Playlist.UpdatePlaylist(playlist);
            playlistUpdatedList.Add(playlist.ToResponse(playlist.EntryList.Count));
        }

        // The row goes first: a failure after it leaves an orphaned file, never a media file that points at nothing.
        uow.Media.RemoveMediaFile(file);
        await uow.Commit(cancellationToken);

        try
        {
            serviceFileStorage.Delete(file.FilePath);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not delete the file of deleted media file {MediaFileID}", id);
        }

        await NotifyQuietly(() => notifierMedia.Deleted(userID, id, cancellationToken), id);

        foreach (ResponsePlaylist updated in playlistUpdatedList)
        {
            await NotifyQuietly(() => notifierPlaylist.Updated(userID, updated, cancellationToken), updated.ID);
        }
    }

    public async Task<MediaLibrary> GetMediaLibrary(Guid userID, string? ifNoneMatch, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        if (!string.IsNullOrWhiteSpace(ifNoneMatch))
        {
            List<MediaFileStamp> stampList = await uow.Media.GetMediaFileStampList(userID, cancellationToken);
            string current = serviceETag.Compute(stampList.Select(x => (x.ID, x.TimeModified)));
            if (serviceETag.Matches(ifNoneMatch, current)) return new(current, null);
        }

        // The ETag comes from the list that is returned, so the two can never disagree.
        List<MediaFile> fileList = await uow.Media.GetMediaFileList(userID, cancellationToken);
        return new(serviceETag.Compute(fileList.Select(x => (x.ID, x.TimeModified))), [.. fileList.Select(x => x.ToResponse())]);
    }

    private static bool Apply(string value, string? current, Action<string> set)
    {
        if (value == current) return false;

        set(value);
        return true;
    }

    // A blank value clears the tag, so a tag never holds an empty string.
    private static bool ApplyText(Optional<string?> requested, string? current, Action<string?> set)
    {
        if (!requested.HasValue) return false;

        string? value = string.IsNullOrWhiteSpace(requested.Value) ? null : requested.Value.Trim();
        if (value == current) return false;

        set(value);
        return true;
    }

    private static bool ApplyNumber(Optional<int?> requested, int? current, Action<int?> set)
    {
        if (!requested.HasValue || requested.Value == current) return false;

        set(requested.Value);
        return true;
    }

    // A failed notification is only logged: the change itself has already happened.
    // The download is named after the title, with the extension of the stored file.
    private static string FileNameOf(MediaFile mediaFile)
    {
        return mediaFile.Metadata is null ? mediaFile.FilePath : $"{mediaFile.Metadata.Title}{Path.GetExtension(mediaFile.FilePath)}";
    }

    private string CopyToTemporary(MediaFile mediaFile)
    {
        try
        {
            return serviceFileStorage.CopyToTemporary(mediaFile.FilePath);
        }
        catch (FileNotFoundException)
        {
            throw new MediaNotFoundException();
        }
    }

    // A file the tagger cannot read is still a media file the user is entitled to, so the failure is logged and the untagged copy is served.
    private bool TryWriteTags(string path, MediaFileMetadata metadata)
    {
        try
        {
            writerMediaTag.Write(path, metadata);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not write the tags of media file {MediaFileID}; serving it untagged", metadata.MediaFileID);
            return false;
        }
    }

    private async Task NotifyQuietly(Func<Task> notify, Guid id)
    {
        try
        {
            await notify();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not notify about {EntityID}", id);
        }
    }

    // Stages the file, stopping it as soon as it would pass what the user has left, so it never takes more disk than that.
    private async Task<StoredFile> Stage(Stream content, string extension, long? remainingBytes, CancellationToken cancellationToken)
    {
        if (remainingBytes is not { } remaining) return await serviceFileStorage.StageAsync(content, extension, cancellationToken);

        try
        {
            return await serviceFileStorage.StageAsync(content, extension, remaining, cancellationToken);
        }
        catch (FileTooLargeException) when (remaining < configStorage.Value.MaxSizeBytes)
        {
            // The quota was the tighter limit, so this is a full library and not an oversized file.
            throw new LibraryFullException();
        }
    }

    // Advisory: it stops a full library or a flood from staging large files in vain. The worker makes the authoritative check.
    // Returns the bytes the user has left, or null when there is no byte limit.
    private async Task<long?> EnsureRoomToEnqueue(Guid userID, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        int pending = await uow.Media.CountMediaUploadJobs(userID, cancellationToken);
        int files = await uow.Media.CountMediaFiles(userID, cancellationToken);

        if (files + pending >= configStorage.Value.MaxFilesPerUser) throw new LibraryFullException();
        if (pending >= configUpload.Value.MaxPendingPerUser) throw new UploadQueueFullException();
        if (configStorage.Value.MaxBytesPerUser == 0) return null;

        long used = await uow.Media.SumMediaFileBytes(userID, cancellationToken) + await uow.Media.SumMediaUploadJobBytes(userID, cancellationToken);
        long remaining = configStorage.Value.MaxBytesPerUser - used;

        return remaining <= 0 ? throw new LibraryFullException() : remaining;
    }
}
