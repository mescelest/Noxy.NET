using Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Artist;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerArtistSetWeightTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerArtistSetWeight _handler;

    public HandlerArtistSetWeightTest()
    {
        _time.Now = new(2027, 5, 6, 7, 8, 9, TimeSpan.Zero);
        _handler = new(new FakeUnitOfWorkFactory(_store), new ArtistItemService(new MetadataChangeService()), new NameKeyService(), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityArtist SeedArtist(string name, Guid? userID = null)
    {
        EntityArtist artist = new() { UserID = userID ?? UserID, Name = name, NameKey = name.ToLowerInvariant(), TimeModified = Original };
        _store.ArtistList.Add(artist);
        return artist;
    }

    private EntityMediaFile SeedFile(string title, params EntityArtist[] artistList)
    {
        EntityMediaFile file = new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new('a', 64), UserID = UserID, TimeModified = Original };
        file.Metadata = new() { Title = title, MediaFileID = file.ID };
        file.ArtistLinkList = [.. artistList.Select((x, index) => new EntityMediaFileArtist { MediaFileID = file.ID, ArtistID = x.ID, Position = index })];
        _store.MediaFileList.Add(file);
        return file;
    }

    private EntityMediaFile Stored(EntityMediaFile file)
    {
        return _store.MediaFileList.Single(x => x.ID == file.ID);
    }

    private Task<ResponseArtist> SetWeight(EntityArtist artist, int weight, Guid? userID = null)
    {
        return _handler.Handle(new(userID ?? UserID, artist.ID, weight), Token).AsTask();
    }

    [Fact]
    public async Task SetWeight_ChangesTheWeight_AndPushesTheItem()
    {
        EntityArtist artist = SeedArtist("Guest");

        ResponseArtist response = await SetWeight(artist, 20);

        Assert.Equal(20, response.Weight);
        Assert.Equal(20, _store.ArtistList.Single(x => x.ID == artist.ID).Weight);
        Assert.Equal(_time.Now.UtcDateTime, _store.ArtistList.Single(x => x.ID == artist.ID).TimeModified);
        Assert.Equal(20, Assert.Single(_publisher.GetList<NotificationArtistUpdated>()).Artist.Weight);
    }

    [Fact]
    public async Task SetWeight_TouchesAndPushesTheFilesThatCarryTheItem_InTheNewOrder()
    {
        EntityArtist soundtrack = SeedArtist("Guest");
        EntityArtist main = SeedArtist("Main");
        EntityMediaFile had = SeedFile("One", soundtrack, main);
        EntityMediaFile unrelated = SeedFile("Two", main);

        await SetWeight(soundtrack, 20);

        Assert.Equal(_time.Now.UtcDateTime, Stored(had).TimeModified);
        Assert.Equal(Original, Stored(unrelated).TimeModified);
        NotificationMediaUpdated pushed = Assert.Single(_publisher.GetList<NotificationMediaUpdated>());
        Assert.Equal("Main; Guest", pushed.MediaFile.Artist);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1001)]
    public async Task SetWeight_OutOfRange_Throws_AndChangesNothing(int weight)
    {
        EntityArtist artist = SeedArtist("Guest");

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => SetWeight(artist, weight));

        Assert.Equal(MetadataWeight.Default, artist.Weight);
        Assert.Equal(0, _store.CommitCount);
    }

    [Theory]
    [InlineData(MetadataWeight.Minimum)]
    [InlineData(MetadataWeight.Maximum)]
    public async Task SetWeight_AtTheLimits_Works(int weight)
    {
        EntityArtist artist = SeedArtist("Guest");

        ResponseArtist response = await SetWeight(artist, weight);

        Assert.Equal(weight, response.Weight);
    }

    [Fact]
    public async Task SetWeight_ForAnotherUsersArtist_ThrowsNotFound()
    {
        EntityArtist artist = SeedArtist("Guest", Guid.CreateVersion7());

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => SetWeight(artist, 20));
    }

    [Fact]
    public async Task SetWeight_ToTheSameValue_ChangesNothing()
    {
        EntityArtist artist = SeedArtist("Guest");
        SeedFile("One", artist);

        await SetWeight(artist, MetadataWeight.Default);

        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
        Assert.Equal(Original, artist.TimeModified);
    }

    [Fact]
    public async Task SetWeight_OnAnArtistNoFileCarries_PushesOnlyTheItem()
    {
        EntityArtist artist = SeedArtist("Guest");

        await SetWeight(artist, 20);

        Assert.Empty(_publisher.GetList<NotificationMediaUpdated>());
        Assert.Single(_publisher.GetList<NotificationArtistUpdated>());
    }
}
