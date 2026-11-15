using System.Security.Cryptography;
using Noxy.NET.MediaPlayer.Application.Handlers.Media;
using Noxy.NET.MediaPlayer.Application.Models.Media;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class HandlerMediaLibraryTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly HandlerMediaLibrary _handler;

    public HandlerMediaLibraryTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), new ETagService());
    }

    private MediaFile Seed(Guid? userID = null, string title = "Media", DateTime? timeCreated = null)
    {
        byte[] bytes = [1, 2, 3];
        MediaFile file = new()
        {
            TimeCreated = timeCreated ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TimeModified = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = bytes.Length,
            ContentHash = Convert.ToHexStringLower(SHA256.HashData(bytes)),
            UserID = userID ?? UserID
        };
        file.Metadata = new() { Title = title, Artist = "Artist", Album = "Album", Genre = "Genre", Year = 1999, MediaFileID = file.ID };

        _store.MediaFileList.Add(file);
        return file;
    }

    private async Task<MediaLibrary> Library(string? ifNoneMatch = null, Guid? userID = null)
    {
        return await _handler.Handle(new(userID ?? UserID, ifNoneMatch), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Handle_ETagIsAQuotedLowercaseSha256()
    {
        Seed();

        MediaLibrary library = await Library();

        Assert.Matches("^\"[0-9a-f]{64}\"$", library.ETag);
    }

    [Fact]
    public async Task Handle_EmptyLibrary_HasAStableETagAndAnEmptyList()
    {
        MediaLibrary first = await Library();
        MediaLibrary second = await Library();

        Assert.Equal(first.ETag, second.ETag);
        Assert.NotNull(first.MediaFileList);
        Assert.Empty(first.MediaFileList);
    }

    [Fact]
    public async Task Handle_SameState_GivesTheSameETag()
    {
        Seed();
        Seed();

        Assert.Equal((await Library()).ETag, (await Library()).ETag);
    }

    [Fact]
    public async Task Handle_ChangesAfterAnEdit()
    {
        MediaFile file = Seed();
        string before = (await Library()).ETag;

        file.TimeModified = file.TimeModified.AddMinutes(1);

        Assert.NotEqual(before, (await Library()).ETag);
    }

    [Fact]
    public async Task Handle_ChangesAfterADelete()
    {
        MediaFile file = Seed();
        Seed();
        string before = (await Library()).ETag;

        _store.MediaFileList.Remove(file);

        Assert.NotEqual(before, (await Library()).ETag);
    }

    [Fact]
    public async Task Handle_ChangesAfterANewFile()
    {
        Seed();
        string before = (await Library()).ETag;

        Seed();

        Assert.NotEqual(before, (await Library()).ETag);
    }

    [Fact]
    public async Task Handle_IsPerUser()
    {
        Seed();
        string before = (await Library()).ETag;

        Seed(Guid.CreateVersion7());

        Assert.Equal(before, (await Library()).ETag);
    }

    [Theory]
    [InlineData("{0}")]
    [InlineData("W/{0}")]
    [InlineData("\"other\", {0}")]
    [InlineData("{0} , \"other\"")]
    [InlineData("*")]
    public async Task Handle_MatchingIfNoneMatch_ReturnsNoList(string template)
    {
        Seed();
        string eTag = (await Library()).ETag;

        MediaLibrary library = await Library(string.Format(template, eTag));

        Assert.Equal(eTag, library.ETag);
        Assert.Null(library.MediaFileList);
    }

    [Theory]
    [InlineData("\"other\"")]
    [InlineData("garbage")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Handle_MismatchedOrMalformedIfNoneMatch_ReturnsTheList(string? ifNoneMatch)
    {
        Seed();
        Seed();

        MediaLibrary library = await Library(ifNoneMatch);

        Assert.Equal(2, library.MediaFileList?.Count);
    }

    [Fact]
    public async Task Handle_ListIsNewestFirst_WithTags()
    {
        DateTime now = new(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        Seed(title: "Older", timeCreated: now.AddDays(-2));
        Seed(title: "Newer", timeCreated: now);

        MediaLibrary library = await Library();

        Assert.NotNull(library.MediaFileList);
        Assert.Equal(["Newer", "Older"], library.MediaFileList.Select(x => x.Title));
        Assert.Equal("Artist", library.MediaFileList[0].Artist);
        Assert.Equal(1999, library.MediaFileList[0].Year);
    }
}
