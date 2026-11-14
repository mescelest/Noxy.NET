using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Persistence.Services;

namespace Noxy.NET.Test.MediaPlayer.Storage;

public class FileStorageServiceTest : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }
    }

    private FileStorageService Create(long maxSizeBytes = 1024)
    {
        return new(Options.Create(new ConfigStorage { RootPath = _root, MaxSizeBytes = maxSizeBytes, AllowedExtensions = ["mp3"] }));
    }

    private static MemoryStream Bytes(int count)
    {
        return new(new byte[count]);
    }

    [Fact]
    public void Constructor_CreatesMissingRootDirectory()
    {
        Create();

        Assert.True(Directory.Exists(_root));
    }

    [Fact]
    public async Task Save_StoresContentUnderGeneratedNameWithExtension()
    {
        FileStorageService service = Create();

        string fileName = await service.SaveAsync(new MemoryStream([1, 2, 3]), ".MP3", TestContext.Current.CancellationToken);

        Assert.EndsWith(".mp3", fileName);
        Assert.True(Guid.TryParse(Path.GetFileNameWithoutExtension(fileName), out _));
        Assert.Equal([1, 2, 3], await File.ReadAllBytesAsync(Path.Combine(_root, fileName), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Save_TwiceGivesDifferentNames()
    {
        FileStorageService service = Create();

        string first = await service.SaveAsync(Bytes(1), "mp3", TestContext.Current.CancellationToken);
        string second = await service.SaveAsync(Bytes(1), "mp3", TestContext.Current.CancellationToken);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public async Task Save_ExactlyMaxSize_Succeeds()
    {
        FileStorageService service = Create(100);

        string fileName = await service.SaveAsync(Bytes(100), "mp3", TestContext.Current.CancellationToken);

        Assert.True(File.Exists(Path.Combine(_root, fileName)));
    }

    [Fact]
    public async Task Save_OverMaxSize_ThrowsAndLeavesNoFile()
    {
        FileStorageService service = Create(100);

        await Assert.ThrowsAsync<FileTooLargeException>(() => service.SaveAsync(Bytes(101), "mp3", TestContext.Current.CancellationToken));

        Assert.Empty(Directory.GetFiles(_root));
    }

    [Theory]
    [InlineData("")]
    [InlineData(".")]
    [InlineData("mp3/../x")]
    [InlineData("m p3")]
    public async Task Save_InvalidExtension_Throws(string extension)
    {
        FileStorageService service = Create();

        await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAsync(Bytes(1), extension, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task OpenRead_ReturnsSavedContent()
    {
        FileStorageService service = Create();
        string fileName = await service.SaveAsync(new MemoryStream([9, 8, 7]), "mp3", TestContext.Current.CancellationToken);

        await using Stream stream = service.OpenRead(fileName);
        using MemoryStream copy = new();
        await stream.CopyToAsync(copy, TestContext.Current.CancellationToken);

        Assert.Equal([9, 8, 7], copy.ToArray());
    }

    [Fact]
    public async Task Delete_RemovesFile()
    {
        FileStorageService service = Create();
        string fileName = await service.SaveAsync(Bytes(1), "mp3", TestContext.Current.CancellationToken);

        service.Delete(fileName);

        Assert.False(File.Exists(Path.Combine(_root, fileName)));
    }

    [Fact]
    public void Delete_MissingFile_DoesNotThrow()
    {
        Create().Delete("missing.mp3");
    }

    [Theory]
    [InlineData("../secret.mp3")]
    [InlineData("sub/file.mp3")]
    [InlineData("")]
    public void OpenReadAndDelete_PathInName_Throws(string fileName)
    {
        FileStorageService service = Create();

        Assert.Throws<ArgumentException>(() => service.OpenRead(fileName));
        Assert.Throws<ArgumentException>(() => service.Delete(fileName));
    }
}
