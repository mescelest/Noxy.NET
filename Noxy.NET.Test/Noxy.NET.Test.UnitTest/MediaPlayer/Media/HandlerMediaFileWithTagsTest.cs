using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Application.Handlers.Media;
using Noxy.NET.MediaPlayer.Application.Models.Media;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class HandlerMediaFileWithTagsTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeFileStorageService _storage = new();
    private readonly FakeMediaTagWriter _tagWriter = new();
    private readonly FakeLogger<HandlerMediaFileWithTags> _logger = new();
    private readonly FakeStore _store = new();
    private readonly HandlerMediaFileWithTags _handler;

    public HandlerMediaFileWithTagsTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), _storage, _tagWriter, _logger);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private static byte[] Mp3(int length = 50)
    {
        byte[] bytes = new byte[length];
        "ID3"u8.CopyTo(bytes);
        return bytes;
    }

    private MediaFile Seed(Guid userID, string title = "My Media")
    {
        byte[] bytes = Mp3();
        MediaFile file = new()
        {
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = bytes.Length,
            ContentHash = Convert.ToHexStringLower(SHA256.HashData(bytes)),
            UserID = userID
        };
        file.Metadata = new() { Title = title, Artist = "Someone", Album = "Album", MediaFileID = file.ID };

        _storage.Files[file.FilePath] = bytes;
        _store.MediaFileList.Add(file);
        return file;
    }

    [Fact]
    public async Task OpenWithTags_WritesTheCurrentMetadataIntoTheCopyOnly()
    {
        MediaFile file = Seed(UserID);
        string hash = file.ContentHash;
        DateTime modified = file.TimeModified;

        await using Stream stream = (await _handler.Handle(new(UserID, file.ID), Token)).Content;

        (string path, MediaFileMetadata metadata) = Assert.Single(_tagWriter.WriteList);
        Assert.NotEqual(file.FilePath, path);
        Assert.Contains(path, _storage.Temporary.Keys);
        Assert.Equal("My Media", metadata.Title);
        Assert.Equal("Someone", metadata.Artist);
        Assert.Equal(Mp3(), _storage.Files[file.FilePath]);
        Assert.Equal(hash, file.ContentHash);
        Assert.Equal(50, file.SizeBytes);
        Assert.Equal(modified, file.TimeModified);
    }

    [Fact]
    public async Task OpenWithTags_ReturnsTheContentNamedAfterTheTitle()
    {
        MediaFile file = Seed(UserID);

        MediaDownload download = await _handler.Handle(new(UserID, file.ID), Token);
        await using Stream stream = download.Content;
        using MemoryStream copy = new();
        await stream.CopyToAsync(copy, Token);

        Assert.Equal(Mp3(), copy.ToArray());
        Assert.Equal("My Media.mp3", download.FileName);
        Assert.Equal("audio/mpeg", download.ContentType);
    }

    [Fact]
    public async Task OpenWithTags_AfterAMetadataEdit_WritesTheNewTitle()
    {
        MediaFile file = Seed(UserID);
        file.Metadata = new() { Title = "Renamed", MediaFileID = file.ID };

        MediaDownload download = await _handler.Handle(new(UserID, file.ID), Token);
        await download.Content.DisposeAsync();

        Assert.Equal("Renamed", Assert.Single(_tagWriter.WriteList).Metadata.Title);
        Assert.Equal("Renamed.mp3", download.FileName);
    }

    [Fact]
    public async Task OpenWithTags_WhenTheWriterThrows_ServesTheUntaggedCopyAndLogsAWarning()
    {
        MediaFile file = Seed(UserID);
        _tagWriter.BeforeWrite = path => _storage.Temporary[path] = "\t\t\t"u8.ToArray();
        _tagWriter.Failure = new InvalidOperationException("Not parseable.");

        MediaDownload download = await _handler.Handle(new(UserID, file.ID), Token);
        using MemoryStream copy = new();
        await using (Stream stream = download.Content)
        {
            await stream.CopyToAsync(copy, Token);

            Assert.Single(_storage.Temporary);
        }

        Assert.Equal(Mp3(), copy.ToArray());
        Assert.Contains(_logger.EntryList, x => x.Level == LogLevel.Warning);
        Assert.Empty(_storage.Temporary);
    }

    [Fact]
    public async Task OpenWithTags_WhenOpeningTheCopyFails_RemovesTheCopyAndRethrows()
    {
        MediaFile file = Seed(UserID);
        _storage.OpenTemporaryFailure = new IOException("Disk gone.");

        await Assert.ThrowsAsync<IOException>(() => _handler.Handle(new(UserID, file.ID), Token).AsTask());

        Assert.Empty(_storage.Temporary);
    }

    [Fact]
    public async Task OpenWithTags_DisposingTheStream_RemovesTheCopy()
    {
        MediaFile file = Seed(UserID);

        MediaDownload download = await _handler.Handle(new(UserID, file.ID), Token);
        Assert.Single(_storage.Temporary);
        await download.Content.DisposeAsync();

        Assert.Empty(_storage.Temporary);
        Assert.Contains(file.FilePath, _storage.Files.Keys);
    }

    [Fact]
    public async Task OpenWithTags_TwoCalls_UseDifferentCopies()
    {
        MediaFile file = Seed(UserID);

        await using Stream first = (await _handler.Handle(new(UserID, file.ID), Token)).Content;
        await using Stream second = (await _handler.Handle(new(UserID, file.ID), Token)).Content;

        Assert.Equal(2, _storage.Temporary.Count);
        Assert.NotEqual(_tagWriter.WriteList[0].FilePath, _tagWriter.WriteList[1].FilePath);
    }

    [Fact]
    public async Task OpenWithTags_UnknownMediaFile_ThrowsMediaNotFound()
    {
        await Assert.ThrowsAsync<MediaNotFoundException>(() => _handler.Handle(new(UserID, Guid.CreateVersion7()), Token).AsTask());
    }

    [Fact]
    public async Task OpenWithTags_AnotherUsersMediaFile_ThrowsMediaNotFound_AndMakesNoCopy()
    {
        MediaFile file = Seed(UserID);

        await Assert.ThrowsAsync<MediaNotFoundException>(() => _handler.Handle(new(Guid.CreateVersion7(), file.ID), Token).AsTask());
        Assert.Empty(_storage.Temporary);
    }

    [Fact]
    public async Task OpenWithTags_FileMissingFromStorage_ThrowsMediaNotFound()
    {
        MediaFile file = Seed(UserID);
        _storage.Files.Remove(file.FilePath);

        await Assert.ThrowsAsync<MediaNotFoundException>(() => _handler.Handle(new(UserID, file.ID), Token).AsTask());
        Assert.Empty(_tagWriter.WriteList);
    }

    [Fact]
    public async Task OpenWithTags_MediaFileWithOnlyATitle_PassesNullForTheOtherFields()
    {
        MediaFile file = Seed(UserID);
        file.Metadata = new() { Title = "Only", MediaFileID = file.ID };

        await using Stream stream = (await _handler.Handle(new(UserID, file.ID), Token)).Content;

        MediaFileMetadata metadata = Assert.Single(_tagWriter.WriteList).Metadata;
        Assert.Equal("Only", metadata.Title);
        Assert.Null(metadata.Artist);
        Assert.Null(metadata.Album);
        Assert.Null(metadata.Genre);
        Assert.Null(metadata.Year);
    }
}
