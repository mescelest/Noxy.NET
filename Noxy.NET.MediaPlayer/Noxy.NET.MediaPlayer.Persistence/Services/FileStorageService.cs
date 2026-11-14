using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.MediaPlayer.Persistence.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _root;
    private readonly string _staging;
    private readonly long _maxSizeBytes;

    public FileStorageService(IOptions<ConfigStorage> options)
    {
        _root = Path.GetFullPath(options.Value.RootPath);
        _staging = Path.Combine(_root, "staging");
        _maxSizeBytes = options.Value.MaxSizeBytes;

        Directory.CreateDirectory(_root);
        Directory.CreateDirectory(_staging);
    }

    public Task<StoredFile> StageAsync(Stream content, string extension, CancellationToken cancellationToken = default)
    {
        return WriteAsync(_staging, content, extension, _maxSizeBytes, cancellationToken);
    }

    public Task<StoredFile> StageAsync(Stream content, string extension, long maxBytes, CancellationToken cancellationToken = default)
    {
        return WriteAsync(_staging, content, extension, Math.Min(_maxSizeBytes, maxBytes), cancellationToken);
    }

    public void Promote(string fileName)
    {
        string staged = ResolvePath(_staging, fileName);
        string target = ResolvePath(_root, fileName);

        if (!File.Exists(staged))
        {
            // Already moved by an earlier attempt that was interrupted before it finished.
            if (File.Exists(target)) return;

            throw new FileNotFoundException("The staged file was not found.", fileName);
        }

        File.Move(staged, target);
    }

    public void DeleteStaged(string fileName)
    {
        File.Delete(ResolvePath(_staging, fileName));
    }

    public IReadOnlyList<string> GetStagedFileNameList()
    {
        return [.. Directory.EnumerateFiles(_staging).Select(x => Path.GetFileName(x))];
    }

    public Stream OpenRead(string fileName)
    {
        return new FileStream(ResolvePath(_root, fileName), FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
    }

    public void Delete(string fileName)
    {
        File.Delete(ResolvePath(_root, fileName));
    }

    private async Task<StoredFile> WriteAsync(string directory, Stream content, string extension, long maxBytes, CancellationToken cancellationToken)
    {
        string normalized = extension.TrimStart('.').ToLowerInvariant();
        if (normalized.Length == 0 || !normalized.All(char.IsAsciiLetterOrDigit))
        {
            throw new ArgumentException("The extension must be letters and digits only.", nameof(extension));
        }

        string fileName = $"{Guid.CreateVersion7()}.{normalized}";
        string path = Path.Combine(directory, fileName);
        StoredFile stored;

        try
        {
            await using FileStream file = new(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
            (long size, string hash) = await CopyWithLimitAsync(content, file, maxBytes, cancellationToken);
            stored = new(fileName, size, hash);
        }
        catch
        {
            File.Delete(path);
            throw;
        }

        return stored;
    }

    // Hashes while copying, so the file is only read once.
    private async Task<(long Size, string Hash)> CopyWithLimitAsync(Stream source, Stream destination, long maxBytes, CancellationToken cancellationToken)
    {
        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        byte[] buffer = new byte[81920];
        long total = 0;
        int read;

        while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
        {
            total += read;
            if (total > maxBytes)
            {
                throw new FileTooLargeException(maxBytes);
            }

            hash.AppendData(buffer, 0, read);
            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        return (total, Convert.ToHexStringLower(hash.GetHashAndReset()));
    }

    private static string ResolvePath(string directory, string fileName)
    {
        if (fileName != Path.GetFileName(fileName) || fileName.Length == 0)
        {
            throw new ArgumentException("The file name must not contain a path.", nameof(fileName));
        }

        return Path.Combine(directory, fileName);
    }
}
