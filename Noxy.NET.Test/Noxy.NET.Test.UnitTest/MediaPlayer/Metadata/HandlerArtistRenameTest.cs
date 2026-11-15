using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Media;
using Noxy.NET.MediaPlayer.Application.Handlers.Metadata;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerArtistRenameTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerArtistRename _handler;

    public HandlerArtistRenameTest()
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
        EntityMediaFile file = new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new string('a', 64), UserID = UserID, TimeModified = Original, AlbumID = album?.ID };
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

    private void SetAutoMerge(bool value)
    {
        _store.UserSettingsList.Add(new() { UserID = UserID, AutoMergeOnRename = value });
    }

    private EntityMediaFile Stored(EntityMediaFile file)
    {
        return _store.MediaFileList.Single(x => x.ID == file.ID);
    }

    private Task<ResponseArtist> Rename(EntityArtist artist, string name, Guid? userID = null)
    {
        return _handler.Handle(new CommandArtistRename(userID ?? UserID, artist.ID, name), Token).AsTask();
    }

    [Fact]
    public async Task Rename_ChangesTheNameAndKeyAndBumpsTheModifiedTime()
    {
        EntityArtist artist = SeedArtist("Old");
        SeedFile("One", null, artist);

        ResponseArtist response = await Rename(artist, "  New Name ");

        EntityArtist stored = Assert.Single(_store.ArtistList);
        Assert.Equal("New Name", stored.Name);
        Assert.Equal("new name", stored.NameKey);
        Assert.Equal(_time.Now.UtcDateTime, stored.TimeModified);
        Assert.Equal("New Name", response.Name);
        Assert.Equal(1, response.MediaFileCount);
    }

    [Fact]
    public async Task Rename_TouchesEveryFileThatHasTheArtistAndNoOtherFile()
    {
        EntityArtist artist = SeedArtist("Old");
        EntityArtist other = SeedArtist("Other");
        EntityMediaFile first = SeedFile("One", null, artist);
        EntityMediaFile second = SeedFile("Two", null, other, artist);
        EntityMediaFile unrelated = SeedFile("Three", null, other);

        await Rename(artist, "New");

        Assert.Equal(_time.Now.UtcDateTime, Stored(first).TimeModified);
        Assert.Equal(_time.Now.UtcDateTime, Stored(second).TimeModified);
        Assert.Equal(Original, Stored(unrelated).TimeModified);
        List<NotificationMediaUpdated> pushed = _publisher.GetList<NotificationMediaUpdated>();
        Assert.Equal(2, pushed.Count);
        Assert.Equal("New", pushed.Single(x => x.MediaFile.ID == first.ID).MediaFile.Artist);
        Assert.Equal("Other; New", pushed.Single(x => x.MediaFile.ID == second.ID).MediaFile.Artist);
    }

    [Fact]
    public async Task Rename_AlsoTouchesTheFilesOfAnAlbumThatListsTheArtistAsAlbumArtist()
    {
        EntityArtist artist = SeedArtist("Old");
        EntityAlbum album = SeedAlbum("Record", artist);
        EntityMediaFile albumFile = SeedFile("Track", album);

        await Rename(artist, "New");

        Assert.Equal(_time.Now.UtcDateTime, Stored(albumFile).TimeModified);
        NotificationMediaUpdated pushed = Assert.Single(_publisher.GetList<NotificationMediaUpdated>());
        Assert.Equal("New", pushed.MediaFile.AlbumArtist);
    }

    [Fact]
    public async Task Rename_CountsOnlyTheFilesThatCarryTheArtistItself()
    {
        EntityArtist artist = SeedArtist("Old");
        EntityAlbum album = SeedAlbum("Record", artist);
        SeedFile("Track", album);
        SeedFile("Own", null, artist);

        ResponseArtist response = await Rename(artist, "New");

        Assert.Equal(1, response.MediaFileCount);
    }

    [Fact]
    public async Task Rename_CommitsOnce_AndPushesTheArtistAfterTheCommit()
    {
        EntityArtist artist = SeedArtist("Old");
        SeedFile("One", null, artist);
        int commitsAtPublish = -1;
        _publisher.OnPublish = _ => commitsAtPublish = _store.CommitCount;

        await Rename(artist, "New");

        Assert.Equal(1, _store.CommitCount);
        Assert.Equal(1, commitsAtPublish);
        NotificationArtistUpdated pushed = Assert.Single(_publisher.GetList<NotificationArtistUpdated>());
        Assert.Equal(UserID, pushed.UserID);
        Assert.Equal("New", pushed.Artist.Name);
        Assert.Empty(_publisher.GetList<NotificationArtistDeleted>());
    }

    [Fact]
    public async Task Rename_ADifferentSpellingOfTheSameName_OnlyChangesHowItIsShown_AndIsNotACollision()
    {
        EntityArtist artist = SeedArtist("radiohead");
        EntityMediaFile file = SeedFile("One", null, artist);

        ResponseArtist response = await Rename(artist, "RADIOHEAD");

        Assert.Equal("RADIOHEAD", response.Name);
        Assert.Equal("radiohead", Assert.Single(_store.ArtistList).NameKey);
        Assert.Equal(_time.Now.UtcDateTime, Stored(file).TimeModified);
        Assert.Single(_publisher.GetList<NotificationMediaUpdated>());
    }

    [Fact]
    public async Task Rename_ToTheSameNameExactly_ChangesNothing()
    {
        EntityArtist artist = SeedArtist("Same");
        EntityMediaFile file = SeedFile("One", null, artist);

        ResponseArtist response = await Rename(artist, " Same ");

        Assert.Equal("Same", response.Name);
        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
        Assert.Equal(Original, Stored(file).TimeModified);
        Assert.Equal(Original, Assert.Single(_store.ArtistList).TimeModified);
    }

    [Fact]
    public async Task Rename_ToANameAnotherArtistHas_WithAutoMergeOff_IsAConflictNamingThatArtist()
    {
        EntityArtist artist = SeedArtist("A");
        EntityArtist other = SeedArtist("B");
        EntityMediaFile file = SeedFile("One", null, artist);

        NameAlreadyInUseException exception = await Assert.ThrowsAsync<NameAlreadyInUseException>(() => Rename(artist, "b"));

        Assert.Equal(other.ID, exception.ExistingID);
        Assert.Equal(2, _store.ArtistList.Count);
        Assert.Equal("A", _store.ArtistList.Single(x => x.ID == artist.ID).Name);
        Assert.Equal(Original, Stored(file).TimeModified);
        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Rename_ToANameAnotherArtistHas_WithAutoMergeOn_MergesIntoThatArtistWhichKeepsItsName()
    {
        SetAutoMerge(true);
        EntityArtist artist = SeedArtist("A");
        EntityArtist other = SeedArtist("B");
        EntityMediaFile onlyA = SeedFile("One", null, artist);
        EntityMediaFile both = SeedFile("Two", null, artist, other);
        EntityMediaFile onlyB = SeedFile("Three", null, other);

        ResponseArtist response = await Rename(artist, "b");

        Assert.Equal("B", response.Name);
        Assert.Equal(other.ID, response.ID);
        Assert.Equal(3, response.MediaFileCount);
        EntityArtist remaining = Assert.Single(_store.ArtistList);
        Assert.Equal(other.ID, remaining.ID);
        Assert.Equal("B", remaining.Name);
        Assert.Equal([other.ID], Stored(onlyA).ArtistLinkList.Select(x => x.ArtistID).ToList());
        Assert.Equal([other.ID], Stored(both).ArtistLinkList.Select(x => x.ArtistID).ToList());
        Assert.Equal(_time.Now.UtcDateTime, Stored(onlyA).TimeModified);
        Assert.Equal(_time.Now.UtcDateTime, Stored(both).TimeModified);
        Assert.Equal(Original, Stored(onlyB).TimeModified);
        Assert.Equal(artist.ID, Assert.Single(_publisher.GetList<NotificationArtistDeleted>()).ID);
        Assert.Equal(other.ID, Assert.Single(_publisher.GetList<NotificationArtistUpdated>()).Artist.ID);
        Assert.Equal(2, _publisher.GetList<NotificationMediaUpdated>().Count);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task Rename_AutoMergeOfAnArtistIntoAnother_ReplacesItInTheAlbumArtistListsToo()
    {
        SetAutoMerge(true);
        EntityArtist artist = SeedArtist("A");
        EntityArtist other = SeedArtist("B");
        EntityAlbum album = SeedAlbum("Record", artist);
        EntityMediaFile file = SeedFile("Track", album);

        await Rename(artist, "B");

        EntityAlbum stored = Assert.Single(_store.AlbumList);
        Assert.Equal([other.ID], stored.ArtistLinkList.Select(x => x.ArtistID).ToList());
        Assert.Equal(EntityAlbum.BuildIdentityKey("record", [other.ID]), stored.IdentityKey);
        Assert.Equal(_time.Now.UtcDateTime, Stored(file).TimeModified);
    }

    [Fact]
    public async Task Rename_ToTheNameOfAnotherUsersArtist_IsAllowed()
    {
        EntityArtist artist = SeedArtist("A");
        SeedArtist("B", Guid.CreateVersion7());

        ResponseArtist response = await Rename(artist, "B");

        Assert.Equal("B", response.Name);
        Assert.Equal(2, _store.ArtistList.Count);
    }

    [Fact]
    public async Task Rename_AnotherUsersArtist_ThrowsNotFound_AndChangesNothing()
    {
        EntityArtist foreign = SeedArtist("Theirs", Guid.CreateVersion7());

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => Rename(foreign, "Mine"));

        Assert.Equal("Theirs", Assert.Single(_store.ArtistList).Name);
        Assert.Equal(0, _store.CommitCount);
    }

    [Fact]
    public async Task Rename_AnUnknownArtist_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => _handler.Handle(new CommandArtistRename(UserID, Guid.CreateVersion7(), "x"), Token).AsTask());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Rename_ToABlankName_IsRejected(string name)
    {
        EntityArtist artist = SeedArtist("A");

        await Assert.ThrowsAsync<ArgumentException>(() => Rename(artist, name));

        Assert.Equal("A", Assert.Single(_store.ArtistList).Name);
    }

    [Fact]
    public async Task Rename_WhenTheCommitFails_PublishesNothing()
    {
        EntityArtist artist = SeedArtist("A");
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Rename(artist, "B"));

        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Rename_ChangesTheLibraryETagOfTheAffectedFiles()
    {
        EntityArtist artist = SeedArtist("Old");
        SeedFile("One", null, artist);
        HandlerMediaLibrary library = new(new FakeUnitOfWorkFactory(_store), new ETagService());
        string before = (await library.Handle(new(UserID, null), Token)).ETag;

        await Rename(artist, "New");

        Assert.NotEqual(before, (await library.Handle(new(UserID, null), Token)).ETag);
    }
}
