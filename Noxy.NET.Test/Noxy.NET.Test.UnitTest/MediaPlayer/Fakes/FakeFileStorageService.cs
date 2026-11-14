using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeFileStorageService : IFileStorageService
{
    public Dictionary<string, byte[]> Files { get; } = [];
    public List<string> Deleted { get; } = [];

    public async Task<StoredFile> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default)
    {
        using MemoryStream copy = new();
        await content.CopyToAsync(copy, cancellationToken);

        string fileName = $"{Guid.CreateVersion7()}.{extension}";
        Files[fileName] = copy.ToArray();

        return new(fileName, copy.Length);
    }

    public Stream OpenRead(string fileName)
    {
        return new MemoryStream(Files[fileName]);
    }

    public void Delete(string fileName)
    {
        Files.Remove(fileName);
        Deleted.Add(fileName);
    }
}
