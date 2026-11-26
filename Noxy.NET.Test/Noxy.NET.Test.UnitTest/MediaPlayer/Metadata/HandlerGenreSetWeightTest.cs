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

public class HandlerGenreSetWeightTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerGenreSetWeight _handler;

    public HandlerGenreSetWeightTest()
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

    private Task<ResponseGenre> SetWeight(EntityGenre genre, int weight, Guid? userID = null)
    {
        return _handler.Handle(new(userID ?? UserID, genre.ID, weight), Token).AsTask();
    }

    [Fact]
    public async Task SetWeight_ChangesTheWeight_AndPushesTheItem()
    {
        EntityGenre genre = SeedGenre("Soundtrack");

        ResponseGenre response = await SetWeight(genre, 20);

        Assert.Equal(20, response.Weight);
        Assert.Equal(20, _store.GenreList.Single(x => x.ID == genre.ID).Weight);
        Assert.Equal(_time.Now.UtcDateTime, _store.GenreList.Single(x => x.ID == genre.ID).TimeModified);
        Assert.Equal(20, Assert.Single(_publisher.GetList<NotificationGenreUpdated>()).Genre.Weight);
    }

    [Fact]
    public async Task SetWeight_TouchesAndPushesTheFilesThatCarryTheItem_InTheNewOrder()
    {
        EntityGenre soundtrack = SeedGenre("Soundtrack");
        EntityGenre pop = SeedGenre("Pop");
        EntityMediaFile had = SeedFile("One", soundtrack, pop);
        EntityMediaFile unrelated = SeedFile("Two", pop);

        await SetWeight(soundtrack, 20);

        Assert.Equal(_time.Now.UtcDateTime, Stored(had).TimeModified);
        Assert.Equal(Original, Stored(unrelated).TimeModified);
        NotificationMediaUpdated pushed = Assert.Single(_publisher.GetList<NotificationMediaUpdated>());
        Assert.Equal("Pop; Soundtrack", pushed.MediaFile.Genre);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1001)]
    public async Task SetWeight_OutOfRange_Throws_AndChangesNothing(int weight)
    {
        EntityGenre genre = SeedGenre("Soundtrack");

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => SetWeight(genre, weight));

        Assert.Equal(MetadataWeight.Default, genre.Weight);
        Assert.Equal(0, _store.CommitCount);
    }

    [Theory]
    [InlineData(MetadataWeight.Minimum)]
    [InlineData(MetadataWeight.Maximum)]
    public async Task SetWeight_AtTheLimits_Works(int weight)
    {
        EntityGenre genre = SeedGenre("Soundtrack");

        ResponseGenre response = await SetWeight(genre, weight);

        Assert.Equal(weight, response.Weight);
    }

    [Fact]
    public async Task SetWeight_ForAnotherUsersGenre_ThrowsNotFound()
    {
        EntityGenre genre = SeedGenre("Soundtrack", Guid.CreateVersion7());

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => SetWeight(genre, 20));
    }

    [Fact]
    public async Task SetWeight_ToTheSameValue_ChangesNothing()
    {
        EntityGenre genre = SeedGenre("Soundtrack");
        SeedFile("One", genre);

        await SetWeight(genre, MetadataWeight.Default);

        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
        Assert.Equal(Original, genre.TimeModified);
    }

    [Fact]
    public async Task SetWeight_OnAGenreNoFileCarries_PushesOnlyTheItem()
    {
        EntityGenre genre = SeedGenre("Soundtrack");

        await SetWeight(genre, 20);

        Assert.Empty(_publisher.GetList<NotificationMediaUpdated>());
        Assert.Single(_publisher.GetList<NotificationGenreUpdated>());
    }
}
