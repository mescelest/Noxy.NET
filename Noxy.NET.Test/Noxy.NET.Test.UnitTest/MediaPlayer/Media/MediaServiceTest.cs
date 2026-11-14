using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class MediaServiceTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeFileStorageService _storage = new();
    private readonly MediaService _service;
    private readonly FakeStore _store = new();

    public MediaServiceTest()
    {
        MediaFileValidator validator = new(Options.Create(new ConfigStorage { RootPath = "x", AllowedExtensions = ["mp3", "flac"] }));
        _service = new(new FakeUnitOfWorkFactory(_store), validator, _storage);
    }

    private static byte[] Mp3(int length = 100)
    {
        byte[] bytes = new byte[length];
        "ID3"u8.CopyTo(bytes);
        return bytes;
    }

    private Task<ResponseMediaFile> Upload(string fileName, byte[] content, string? title = null)
    {
        return _service.Upload(UserID, fileName, title, new MemoryStream(content), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Upload_StoresTheWholeFileIncludingTheHeader()
    {
        byte[] content = Mp3();

        await Upload("song.mp3", content);

        Assert.Equal(content, Assert.Single(_storage.Files).Value);
    }

    [Fact]
    public async Task Upload_SavesMediaFileAndMetadataForTheUser()
    {
        ResponseMediaFile response = await Upload("song.test.mp3", Mp3(100));

        MediaFile saved = Assert.Single(_store.MediaFiles);
        Assert.Equal(response.ID, saved.ID);
        Assert.Equal(UserID, saved.UserID);
        Assert.Equal("audio/mpeg", saved.ContentType);
        Assert.Equal(100, saved.SizeBytes);
        Assert.Equal(_storage.Files.Keys.Single(), saved.FilePath);
        Assert.EndsWith(".mp3", saved.FilePath);
        Assert.Equal(saved.ID, saved.Metadata?.MediaFileID);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task Upload_ReturnsTheSavedFile()
    {
        ResponseMediaFile response = await Upload("song.mp3", Mp3(100), "My song");

        Assert.Equal("My song", response.Title);
        Assert.Equal("audio/mpeg", response.ContentType);
        Assert.Equal(100, response.SizeBytes);
    }

    [Theory]
    [InlineData("song.test.mp3", null, "song.test")]
    [InlineData("song.mp3", "", "song")]
    [InlineData("song.mp3", "   ", "song")]
    [InlineData("song.mp3", "  Given  ", "Given")]
    [InlineData(".mp3", null, "Untitled")]
    public async Task Upload_ResolvesTheTitle(string fileName, string? title, string expected)
    {
        ResponseMediaFile response = await Upload(fileName, Mp3(), title);

        Assert.Equal(expected, response.Title);
    }

    [Fact]
    public async Task Upload_TooLongTitle_IsCut()
    {
        ResponseMediaFile response = await Upload("song.mp3", Mp3(), new string('a', 400));

        Assert.Equal(255, response.Title.Length);
    }

    [Fact]
    public async Task Upload_FileShorterThanTheHeader_IsStoredWhole()
    {
        byte[] content = "ID3"u8.ToArray();

        ResponseMediaFile response = await Upload("tiny.mp3", content);

        Assert.Equal(content, Assert.Single(_storage.Files).Value);
        Assert.Equal(3, response.SizeBytes);
    }

    [Theory]
    [InlineData("song.exe")]
    [InlineData("song.flac")]
    public async Task Upload_HeaderOrExtensionWrong_ThrowsAndStoresNothing(string fileName)
    {
        await Assert.ThrowsAsync<UnsupportedMediaTypeException>(() => Upload(fileName, Mp3()));

        Assert.Empty(_storage.Files);
        Assert.Empty(_store.MediaFiles);
        Assert.Equal(0, _store.CommitCount);
    }

    [Fact]
    public async Task Upload_CommitFails_DeletesTheStoredFile()
    {
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Upload("song.mp3", Mp3()));

        Assert.Empty(_storage.Files);
        Assert.Single(_storage.Deleted);
        Assert.Empty(_store.MediaFiles);
    }
}
