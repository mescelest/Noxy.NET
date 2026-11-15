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

public class HandlerAlbumUpdateTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerAlbumUpdate _handler;

    public HandlerAlbumUpdateTest()
    {
        _time.Now = new(2027, 5, 6, 7, 8, 9, TimeSpan.Zero);
        _handler = new(new FakeUnitOfWorkFactory(_store), new NameKeyService(), new MetadataChangeService(), _publisher, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityArtist SeedArtist(string name)
    {
        EntityArtist artist = new() { UserID = UserID, Name = name, NameKey = name.ToLowerInvariant(), TimeModified = Original };
        _store.ArtistList.Add(artist);
        return artist;
    }

    private EntityAlbum SeedAlbum(string title, params EntityArtist[] artistList)
    {
        EntityAlbum album = new() { UserID = UserID, Title = title, TitleKey = title.ToLowerInvariant(), IdentityKey = EntityAlbum.BuildIdentityKey(title.ToLowerInvariant(), artistList.Select(x => x.ID)), TimeModified = Original };
        album.ArtistLinkList = [.. artistList.Select((x, index) => new EntityAlbumArtist { AlbumID = album.ID, ArtistID = x.ID, Position = index })];
        _store.AlbumList.Add(album);
        return album;
    }

    private EntityMediaFile SeedFile(string title, EntityAlbum? album)
    {
        EntityMediaFile file = new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new string('a', 64), UserID = UserID, TimeModified = Original, AlbumID = album?.ID };
        file.Metadata = new() { Title = title, MediaFileID = file.ID };
        _store.MediaFileList.Add(file);
        return file;
    }

    private EntityMediaFile Stored(EntityMediaFile file)
    {
        return _store.MediaFileList.Single(x => x.ID == file.ID);
    }

    private Task<ResponseAlbum> Update(EntityAlbum album, CommandAlbumUpdate changes)
    {
        return _handler.Handle(changes with { UserID = UserID, ID = album.ID }, Token).AsTask();
    }

    private static CommandAlbumUpdate Changes()
    {
        return new(Guid.Empty, Guid.Empty);
    }

    [Fact]
    public async Task Update_ATitle_ChangesTheTitleKeyAndIdentityAndTouchesItsFiles()
    {
        EntityArtist artist = SeedArtist("A");
        EntityAlbum album = SeedAlbum("Old", artist);
        EntityMediaFile file = SeedFile("One", album);
        EntityMediaFile unrelated = SeedFile("Two", null);

        ResponseAlbum response = await Update(album, Changes() with { Title = new("  New Title ") });

        EntityAlbum stored = Assert.Single(_store.AlbumList);
        Assert.Equal("New Title", stored.Title);
        Assert.Equal("new title", stored.TitleKey);
        Assert.Equal(EntityAlbum.BuildIdentityKey("new title", [artist.ID]), stored.IdentityKey);
        Assert.Equal(_time.Now.UtcDateTime, stored.TimeModified);
        Assert.Equal("New Title", response.Title);
        Assert.Equal(1, response.MediaFileCount);
        Assert.Equal(_time.Now.UtcDateTime, Stored(file).TimeModified);
        Assert.Equal(Original, Stored(unrelated).TimeModified);
        Assert.Equal("New Title", Assert.Single(_publisher.GetList<NotificationMediaUpdated>()).MediaFile.Album);
        Assert.Equal(response.ID, Assert.Single(_publisher.GetList<NotificationAlbumUpdated>()).Album.ID);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task Update_TheArtists_ReplacesTheListRecomputesTheIdentityAndChangesTheAlbumArtistTextOfItsFiles()
    {
        EntityArtist first = SeedArtist("A");
        EntityArtist second = SeedArtist("B");
        EntityAlbum album = SeedAlbum("Record", first);
        SeedFile("One", album);

        ResponseAlbum response = await Update(album, Changes() with { ArtistIDList = new([second.ID, first.ID]) });

        EntityAlbum stored = Assert.Single(_store.AlbumList);
        Assert.Equal([second.ID, first.ID], stored.ArtistLinkList.OrderBy(x => x.Position).Select(x => x.ArtistID).ToList());
        Assert.Equal(EntityAlbum.BuildIdentityKey("record", [first.ID, second.ID]), stored.IdentityKey);
        Assert.Equal([second.ID, first.ID], response.ArtistIDList);
        Assert.Equal("B; A", Assert.Single(_publisher.GetList<NotificationMediaUpdated>()).MediaFile.AlbumArtist);
    }

    [Fact]
    public async Task Update_AnEmptyOrNullArtistList_ClearsTheArtists()
    {
        EntityArtist artist = SeedArtist("A");
        EntityAlbum album = SeedAlbum("Record", artist);

        ResponseAlbum response = await Update(album, Changes() with { ArtistIDList = new(null) });

        Assert.Empty(response.ArtistIDList);
        Assert.Empty(Assert.Single(_store.AlbumList).ArtistLinkList);
        Assert.Equal("record|", Assert.Single(_store.AlbumList).IdentityKey);
    }

    [Fact]
    public async Task Update_NothingChanged_DoesNotCommitBumpOrPush()
    {
        EntityArtist artist = SeedArtist("A");
        EntityAlbum album = SeedAlbum("Record", artist);
        SeedFile("One", album);

        await Update(album, Changes() with { Title = new("Record"), ArtistIDList = new([artist.ID]) });
        await Update(album, Changes());

        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
        Assert.Equal(Original, Assert.Single(_store.AlbumList).TimeModified);
    }

    [Fact]
    public async Task Update_ADifferentSpellingOfTheTitle_IsNotACollisionWithItself()
    {
        EntityAlbum album = SeedAlbum("record");

        ResponseAlbum response = await Update(album, Changes() with { Title = new("RECORD") });

        Assert.Equal("RECORD", response.Title);
    }

    [Fact]
    public async Task Update_ToTheTitleAndArtistsOfAnotherAlbum_WithAutoMergeOff_IsAConflictNamingIt()
    {
        EntityAlbum album = SeedAlbum("A");
        EntityAlbum other = SeedAlbum("B");
        SeedFile("One", album);

        NameAlreadyInUseException exception = await Assert.ThrowsAsync<NameAlreadyInUseException>(() => Update(album, Changes() with { Title = new("b") }));

        Assert.Equal(other.ID, exception.ExistingID);
        Assert.Equal(2, _store.AlbumList.Count);
        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Update_ToTheTitleAndArtistsOfAnotherAlbum_WithAutoMergeOn_MergesIntoItWhichKeepsItsTitle()
    {
        _store.UserSettingsList.Add(new() { UserID = UserID, AutoMergeOnRename = true });
        EntityAlbum album = SeedAlbum("A");
        EntityAlbum other = SeedAlbum("B");
        EntityMediaFile moving = SeedFile("One", album);
        EntityMediaFile staying = SeedFile("Two", other);

        ResponseAlbum response = await Update(album, Changes() with { Title = new("b") });

        Assert.Equal(other.ID, response.ID);
        Assert.Equal("B", response.Title);
        Assert.Equal(2, response.MediaFileCount);
        Assert.Equal(other.ID, Assert.Single(_store.AlbumList).ID);
        Assert.Equal(other.ID, Stored(moving).AlbumID);
        Assert.Equal(_time.Now.UtcDateTime, Stored(moving).TimeModified);
        Assert.Equal(Original, Stored(staying).TimeModified);
        Assert.Equal(album.ID, Assert.Single(_publisher.GetList<NotificationAlbumDeleted>()).ID);
        Assert.Equal(other.ID, Assert.Single(_publisher.GetList<NotificationAlbumUpdated>()).Album.ID);
    }

    [Fact]
    public async Task Update_AnotherUsersAlbum_ThrowsNotFound()
    {
        EntityAlbum foreign = new() { UserID = Guid.CreateVersion7(), Title = "Theirs", TitleKey = "theirs", IdentityKey = "theirs|" };
        _store.AlbumList.Add(foreign);

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => Update(foreign, Changes() with { Title = new("Mine") }));
    }

    [Fact]
    public async Task Update_AnUnknownArtist_ThrowsNotFound_AndChangesNothing()
    {
        EntityAlbum album = SeedAlbum("A");

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => Update(album, Changes() with { ArtistIDList = new([Guid.CreateVersion7()]) }));

        Assert.Equal(0, _store.CommitCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Update_ABlankOrNullTitle_IsRejected(string? title)
    {
        EntityAlbum album = SeedAlbum("A");

        await Assert.ThrowsAsync<ArgumentException>(() => Update(album, Changes() with { Title = new(title) }));

        Assert.Equal("A", Assert.Single(_store.AlbumList).Title);
    }

    [Fact]
    public async Task Update_WhenTheCommitFails_PublishesNothing()
    {
        EntityAlbum album = SeedAlbum("A");
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Update(album, Changes() with { Title = new("B") }));

        Assert.Empty(_publisher.PublishedList);
    }
}
