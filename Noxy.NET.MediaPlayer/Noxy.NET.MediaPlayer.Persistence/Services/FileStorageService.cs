using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.MediaPlayer.Persistence.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _root;
    private readonly long _maxSizeBytes;

    public FileStorageService(IOptions<ConfigStorage> options)
    {
        _root = Path.GetFullPath(options.Value.RootPath);
        _maxSizeBytes = options.Value.MaxSizeBytes;

        Directory.CreateDirectory(_root);
    }

    public async Task<StoredFile> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default)
    {
        string normalized = extension.TrimStart('.').ToLowerInvariant();
        if (normalized.Length == 0 || !normalized.All(char.IsAsciiLetterOrDigit))
        {
            throw new ArgumentException("The extension must be letters and digits only.", nameof(extension));
        }

        string fileName = $"{Guid.CreateVersion7()}.{normalized}";
        string path = Path.Combine(_root, fileName);
        long size;

        try
        {
            await using FileStream file = new(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
            size = await CopyWithLimitAsync(content, file, cancellationToken);
        }
        catch
        {
            File.Delete(path);
            throw;
        }

        return new(fileName, size);
    }

    public Stream OpenRead(string fileName)
    {
        return new FileStream(ResolvePath(fileName), FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
    }

    public void Delete(string fileName)
    {
        File.Delete(ResolvePath(fileName));
    }

    private async Task<long> CopyWithLimitAsync(Stream source, Stream destination, CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[81920];
        long total = 0;
        int read;

        while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
        {
            total += read;
            if (total > _maxSizeBytes)
            {
                throw new FileTooLargeException(_maxSizeBytes);
            }

            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        return total;
    }

    private string ResolvePath(string fileName)
    {
        if (fileName != Path.GetFileName(fileName) || fileName.Length == 0)
        {
            throw new ArgumentException("The file name must not contain a path.", nameof(fileName));
        }

        return Path.Combine(_root, fileName);
    }
}
