using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Metadata;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerGenreMediaFileListAddTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerGenreMediaFileListAdd _handler;

    public HandlerGenreMediaFileListAddTest()
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
    public async Task Add_PutsTheGenreAtTheEndWithoutDuplicating_AndTouchesAndPushesOnlyTheChangedFiles()
    {
        EntityGenre genre = SeedGenre("Rock");
        EntityGenre other = SeedGenre("Pop");
        EntityMediaFile lacking = SeedFile("One", other);
        EntityMediaFile had = SeedFile("Two", genre);

        ResponseGenre response = await _handler.Handle(new CommandGenreMediaFileListAdd(UserID, genre.ID, [lacking.ID, had.ID]), Token);

        Assert.Equal([other.ID, genre.ID], Stored(lacking).GenreLinkList.OrderBy(x => x.Position).Select(x => x.GenreID).ToList());
        Assert.Single(Stored(had).GenreLinkList);
        Assert.Equal(_time.Now.UtcDateTime, Stored(lacking).TimeModified);
        Assert.Equal(Original, Stored(had).TimeModified);
        Assert.Equal(2, response.MediaFileCount);
        Assert.Equal("Pop; Rock", Assert.Single(_publisher.GetList<NotificationMediaUpdated>()).MediaFile.Genre);
        Assert.Equal(genre.ID, Assert.Single(_publisher.GetList<NotificationGenreUpdated>()).Genre.ID);
    }

    [Fact]
    public async Task Add_AnUnknownFileOrAnotherUsersGenre_IsNotFound_AndWhatIsAlreadyThereChangesNothing()
    {
        EntityGenre genre = SeedGenre("Rock");
        EntityGenre foreign = SeedGenre("Theirs", Guid.CreateVersion7());
        EntityMediaFile file = SeedFile("One", genre);

        await Assert.ThrowsAsync<MediaNotFoundException>(() => _handler.Handle(new CommandGenreMediaFileListAdd(UserID, genre.ID, [Guid.CreateVersion7()]), Token).AsTask());
        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => _handler.Handle(new CommandGenreMediaFileListAdd(UserID, foreign.ID, [file.ID]), Token).AsTask());
        await _handler.Handle(new CommandGenreMediaFileListAdd(UserID, genre.ID, [file.ID]), Token);

        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }
}
