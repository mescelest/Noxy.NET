using System.Security.Cryptography;
using Noxy.NET.MediaPlayer.Application.Handlers.Media;
using Noxy.NET.MediaPlayer.Application.Models.Media;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class HandlerMediaFileTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeFileStorageService _storage = new();
    private readonly FakeStore _store = new();
    private readonly HandlerMediaFile _handler;

    public HandlerMediaFileTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), _storage);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private static byte[] Mp3(int length = 100)
    {
        byte[] bytes = new byte[length];
        "ID3"u8.CopyTo(bytes);
        return bytes;
    }

    // A file that is already in the library, as the worker would have left it.
    private EntityMediaFile Seed(Guid userID, string title, int length = 100)
    {
        byte[] bytes = Mp3(length);
        EntityMediaFile file = new()
        {
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = length,
            ContentHash = Convert.ToHexStringLower(SHA256.HashData(bytes)),
            UserID = userID
        };
        file.Metadata = new() { Title = title, MediaFileID = file.ID };

        _storage.Files[file.FilePath] = bytes;
        _store.MediaFileList.Add(file);
        return file;
    }

    [Fact]
    public async Task Handle_ReturnsTheContentNamedAfterTheTitle()
    {
        EntityMediaFile file = Seed(UserID, "My Media", 50);

        MediaDownload download = await _handler.Handle(new(UserID, file.ID), Token);
        await using Stream stream = download.Content;
        using MemoryStream copy = new();
        await stream.CopyToAsync(copy, Token);

        Assert.Equal(Mp3(50), copy.ToArray());
        Assert.Equal("audio/mpeg", download.ContentType);
        Assert.Equal("My Media.mp3", download.FileName);
    }

    [Fact]
    public async Task Handle_ReturnsTheStoredContentHash()
    {
        EntityMediaFile file = Seed(UserID, "My Media", 50);

        MediaDownload download = await _handler.Handle(new(UserID, file.ID), Token);
        await download.Content.DisposeAsync();

        Assert.Equal(file.ContentHash, download.ContentHash);
    }

    [Fact]
    public async Task Handle_UnknownId_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<MediaNotFoundException>(() => _handler.Handle(new(UserID, Guid.CreateVersion7()), Token).AsTask());
    }

    [Fact]
    public async Task Handle_AnotherUsersFile_ThrowsNotFound()
    {
        EntityMediaFile file = Seed(UserID, "media");

        await Assert.ThrowsAsync<MediaNotFoundException>(() => _handler.Handle(new(Guid.CreateVersion7(), file.ID), Token).AsTask());
    }

    [Fact]
    public async Task Handle_FileMissingFromStorage_ThrowsNotFound()
    {
        EntityMediaFile file = Seed(UserID, "media");
        _storage.Files.Clear();

        await Assert.ThrowsAsync<MediaNotFoundException>(() => _handler.Handle(new(UserID, file.ID), Token).AsTask());
    }
}
