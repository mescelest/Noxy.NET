using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Metadata;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerGenreMediaFileListRemoveTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerGenreMediaFileListRemove _handler;

    public HandlerGenreMediaFileListRemoveTest()
    {
        _time.Now = new(2027, 5, 6, 7, 8, 9, TimeSpan.Zero);
        _handler = new(new FakeUnitOfWorkFactory(_store), new GenreItemService(), new NameKeyService(), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityGenre SeedGenre(string name)
    {
        EntityGenre genre = new() { UserID = UserID, Name = name, NameKey = name.ToLowerInvariant(), TimeModified = Original };
        _store.GenreList.Add(genre);
        return genre;
    }

    private EntityMediaFile SeedFile(string title, Guid? userID = null, params EntityGenre[] genreList)
    {
        EntityMediaFile file = new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new string('a', 64), UserID = userID ?? UserID, TimeModified = Original };
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
    public async Task Remove_ClearsOnlyThatGenreFromTheFiles_AndTouchesAndPushesThem()
    {
        EntityGenre genre = SeedGenre("Rock");
        EntityGenre other = SeedGenre("Pop");
        EntityMediaFile file = SeedFile("One", null, genre, other);
        EntityMediaFile untouched = SeedFile("Two", null, genre);

        ResponseGenre response = await _handler.Handle(new CommandGenreMediaFileListRemove(UserID, genre.ID, [file.ID]), Token);

        Assert.Equal([other.ID], Stored(file).GenreLinkList.Select(x => x.GenreID).ToList());
        Assert.Single(Stored(untouched).GenreLinkList);
        Assert.Equal(_time.Now.UtcDateTime, Stored(file).TimeModified);
        Assert.Equal(1, response.MediaFileCount);
        Assert.Equal("Pop", Assert.Single(_publisher.GetList<NotificationMediaUpdated>()).MediaFile.Genre);
        Assert.Contains(_store.GenreList, x => x.ID == genre.ID);
    }

    [Fact]
    public async Task Remove_AFileWithoutTheGenre_ChangesNothing_AndAForeignFileIsNotFound()
    {
        EntityGenre genre = SeedGenre("Rock");
        EntityMediaFile file = SeedFile("One");
        EntityMediaFile foreign = SeedFile("Theirs", Guid.CreateVersion7(), genre);

        await _handler.Handle(new CommandGenreMediaFileListRemove(UserID, genre.ID, [file.ID]), Token);
        await Assert.ThrowsAsync<MediaNotFoundException>(() => _handler.Handle(new CommandGenreMediaFileListRemove(UserID, genre.ID, [foreign.ID]), Token).AsTask());

        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }
}
