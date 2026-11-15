using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Artist;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerArtistDeleteTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerArtistDelete _handler;

    public HandlerArtistDeleteTest()
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

    private EntityMediaFile Stored(EntityMediaFile file)
    {
        return _store.MediaFileList.Single(x => x.ID == file.ID);
    }

    private Task Delete(EntityArtist artist, Guid? userID = null)
    {
        return _handler.Handle(new CommandArtistDelete(userID ?? UserID, artist.ID), Token).AsTask();
    }

    [Fact]
    public async Task Delete_RemovesTheArtistButKeepsItsFiles()
    {
        EntityArtist artist = SeedArtist("A");
        SeedFile("One", null, artist);
        SeedFile("Two", null, artist);

        await Delete(artist);

        Assert.Empty(_store.ArtistList);
        Assert.Equal(2, _store.MediaFileList.Count);
        Assert.All(_store.MediaFileList, x => Assert.Empty(x.ArtistLinkList));
    }

    [Fact]
    public async Task Delete_TouchesAndPushesExactlyTheFilesThatHadTheArtist()
    {
        EntityArtist artist = SeedArtist("A");
        EntityArtist other = SeedArtist("B");
        EntityMediaFile had = SeedFile("One", null, other, artist);
        EntityMediaFile unrelated = SeedFile("Two", null, other);

        await Delete(artist);

        Assert.Equal(_time.Now.UtcDateTime, Stored(had).TimeModified);
        Assert.Equal(Original, Stored(unrelated).TimeModified);
        NotificationMediaUpdated pushed = Assert.Single(_publisher.GetList<NotificationMediaUpdated>());
        Assert.Equal(had.ID, pushed.MediaFile.ID);
        Assert.Equal("B", pushed.MediaFile.Artist);
        Assert.Equal([other.ID], pushed.MediaFile.ArtistIDList);
    }

    [Fact]
    public async Task Delete_PublishesTheDeletedArtistAfterTheCommit_AndCommitsOnce()
    {
        EntityArtist artist = SeedArtist("A");
        SeedFile("One", null, artist);
        int commitsAtPublish = -1;
        _publisher.OnPublish = _ => commitsAtPublish = _store.CommitCount;

        await Delete(artist);

        Assert.Equal(1, _store.CommitCount);
        Assert.Equal(1, commitsAtPublish);
        NotificationArtistDeleted pushed = Assert.Single(_publisher.GetList<NotificationArtistDeleted>());
        Assert.Equal(UserID, pushed.UserID);
        Assert.Equal(artist.ID, pushed.ID);
        Assert.Empty(_publisher.GetList<NotificationArtistUpdated>());
    }

    [Fact]
    public async Task Delete_AnArtistOfNoFile_StillWorks()
    {
        EntityArtist artist = SeedArtist("A");

        await Delete(artist);

        Assert.Empty(_store.ArtistList);
        Assert.Single(_publisher.GetList<NotificationArtistDeleted>());
        Assert.Empty(_publisher.GetList<NotificationMediaUpdated>());
    }

    [Fact]
    public async Task Delete_AnAlbumArtist_LeavesTheAlbumWithoutItAndRecomputesItsIdentityKey()
    {
        EntityArtist artist = SeedArtist("A");
        EntityArtist other = SeedArtist("B");
        EntityAlbum album = SeedAlbum("Record", artist, other);
        EntityMediaFile file = SeedFile("Track", album);

        await Delete(artist);

        EntityAlbum stored = Assert.Single(_store.AlbumList);
        Assert.Equal([other.ID], stored.ArtistLinkList.Select(x => x.ArtistID).ToList());
        Assert.Equal(EntityAlbum.BuildIdentityKey("record", [other.ID]), stored.IdentityKey);
        Assert.Equal(_time.Now.UtcDateTime, stored.TimeModified);
        Assert.Equal(_time.Now.UtcDateTime, Stored(file).TimeModified);
        Assert.Equal("B", Assert.Single(_publisher.GetList<NotificationMediaUpdated>()).MediaFile.AlbumArtist);
        Assert.Equal(album.ID, Assert.Single(_publisher.GetList<NotificationAlbumUpdated>()).Album.ID);
    }

    [Fact]
    public async Task Delete_AnAlbumArtist_WhoseRemovalMakesTwoAlbumsTheSame_MergesTheAlbums()
    {
        EntityArtist artist = SeedArtist("A");
        EntityAlbum withArtist = SeedAlbum("Hits", artist);
        EntityAlbum withoutArtist = SeedAlbum("Hits");
        EntityMediaFile moving = SeedFile("One", withArtist);
        EntityMediaFile staying = SeedFile("Two", withoutArtist);

        await Delete(artist);

        EntityAlbum remaining = Assert.Single(_store.AlbumList);
        Assert.Equal(withoutArtist.ID, remaining.ID);
        Assert.Equal(withoutArtist.ID, Stored(moving).AlbumID);
        Assert.Equal(withoutArtist.ID, Stored(staying).AlbumID);
        Assert.Equal(_time.Now.UtcDateTime, Stored(moving).TimeModified);
        Assert.Equal(Original, Stored(staying).TimeModified);
        Assert.Equal(withArtist.ID, Assert.Single(_publisher.GetList<NotificationAlbumDeleted>()).ID);
        Assert.Equal(2, Assert.Single(_publisher.GetList<NotificationAlbumUpdated>()).Album.MediaFileCount);
    }

    [Fact]
    public async Task Delete_AnotherUsersArtist_ThrowsNotFound_AndChangesNothing()
    {
        EntityArtist foreign = SeedArtist("Theirs", Guid.CreateVersion7());

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => Delete(foreign));

        Assert.Single(_store.ArtistList);
        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Delete_AnUnknownArtist_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => _handler.Handle(new CommandArtistDelete(UserID, Guid.CreateVersion7()), Token).AsTask());
    }

    [Fact]
    public async Task Delete_WhenTheCommitFails_PublishesNothing()
    {
        EntityArtist artist = SeedArtist("A");
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Delete(artist));

        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Delete_PassesTheCancellationTokenToThePublish()
    {
        EntityArtist artist = SeedArtist("A");
        using CancellationTokenSource source = new();

        await _handler.Handle(new CommandArtistDelete(UserID, artist.ID), source.Token);

        Assert.All(_publisher.TokenList, x => Assert.Equal(source.Token, x));
    }
}
