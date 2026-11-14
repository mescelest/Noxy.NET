using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Application.Streams;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class MediaService(IUnitOfWorkFactory uowFactory, IMediaFileValidator validator, IFileStorageService storage, IUploadSignal signal, IOptions<ConfigStorage> storageOptions, IOptions<ConfigUpload> uploadOptions, IUploadGate gate) : IMediaService
{
    public async Task<ResponseUploadAccepted> Enqueue(Guid userID, string fileName, string? title, Stream content, CancellationToken cancellationToken = default)
    {
        // One upload per user at a time, held until its job is saved, so the checks below see every earlier upload.
        using IDisposable lease = gate.TryEnter(userID) ?? throw new UploadInProgressException();

        // The body can't be rewound, so the header is read for the check and then replayed in front of the rest.
        byte[] header = new byte[IMediaFileValidator.HeaderSize];
        int headerLength = await content.ReadAtLeastAsync(header, header.Length, false, cancellationToken);
        ValidatedMediaFile file = validator.Validate(fileName, header.AsSpan(0, headerLength));

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
            await using IUnitOfWork uow = uowFactory.Create();
            uow.Media.AddMediaUploadJob(job);
            await uow.Commit(cancellationToken);
        }
        catch
        {
            // Don't leave a file in staging that no job points to.
            storage.DeleteStaged(staged.FileName);
            throw;
        }

        signal.Notify();
        return new() { JobID = job.ID };
    }

    public async Task<List<ResponseMediaFile>> GetMediaFileList(Guid userID, int skip, int take, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = uowFactory.Create();

        List<MediaFile> fileList = await uow.Media.GetMediaFileList(userID, skip, take, cancellationToken);
        return [.. fileList.Select(MediaFileMapper.ToResponse)];
    }

    public async Task<MediaDownload> Open(Guid userID, Guid id, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = uowFactory.Create();

        MediaFile mediaFile = await uow.Media.FindMediaFile(id, userID, cancellationToken) ?? throw new MediaNotFoundException();

        // The download is named after the title, with the extension of the stored file.
        string fileName = mediaFile.Metadata == null ? mediaFile.FilePath : $"{mediaFile.Metadata.Title}{Path.GetExtension(mediaFile.FilePath)}";

        try
        {
            return new(storage.OpenRead(mediaFile.FilePath), mediaFile.ContentType, fileName);
        }
        catch (FileNotFoundException)
        {
            throw new MediaNotFoundException();
        }
    }

    // Stages the file, stopping it as soon as it would pass what the user has left, so it never takes more disk than that.
    private async Task<StoredFile> Stage(Stream content, string extension, long? remainingBytes, CancellationToken cancellationToken)
    {
        if (remainingBytes is not { } remaining) return await storage.StageAsync(content, extension, cancellationToken);

        try
        {
            return await storage.StageAsync(content, extension, remaining, cancellationToken);
        }
        catch (FileTooLargeException) when (remaining < storageOptions.Value.MaxSizeBytes)
        {
            // The quota was the tighter limit, so this is a full library and not an oversized file.
            throw new LibraryFullException();
        }
    }

    // Advisory: it stops a full library or a flood from staging large files in vain. The worker makes the authoritative check.
    // Returns the bytes the user has left, or null when there is no byte limit.
    private async Task<long?> EnsureRoomToEnqueue(Guid userID, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = uowFactory.Create();

        int pending = await uow.Media.CountMediaUploadJobs(userID, cancellationToken);
        int files = await uow.Media.CountMediaFiles(userID, cancellationToken);

        if (files + pending >= storageOptions.Value.MaxFilesPerUser) throw new LibraryFullException();
        if (pending >= uploadOptions.Value.MaxPendingPerUser) throw new UploadQueueFullException();
        if (storageOptions.Value.MaxBytesPerUser == 0) return null;

        long used = await uow.Media.SumMediaFileBytes(userID, cancellationToken) + await uow.Media.SumMediaUploadJobBytes(userID, cancellationToken);
        long remaining = storageOptions.Value.MaxBytesPerUser - used;
        if (remaining <= 0) throw new LibraryFullException();

        return remaining;
    }
}
