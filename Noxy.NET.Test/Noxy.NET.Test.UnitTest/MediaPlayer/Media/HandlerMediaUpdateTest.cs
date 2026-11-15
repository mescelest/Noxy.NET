using System.Security.Cryptography;
using Noxy.NET.MediaPlayer.Application.Commands.Media;
using Noxy.NET.MediaPlayer.Application.Handlers.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class HandlerMediaUpdateTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerMediaUpdate _handler;

    public HandlerMediaUpdateTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), _publisher, _time);
    }

    private EntityMediaFile Seed(Guid? userID = null, string title = "Media", DateTime? timeCreated = null, DateTime? timeModified = null)
    {
        byte[] bytes = [1, 2, 3];
        EntityMediaFile file = new()
        {
            TimeCreated = timeCreated ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TimeModified = timeModified ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
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

    private EntityMediaFile Stored(EntityMediaFile file)
    {
        return _store.MediaFileList.Single(x => x.ID == file.ID);
    }

    // The changes are given as a command with placeholder user and ID, which are filled in here like the controller fills them from the token and the route.
    private async Task<ResponseMediaFile> Update(EntityMediaFile file, CommandMediaUpdate changes, Guid? userID = null)
    {
        return await _handler.Handle(changes with { UserID = userID ?? UserID, ID = file.ID }, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Update_ChangesOnlyTheSentTags()
    {
        EntityMediaFile file = Seed();

        ResponseMediaFile response = await Update(file, new(Guid.Empty, Guid.Empty) { Artist = new("New Artist") });

        Assert.Equal("New Artist", response.Artist);
        Assert.Equal("Album", response.Album);
        Assert.Equal("Media", response.Title);
        Assert.Equal("New Artist", Stored(file).Metadata?.Artist);
        Assert.Equal("Album", Stored(file).Metadata?.Album);
    }

    [Fact]
    public async Task Update_StoresEveryEditableTag()
    {
        EntityMediaFile file = Seed();

        ResponseMediaFile response = await Update(file, new(Guid.Empty, Guid.Empty)
        {
            Title = new("T"),
            Artist = new("Ar"),
            Album = new("Al"),
            AlbumArtist = new("AA"),
            Subtitle = new("S"),
            Comment = new("C"),
            Genre = new("G"),
            TrackNumber = new(4),
            DiscNumber = new(2),
            Year = new(2024)
        });

        Assert.Equal(["T", "Ar", "Al", "AA", "S", "C", "G"], [response.Title, response.Artist, response.Album, response.AlbumArtist, response.Subtitle, response.Comment, response.Genre]);
        Assert.Equal([4, 2, 2024], [response.TrackNumber, response.DiscNumber, response.Year]);
        Assert.Equal(2024, Stored(file).Metadata?.Year);
    }

    [Fact]
    public async Task Update_NullClearsATag()
    {
        EntityMediaFile file = Seed();

        ResponseMediaFile response = await Update(file, new(Guid.Empty, Guid.Empty) { Artist = new(null), Year = new(null) });

        Assert.Null(response.Artist);
        Assert.Null(response.Year);
        Assert.Null(Stored(file).Metadata?.Artist);
        Assert.Null(Stored(file).Metadata?.Year);
        Assert.Equal("Album", Stored(file).Metadata?.Album);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Update_EmptyOrWhitespaceNullableTag_ClearsIt(string value)
    {
        EntityMediaFile file = Seed();

        ResponseMediaFile response = await Update(file, new(Guid.Empty, Guid.Empty) { Artist = new(value), Genre = new(value) });

        Assert.Null(response.Artist);
        Assert.Null(response.Genre);
        Assert.Null(Stored(file).Metadata?.Artist);
    }

    [Fact]
    public async Task Update_TextIsTrimmed()
    {
        EntityMediaFile file = Seed();

        ResponseMediaFile response = await Update(file, new(Guid.Empty, Guid.Empty) { Title = new("  New Title  "), Album = new("  Other ") });

        Assert.Equal("New Title", response.Title);
        Assert.Equal("Other", response.Album);
    }

    [Fact]
    public async Task Update_BumpsTimeModifiedToTheClock_AndKeepsTimeCreated()
    {
        EntityMediaFile file = Seed();
        _time.Now = new(2027, 5, 6, 7, 8, 9, TimeSpan.Zero);

        ResponseMediaFile response = await Update(file, new(Guid.Empty, Guid.Empty) { Title = new("Changed") });

        Assert.Equal(_time.Now.UtcDateTime, response.TimeModified);
        Assert.Equal(_time.Now.UtcDateTime, Stored(file).TimeModified);
        Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), response.TimeCreated);
    }

    [Fact]
    public async Task Update_CommitsOnceAndPushesTheNewStateToTheOwner()
    {
        EntityMediaFile file = Seed();

        ResponseMediaFile response = await Update(file, new(Guid.Empty, Guid.Empty) { Title = new("Changed") });

        Assert.Equal(1, _store.CommitCount);
        NotificationMediaUpdated pushed = Assert.Single(_publisher.GetList<NotificationMediaUpdated>());
        Assert.Equal(UserID, pushed.UserID);
        Assert.Equal(file.ID, pushed.MediaFile.ID);
        Assert.Equal("Changed", pushed.MediaFile.Title);
        Assert.Equal(response.TimeModified, pushed.MediaFile.TimeModified);
    }

    [Fact]
    public async Task Update_NothingChanged_DoesNotCommitBumpOrPush_AndReturnsTheMediaFile()
    {
        EntityMediaFile file = Seed();
        DateTime before = file.TimeModified;
        _time.Now = new(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);

        ResponseMediaFile same = await Update(file, new(Guid.Empty, Guid.Empty) { Title = new("Media"), Artist = new("Artist"), Year = new(1999) });
        ResponseMediaFile empty = await Update(file, new(Guid.Empty, Guid.Empty));

        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.GetList<NotificationMediaUpdated>());
        Assert.Equal(before, Stored(file).TimeModified);
        Assert.Equal("Media", same.Title);
        Assert.Equal(file.ID, empty.ID);
    }

    [Fact]
    public async Task Update_AnotherUsersMediaFile_ThrowsNotFound_AndPushesNothing()
    {
        EntityMediaFile file = Seed();

        await Assert.ThrowsAsync<MediaNotFoundException>(() => Update(file, new(Guid.Empty, Guid.Empty) { Title = new("Hijacked") }, Guid.CreateVersion7()));

        Assert.Equal("Media", Stored(file).Metadata?.Title);
        Assert.Empty(_publisher.GetList<NotificationMediaUpdated>());
        Assert.Equal(0, _store.CommitCount);
    }

    [Fact]
    public async Task Update_UnknownId_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<MediaNotFoundException>(() => _handler.Handle(new(UserID, Guid.CreateVersion7()) { Title = new("x") }, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Update_MediaFileWithoutMetadata_ThrowsNotFound()
    {
        EntityMediaFile file = Seed();
        file.Metadata = null;

        await Assert.ThrowsAsync<MediaNotFoundException>(() => Update(file, new(Guid.Empty, Guid.Empty) { Title = new("x") }));
    }

    [Fact]
    public async Task Update_NoOpEdit_DoesNotChangeTheLibraryETag()
    {
        EntityMediaFile file = Seed();
        HandlerMediaLibrary library = new(new FakeUnitOfWorkFactory(_store), new ETagService());
        string before = (await library.Handle(new(UserID, null), TestContext.Current.CancellationToken)).ETag;

        await Update(file, new(Guid.Empty, Guid.Empty) { Title = new("Media") });

        Assert.Equal(before, (await library.Handle(new(UserID, null), TestContext.Current.CancellationToken)).ETag);
    }

    [Fact]
    public async Task Update_WhenTheCommitFails_PublishesNothing()
    {
        EntityMediaFile file = Seed();
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Update(file, new(Guid.Empty, Guid.Empty) { Title = new("Changed") }));

        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Update_PublishesOnlyAfterTheCommit()
    {
        EntityMediaFile file = Seed();
        int commits = _store.CommitCount;
        int commitsAtPublish = -1;
        _publisher.OnPublish = _ => commitsAtPublish = _store.CommitCount;

        await Update(file, new(Guid.Empty, Guid.Empty) { Title = new("Changed") });

        Assert.Equal(commits + 1, commitsAtPublish);
    }

    [Fact]
    public async Task Update_PassesTheCancellationTokenToThePublish()
    {
        EntityMediaFile file = Seed();
        using CancellationTokenSource source = new();

        await _handler.Handle(new(UserID, file.ID) { Title = new("Changed") }, source.Token);

        Assert.Equal(source.Token, Assert.Single(_publisher.TokenList));
    }
}
