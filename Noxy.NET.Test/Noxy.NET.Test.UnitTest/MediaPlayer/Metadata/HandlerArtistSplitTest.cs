using Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Artist;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerArtistSplitTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerArtistSplit _handler;

    public HandlerArtistSplitTest()
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

    private EntityMediaFile SeedFile(string title, EntityAlbum? album = null, params EntityArtist[] artistList)
    {
        EntityMediaFile file = new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new('a', 64), UserID = UserID, TimeModified = Original, AlbumID = album?.ID };
        file.Metadata = new() { Title = title, MediaFileID = file.ID };
        file.ArtistLinkList = [.. artistList.Select((x, index) => new EntityMediaFileArtist { MediaFileID = file.ID, ArtistID = x.ID, Position = index })];
        _store.MediaFileList.Add(file);
        return file;
    }

    private EntityAlbum SeedAlbum(string title, params EntityArtist[] artistList)
    {
        EntityAlbum album = new() { UserID = UserID, Title = title, TitleKey = title.ToLowerInvariant(), IdentityKey = EntityAlbum.BuildIdentityKey(title.ToLowerInvariant(), artistList.Select(x => x.ID)), TimeModified = Original };
        album.ArtistLinkList = [.. artistList.Select((x, index) => new EntityAlbumArtist { AlbumID = album.ID, ArtistID = x.ID, Position = index })];
        _store.AlbumList.Add(album);
        return album;
    }

    private EntityMediaFile Stored(EntityMediaFile file)
    {
        return _store.MediaFileList.Single(x => x.ID == file.ID);
    }

    private List<string> ArtistNameList(EntityMediaFile file)
    {
        return [.. Stored(file).ArtistLinkList.OrderBy(x => x.Position).Select(x => _store.ArtistList.Single(a => a.ID == x.ArtistID).Name)];
    }

    private Task<List<ResponseArtist>> Split(EntityArtist artist, params string[] nameList)
    {
        return _handler.Handle(new(UserID, artist.ID, [.. nameList]), Token).AsTask();
    }

    [Fact]
    public async Task Split_ReplacesTheItemByTheNamedItemsInTheOriginalPositionAndRemovesIt()
    {
        EntityArtist combined = SeedArtist("A & B");
        EntityArtist before = SeedArtist("Before");
        EntityArtist after = SeedArtist("After");
        EntityMediaFile file = SeedFile("One", null, before, combined, after);

        List<ResponseArtist> responseList = await Split(combined, "A", "B");

        Assert.Equal(["Before", "A", "B", "After"], ArtistNameList(file));
        Assert.DoesNotContain(_store.ArtistList, x => x.ID == combined.ID);
        Assert.Equal(["A", "B"], responseList.Select(x => x.Name).ToList());
        Assert.All(responseList, x => Assert.Equal(1, x.MediaFileCount));
    }

    [Fact]
    public async Task Split_ReusesExistingItemsAndCreatesTheRest_ByTheKeyRules()
    {
        EntityArtist combined = SeedArtist("A & B");
        EntityArtist existing = SeedArtist("Existing");
        SeedFile("One", null, combined);

        List<ResponseArtist> responseList = await Split(combined, " EXISTING ", "Brand New");

        Assert.Equal(existing.ID, responseList[0].ID);
        Assert.Equal("Brand New", responseList[1].Name);
        Assert.Equal(2, _store.ArtistList.Count);
        Assert.Equal("Existing", _store.ArtistList.Single(x => x.ID == existing.ID).Name);
    }

    [Fact]
    public async Task Split_AFileThatAlreadyHasATarget_DoesNotGetItTwice()
    {
        EntityArtist combined = SeedArtist("A & B");
        EntityArtist a = SeedArtist("A");
        EntityMediaFile file = SeedFile("One", null, a, combined);

        await Split(combined, "A", "B");

        Assert.Equal(["A", "B"], ArtistNameList(file));
    }

    [Fact]
    public async Task Split_ANameThatIsTheItemItself_KeepsIt()
    {
        EntityArtist combined = SeedArtist("A & B");
        EntityMediaFile file = SeedFile("One", null, combined);

        List<ResponseArtist> responseList = await Split(combined, "a & b", "C");

        Assert.Contains(_store.ArtistList, x => x.ID == combined.ID);
        Assert.Equal(["A & B", "C"], ArtistNameList(file));
        Assert.Equal(combined.ID, responseList[0].ID);
        Assert.Empty(_publisher.GetList<NotificationArtistDeleted>());
    }

    [Fact]
    public async Task Split_TouchesAndPushesTheChangedFiles_TheItemsAndTheDeletionAfterTheCommit()
    {
        EntityArtist combined = SeedArtist("A & B");
        EntityMediaFile file = SeedFile("One", null, combined);
        EntityMediaFile unrelated = SeedFile("Two");
        int commitsAtPublish = -1;
        _publisher.OnPublish = _ => commitsAtPublish = _store.CommitCount;

        await Split(combined, "A", "B");

        Assert.Equal(_time.Now.UtcDateTime, Stored(file).TimeModified);
        Assert.Equal(Original, Stored(unrelated).TimeModified);
        Assert.Equal("A; B", Assert.Single(_publisher.GetList<NotificationMediaUpdated>()).MediaFile.Artist);
        Assert.Equal(2, _publisher.GetList<NotificationArtistUpdated>().Count);
        Assert.Equal(combined.ID, Assert.Single(_publisher.GetList<NotificationArtistDeleted>()).ID);
        Assert.Equal(1, _store.CommitCount);
        Assert.Equal(1, commitsAtPublish);
    }

    [Fact]
    public async Task Split_AlsoReplacesTheItemInEveryAlbumArtistList()
    {
        EntityArtist combined = SeedArtist("A & B");
        EntityAlbum album = SeedAlbum("Record", combined);
        EntityMediaFile file = SeedFile("Track", album);

        await Split(combined, "A", "B");

        EntityAlbum stored = Assert.Single(_store.AlbumList);
        Assert.Equal(["A", "B"], stored.ArtistLinkList.OrderBy(x => x.Position).Select(x => _store.ArtistList.Single(a => a.ID == x.ArtistID).Name).ToList());
        Assert.Equal(EntityAlbum.BuildIdentityKey("record", stored.ArtistLinkList.Select(x => x.ArtistID)), stored.IdentityKey);
        Assert.Equal("A; B", Assert.Single(_publisher.GetList<NotificationMediaUpdated>()).MediaFile.AlbumArtist);
        Assert.Equal(_time.Now.UtcDateTime, Stored(file).TimeModified);
    }

    [Theory]
    [InlineData]
    [InlineData("A")]
    [InlineData("A", "a")]
    [InlineData("A", " A ")]
    public async Task Split_FewerThanTwoDifferentNames_IsInvalid_AndChangesNothing(params string[] nameList)
    {
        EntityArtist combined = SeedArtist("A & B");

        await Assert.ThrowsAsync<InvalidMetadataOperationException>(() => Split(combined, nameList));

        Assert.Equal(0, _store.CommitCount);
        Assert.Single(_store.ArtistList);
    }

    [Fact]
    public async Task Split_ABlankName_IsRejected()
    {
        EntityArtist combined = SeedArtist("A & B");

        await Assert.ThrowsAsync<ArgumentException>(() => Split(combined, "A", "  "));

        Assert.Equal(0, _store.CommitCount);
    }

    [Fact]
    public async Task Split_NeverReusesAnotherUsersItem()
    {
        EntityArtist combined = SeedArtist("A & B");
        EntityArtist foreign = SeedArtist("A", Guid.CreateVersion7());
        SeedFile("One", null, combined);

        List<ResponseArtist> responseList = await Split(combined, "A", "B");

        Assert.NotEqual(foreign.ID, responseList[0].ID);
        Assert.Equal(3, _store.ArtistList.Count);
    }

    [Fact]
    public async Task Split_AnotherUsersItem_IsNotFound()
    {
        EntityArtist foreign = SeedArtist("Theirs", Guid.CreateVersion7());

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => Split(foreign, "A", "B"));
    }

    [Fact]
    public async Task Split_WhenTheCommitFails_PublishesNothing()
    {
        EntityArtist combined = SeedArtist("A & B");
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Split(combined, "A", "B"));

        Assert.Empty(_publisher.PublishedList);
    }
}
