using System.Security.Cryptography;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeFileStorageService : IFileStorageService
{
    public Dictionary<string, byte[]> Files { get; } = [];
    public Dictionary<string, byte[]> Staged { get; } = [];
    public List<string> DeletedList { get; } = [];
    public List<string> DeletedStagedList { get; } = [];

    public Exception? StageFailure { get; set; }
    public Exception? PromoteFailure { get; set; }
    public Exception? DeleteStagedFailure { get; set; }
    public Exception? DeleteFailure { get; set; }

    public async Task<StoredFile> StageAsync(Stream content, string extension, CancellationToken cancellationToken = default)
    {
        if (StageFailure is not null) throw StageFailure;

        (string fileName, byte[] bytes) = await Read(content, extension, cancellationToken);
        Staged[fileName] = bytes;

        return Describe(fileName, bytes);
    }

    public async Task<StoredFile> StageAsync(Stream content, string extension, long maxBytes, CancellationToken cancellationToken = default)
    {
        if (StageFailure is not null) throw StageFailure;

        (string fileName, byte[] bytes) = await Read(content, extension, cancellationToken);
        if (bytes.Length > maxBytes) throw new FileTooLargeException(maxBytes);

        Staged[fileName] = bytes;

        return Describe(fileName, bytes);
    }

    public void Promote(string fileName)
    {
        if (PromoteFailure is not null) throw PromoteFailure;

        if (!Staged.Remove(fileName, out byte[]? bytes))
        {
            if (Files.ContainsKey(fileName)) return;

            throw new FileNotFoundException(fileName);
        }

        Files[fileName] = bytes;
    }

    public void DeleteStaged(string fileName)
    {
        if (DeleteStagedFailure is not null) throw DeleteStagedFailure;

        Staged.Remove(fileName);
        DeletedStagedList.Add(fileName);
    }

    public IReadOnlyList<string> GetStagedFileNameList()
    {
        return [.. Staged.Keys];
    }

    public Stream OpenRead(string fileName)
    {
        if (!Files.TryGetValue(fileName, out byte[]? bytes)) throw new FileNotFoundException(fileName);

        return new MemoryStream(bytes);
    }

    public void Delete(string fileName)
    {
        if (DeleteFailure is not null) throw DeleteFailure;

        Files.Remove(fileName);
        DeletedList.Add(fileName);
    }

    private static async Task<(string FileName, byte[] Bytes)> Read(Stream content, string extension, CancellationToken cancellationToken)
    {
        using MemoryStream copy = new();
        await content.CopyToAsync(copy, cancellationToken);

        return ($"{Guid.CreateVersion7()}.{extension}", copy.ToArray());
    }

    private static StoredFile Describe(string fileName, byte[] bytes)
    {
        return new(fileName, bytes.Length, Convert.ToHexStringLower(SHA256.HashData(bytes)));
    }
}
