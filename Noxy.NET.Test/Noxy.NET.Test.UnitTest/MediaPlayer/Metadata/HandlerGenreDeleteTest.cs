using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Genre;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerGenreDeleteTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerGenreDelete _handler;

    public HandlerGenreDeleteTest()
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

    [Fact]
    public async Task Delete_RemovesTheGenreFromItsFilesWhichStayAndArePushed()
    {
        EntityGenre genre = SeedGenre("Rock");
        EntityGenre other = SeedGenre("Pop");
        EntityMediaFile had = SeedFile("One", genre, other);
        EntityMediaFile unrelated = SeedFile("Two", other);

        await _handler.Handle(new CommandGenreDelete(UserID, genre.ID), Token);

        Assert.Equal(other.ID, Assert.Single(_store.GenreList).ID);
        Assert.Equal(2, _store.MediaFileList.Count);
        Assert.Equal([other.ID], _store.MediaFileList.Single(x => x.ID == had.ID).GenreLinkList.Select(x => x.GenreID).ToList());
        Assert.Equal(_time.Now.UtcDateTime, _store.MediaFileList.Single(x => x.ID == had.ID).TimeModified);
        Assert.Equal(Original, _store.MediaFileList.Single(x => x.ID == unrelated.ID).TimeModified);
        Assert.Equal("Pop", Assert.Single(_publisher.GetList<NotificationMediaUpdated>()).MediaFile.Genre);
        Assert.Equal(genre.ID, Assert.Single(_publisher.GetList<NotificationGenreDeleted>()).ID);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task Delete_AnotherUsersGenre_ThrowsNotFound_AndChangesNothing()
    {
        EntityGenre foreign = SeedGenre("Theirs", Guid.CreateVersion7());

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => _handler.Handle(new CommandGenreDelete(UserID, foreign.ID), Token).AsTask());

        Assert.Single(_store.GenreList);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Delete_WhenTheCommitFails_PublishesNothing()
    {
        EntityGenre genre = SeedGenre("Rock");
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(new CommandGenreDelete(UserID, genre.ID), Token).AsTask());

        Assert.Empty(_publisher.PublishedList);
    }
}
