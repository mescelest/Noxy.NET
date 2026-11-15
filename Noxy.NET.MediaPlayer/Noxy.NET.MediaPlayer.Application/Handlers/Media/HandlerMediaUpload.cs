using Mediator;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Commands.Media;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Models.Upload;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Upload;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Media;

public class HandlerMediaUpload(
    IUnitOfWorkFactory factoryUnitOfWork,
    IMediaFileValidationService serviceMediaFileValidation,
    IFileStorageService serviceFileStorage,
    IUploadSignal signalUpload,
    IOptions<ConfigStorage> configStorage,
    IOptions<ConfigUpload> configUpload,
    IUploadGate gateUpload) : ICommandHandler<CommandMediaUpload, ResponseUploadAccepted>
{
    // Stages the file and queues a job for it. The file name is only used to pick the extension and the default title.
    public async ValueTask<ResponseUploadAccepted> Handle(CommandMediaUpload command, CancellationToken cancellationToken)
    {
        // One upload per user at a time, held until its job is saved, so the checks below see every earlier upload.
        using IDisposable lease = gateUpload.TryEnter(command.UserID) ?? throw new UploadInProgressException();

        // The body can't be rewound, so the header is read for the check and then replayed in front of the rest.
        byte[] header = new byte[IMediaFileValidationService.HeaderSize];
        int headerLength = await command.Content.ReadAtLeastAsync(header, header.Length, false, cancellationToken);
        ValidatedMediaFile file = serviceMediaFileValidation.Validate(command.FileName, header.AsSpan(0, headerLength));

        long? remainingBytes = await EnsureRoomToEnqueue(command.UserID, cancellationToken);

        StoredFile staged = await Stage(new PrefixedStream(header.AsMemory(0, headerLength), command.Content), file.Extension, remainingBytes, cancellationToken);

        EntityMediaUploadJob job = new()
        {
            UserID = command.UserID,
            StagedFileName = staged.FileName,
            Title = command.ResolveTitle(),
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
