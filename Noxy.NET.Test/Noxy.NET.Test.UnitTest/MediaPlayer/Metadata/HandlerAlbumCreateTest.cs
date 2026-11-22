using Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Album;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerAlbumCreateTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly HandlerAlbumCreate _handler;

    public HandlerAlbumCreateTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), new NameKeyService(), new MetadataChangeService(), _publisher, new FakeTimeProvider());
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityArtist SeedArtist(string name, Guid? userID = null)
    {
        EntityArtist artist = new() { UserID = userID ?? UserID, Name = name, NameKey = name.ToLowerInvariant() };
        _store.ArtistList.Add(artist);
        return artist;
    }

    private Task<ResponseAlbum> Create(string title, params Guid[] artistIDList)
    {
        return _handler.Handle(new(UserID, title, [.. artistIDList]), Token).AsTask();
    }

    [Fact]
    public async Task Create_StoresTheTrimmedTitleItsKeyAndTheArtistsInOrder()
    {
        EntityArtist first = SeedArtist("A");
        EntityArtist second = SeedArtist("B");

        ResponseAlbum response = await Create("  Greatest   Hits ", second.ID, first.ID);

        EntityAlbum album = Assert.Single(_store.AlbumList);
        Assert.Equal("Greatest   Hits", album.Title);
        Assert.Equal("greatest hits", album.TitleKey);
        Assert.Equal(EntityAlbum.BuildIdentityKey("greatest hits", [first.ID, second.ID]), album.IdentityKey);
        Assert.Equal([second.ID, first.ID], album.ArtistLinkList.OrderBy(x => x.Position).Select(x => x.ArtistID).ToList());
        Assert.Equal([second.ID, first.ID], response.ArtistIDList);
        Assert.Equal(0, response.MediaFileCount);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task Create_PushesTheAlbumToTheOwnerAfterTheCommit()
    {
        int commitsAtPublish = -1;
        _publisher.OnPublish = _ => commitsAtPublish = _store.CommitCount;

        ResponseAlbum response = await Create("Record");

        NotificationAlbumUpdated pushed = Assert.Single(_publisher.GetList<NotificationAlbumUpdated>());
        Assert.Equal(UserID, pushed.UserID);
        Assert.Equal(response.ID, pushed.Album.ID);
        Assert.Equal(1, commitsAtPublish);
    }

    [Fact]
    public async Task Create_ARepeatedArtistID_IsLinkedOnce()
    {
        EntityArtist artist = SeedArtist("A");

        ResponseAlbum response = await Create("Record", artist.ID, artist.ID);

        Assert.Equal([artist.ID], response.ArtistIDList);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_ABlankTitle_IsRejected(string title)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Create(title));

        Assert.Empty(_store.AlbumList);
    }

    [Fact]
    public async Task Create_ATitleOver255Characters_IsRejected()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Create(new('a', 256)));
    }

    [Fact]
    public async Task Create_TheSameTitleAndArtists_IsAConflictNamingTheExistingAlbum()
    {
        EntityArtist artist = SeedArtist("A");
        await Create("Record", artist.ID);

        NameAlreadyInUseException exception = await Assert.ThrowsAsync<NameAlreadyInUseException>(() => Create(" RECORD ", artist.ID));

        Assert.Equal(Assert.Single(_store.AlbumList).ID, exception.ExistingID);
    }

    [Fact]
    public async Task Create_TheSameTitleWithOtherArtists_IsAnotherAlbum()
    {
        EntityArtist first = SeedArtist("A");
        EntityArtist second = SeedArtist("B");
        await Create("Hits", first.ID);

        await Create("Hits", second.ID);
        await Create("Hits");

        Assert.Equal(3, _store.AlbumList.Count);
    }

    [Fact]
    public async Task Create_AnUnknownOrForeignArtist_ThrowsNotFound_AndSavesNothing()
    {
        EntityArtist foreign = SeedArtist("Theirs", Guid.CreateVersion7());

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => Create("Record", Guid.CreateVersion7()));
        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => Create("Record", foreign.ID));

        Assert.Empty(_store.AlbumList);
        Assert.Equal(0, _store.CommitCount);
    }

    [Fact]
    public async Task Create_TheSameTitleForAnotherUser_IsAllowed()
    {
        await Create("Record");
        await _handler.Handle(new(Guid.CreateVersion7(), "Record", []), Token);

        Assert.Equal(2, _store.AlbumList.Count);
    }
}
