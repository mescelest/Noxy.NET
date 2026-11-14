using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Application.Streams;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class MediaService(IUnitOfWorkFactory uowFactory, IMediaFileValidator validator, IFileStorageService storage) : IMediaService
{
    private const int MaxTitleLength = 255;

    public async Task<ResponseMediaFile> Upload(Guid userID, string fileName, string? title, Stream content, CancellationToken cancellationToken = default)
    {
        // The body can't be rewound, so the header is read for the check and then replayed in front of the rest.
        byte[] header = new byte[IMediaFileValidator.HeaderSize];
        int headerLength = await content.ReadAtLeastAsync(header, header.Length, false, cancellationToken);
        ValidatedMediaFile file = validator.Validate(fileName, header.AsSpan(0, headerLength));

        StoredFile stored = await storage.SaveAsync(new PrefixedStream(header.AsMemory(0, headerLength), content), file.Extension, cancellationToken);

        MediaFile mediaFile = new()
        {
            FilePath = stored.FileName,
            ContentType = file.ContentType,
            SizeBytes = stored.SizeBytes,
            UserID = userID
        };
        mediaFile.Metadata = new()
        {
            Title = ResolveTitle(title, fileName),
            MediaFileID = mediaFile.ID
        };

        try
        {
            await using IUnitOfWork uow = uowFactory.Create();
            uow.Media.AddMediaFile(mediaFile);
            await uow.Commit(cancellationToken);
        }
        catch
        {
            // Don't leave a file on disk that nothing points to.
            storage.Delete(stored.FileName);
            throw;
        }

        return new()
        {
            ID = mediaFile.ID,
            Title = mediaFile.Metadata.Title,
            ContentType = mediaFile.ContentType,
            SizeBytes = mediaFile.SizeBytes,
            TimeCreated = mediaFile.TimeCreated
        };
    }

    private static string ResolveTitle(string? title, string fileName)
    {
        string resolved = (string.IsNullOrWhiteSpace(title) ? Path.GetFileNameWithoutExtension(fileName) : title).Trim();
        if (resolved.Length == 0) resolved = "Untitled";

        return resolved.Length > MaxTitleLength ? resolved[..MaxTitleLength] : resolved;
    }
}
