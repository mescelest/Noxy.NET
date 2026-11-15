using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Genre;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerGenreMergeTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerGenreMerge _handler;

    public HandlerGenreMergeTest()
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
    public async Task Merge_MovesTheLinksWithoutDuplicates_RemovesTheSource_AndPushes()
    {
        EntityGenre source = SeedGenre("Source");
        EntityGenre target = SeedGenre("Target");
        EntityMediaFile both = SeedFile("One", source, target);
        EntityMediaFile onlySource = SeedFile("Two", source);
        EntityMediaFile untouched = SeedFile("Three", target);

        ResponseGenre response = await _handler.Handle(new CommandGenreMerge(UserID, source.ID, target.ID), Token);

        Assert.Equal(target.ID, response.ID);
        Assert.Equal(3, response.MediaFileCount);
        Assert.DoesNotContain(_store.GenreList, x => x.ID == source.ID);
        Assert.Equal([target.ID], Stored(both).GenreLinkList.Select(x => x.GenreID).ToList());
        Assert.Equal([target.ID], Stored(onlySource).GenreLinkList.Select(x => x.GenreID).ToList());
        Assert.Equal(Original, Stored(untouched).TimeModified);
        Assert.Equal(2, _publisher.GetList<NotificationMediaUpdated>().Count);
        Assert.Equal(source.ID, Assert.Single(_publisher.GetList<NotificationGenreDeleted>()).ID);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task Merge_IntoItselfIsInvalid_AndAnotherUsersGenreIsNotFound()
    {
        EntityGenre mine = SeedGenre("Mine");
        EntityGenre foreign = SeedGenre("Theirs", Guid.CreateVersion7());

        await Assert.ThrowsAsync<InvalidMetadataOperationException>(() => _handler.Handle(new CommandGenreMerge(UserID, mine.ID, mine.ID), Token).AsTask());
        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => _handler.Handle(new CommandGenreMerge(UserID, mine.ID, foreign.ID), Token).AsTask());

        Assert.Equal(0, _store.CommitCount);
    }
}
