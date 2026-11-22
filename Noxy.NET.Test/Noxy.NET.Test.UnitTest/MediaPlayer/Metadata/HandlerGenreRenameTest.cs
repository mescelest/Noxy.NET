using Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Genre;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerGenreRenameTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerGenreRename _handler;

    public HandlerGenreRenameTest()
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
        EntityMediaFile file = new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new('a', 64), UserID = UserID, TimeModified = Original };
        file.Metadata = new() { Title = title, MediaFileID = file.ID };
        file.GenreLinkList = [.. genreList.Select((x, index) => new EntityMediaFileGenre { MediaFileID = file.ID, GenreID = x.ID, Position = index })];
        _store.MediaFileList.Add(file);
        return file;
    }

    private EntityMediaFile Stored(EntityMediaFile file)
    {
        return _store.MediaFileList.Single(x => x.ID == file.ID);
    }

    private Task<ResponseGenre> Rename(EntityGenre genre, string name)
    {
        return _handler.Handle(new(UserID, genre.ID, name), Token).AsTask();
    }

    [Fact]
    public async Task Rename_ChangesTheNameAndTouchesAndPushesTheFilesThatHaveIt()
    {
        EntityGenre genre = SeedGenre("Old");
        EntityGenre other = SeedGenre("Other");
        EntityMediaFile had = SeedFile("One", genre, other);
        EntityMediaFile unrelated = SeedFile("Two", other);

        ResponseGenre response = await Rename(genre, "New");

        Assert.Equal("New", response.Name);
        Assert.Equal(1, response.MediaFileCount);
        Assert.Equal("new", _store.GenreList.Single(x => x.ID == genre.ID).NameKey);
        Assert.Equal(_time.Now.UtcDateTime, Stored(had).TimeModified);
        Assert.Equal(Original, Stored(unrelated).TimeModified);
        NotificationMediaUpdated pushed = Assert.Single(_publisher.GetList<NotificationMediaUpdated>());
        Assert.Equal("New; Other", pushed.MediaFile.Genre);
        Assert.Equal("New", Assert.Single(_publisher.GetList<NotificationGenreUpdated>()).Genre.Name);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task Rename_ToTheNameOfAnotherGenre_IsAConflictNamingItWhenAutoMergeIsOff()
    {
        EntityGenre genre = SeedGenre("A");
        EntityGenre other = SeedGenre("B");

        NameAlreadyInUseException exception = await Assert.ThrowsAsync<NameAlreadyInUseException>(() => Rename(genre, "b"));

        Assert.Equal(other.ID, exception.ExistingID);
        Assert.Equal(2, _store.GenreList.Count);
        Assert.Equal(0, _store.CommitCount);
    }

    [Fact]
    public async Task Rename_ToTheNameOfAnotherGenre_MergesWhenAutoMergeIsOn()
    {
        _store.UserSettingList.Add(new EntityUserSettingBoolean { UserID = UserID, Key = SettingKey.MergeOnRename, Value = true });
        EntityGenre genre = SeedGenre("A");
        EntityGenre other = SeedGenre("B");
        EntityMediaFile file = SeedFile("One", genre);

        ResponseGenre response = await Rename(genre, "B");

        Assert.Equal(other.ID, response.ID);
        Assert.Equal(1, response.MediaFileCount);
        Assert.Equal(other.ID, Assert.Single(_store.GenreList).ID);
        Assert.Equal([other.ID], Stored(file).GenreLinkList.Select(x => x.GenreID).ToList());
        Assert.Equal(genre.ID, Assert.Single(_publisher.GetList<NotificationGenreDeleted>()).ID);
    }

    [Fact]
    public async Task Rename_ADifferentSpellingOfTheSameName_IsNotACollision()
    {
        EntityGenre genre = SeedGenre("rock");

        ResponseGenre response = await Rename(genre, "ROCK");

        Assert.Equal("ROCK", response.Name);
    }

    [Fact]
    public async Task Rename_ToTheSameName_ChangesNothing()
    {
        EntityGenre genre = SeedGenre("Same");

        await Rename(genre, "Same");

        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Rename_AnotherUsersGenre_ThrowsNotFound()
    {
        EntityGenre foreign = SeedGenre("Theirs", Guid.CreateVersion7());

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => Rename(foreign, "Mine"));
    }
}
