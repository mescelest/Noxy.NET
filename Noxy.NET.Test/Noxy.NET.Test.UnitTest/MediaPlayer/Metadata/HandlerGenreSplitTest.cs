using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Genre;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerGenreSplitTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerGenreSplit _handler;

    public HandlerGenreSplitTest()
    {
        _time.Now = new(2027, 5, 6, 7, 8, 9, TimeSpan.Zero);
        _handler = new(new FakeUnitOfWorkFactory(_store), new GenreItemService(), new NameKeyService(), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityGenre SeedGenre(string name, Guid? userID = null)
    {
        EntityGenre genre = new() { UserID = userID ?? UserID, Name = name, NameKey = name.ToLowerInvariant(), TimeModified = Original };
        _store.GenreList.Add(genre);
        return genre;
    }

    private EntityMediaFile SeedFile(string title, params EntityGenre[] genreList)
    {
        EntityMediaFile file = new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new string('a', 64), UserID = UserID, TimeModified = Original };
        file.Metadata = new() { Title = title, MediaFileID = file.ID };
        file.GenreLinkList = [.. genreList.Select((x, index) => new EntityMediaFileGenre { MediaFileID = file.ID, GenreID = x.ID, Position = index })];
        _store.MediaFileList.Add(file);
        return file;
    }

    private EntityMediaFile Stored(EntityMediaFile file)
    {
        return _store.MediaFileList.Single(x => x.ID == file.ID);
    }

    [Fact]
    public async Task Split_ReplacesTheGenreByTheNamedGenresInThePositionAndRemovesIt()
    {
        EntityGenre combined = SeedGenre("Rock & Roll");
        EntityGenre before = SeedGenre("Before");
        EntityGenre existing = SeedGenre("Roll");
        EntityMediaFile file = SeedFile("One", before, combined);

        List<ResponseGenre> responseList = await _handler.Handle(new CommandGenreSplit(UserID, combined.ID, ["Rock", "roll"]), Token);

        Assert.Equal(["Rock", "Roll"], responseList.Select(x => x.Name).ToList());
        Assert.Equal(existing.ID, responseList[1].ID);
        Assert.Equal(["Before", "Rock", "Roll"], Stored(file).GenreLinkList.OrderBy(x => x.Position).Select(x => _store.GenreList.Single(g => g.ID == x.GenreID).Name).ToList());
        Assert.DoesNotContain(_store.GenreList, x => x.ID == combined.ID);
        Assert.Equal(_time.Now.UtcDateTime, Stored(file).TimeModified);
        Assert.Equal(combined.ID, Assert.Single(_publisher.GetList<NotificationGenreDeleted>()).ID);
        Assert.Equal(2, _publisher.GetList<NotificationGenreUpdated>().Count);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task Split_FewerThanTwoDifferentNamesIsInvalid_AndAnotherUsersGenreIsNotFound()
    {
        EntityGenre combined = SeedGenre("Rock & Roll");
        EntityGenre foreign = SeedGenre("Theirs", Guid.CreateVersion7());

        await Assert.ThrowsAsync<InvalidMetadataOperationException>(() => _handler.Handle(new CommandGenreSplit(UserID, combined.ID, ["Rock", "rock"]), Token).AsTask());
        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => _handler.Handle(new CommandGenreSplit(UserID, foreign.ID, ["A", "B"]), Token).AsTask());

        Assert.Equal(0, _store.CommitCount);
    }
}
