using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Persistence;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class MetadataRepositoryTest : IDisposable
{
    private readonly DatabaseFixture _fixture = new();

    public void Dispose()
    {
        _fixture.Dispose();
    }

    private async Task<EntityUser> CreateUser(string email = "user@example.com")
    {
        EntityUser user = new() { Email = email };

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Authentication.AddUser(user);
        await uow.Commit(TestContext.Current.CancellationToken);

        return user;
    }

    private async Task<EntityMediaFile> CreateMediaFile(Guid userID)
    {
        EntityMediaFile file = new()
        {
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = 1234,
            ContentHash = new('a', 64),
            UserID = userID
        };

        file.Metadata = new() { Title = "Media", MediaFileID = file.ID };

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Media.AddMediaFile(file);
        await uow.Commit(TestContext.Current.CancellationToken);

        return file;
    }

    private static EntityArtist CreateArtist(Guid userID, string name = "Artist", string? key = null)
    {
        return new() { UserID = userID, Name = name, NameKey = key ?? name.ToLowerInvariant() };
    }

    private static EntityGenre CreateGenre(Guid userID, string name = "Genre", string? key = null)
    {
        return new() { UserID = userID, Name = name, NameKey = key ?? name.ToLowerInvariant() };
    }

    private static EntityAlbum CreateAlbum(Guid userID, string title = "Album", string? identityKey = null)
    {
        return new() { UserID = userID, Title = title, TitleKey = title.ToLowerInvariant(), IdentityKey = identityKey ?? $"{title.ToLowerInvariant()}|" };
    }

    private async Task Add(EntityArtist artist)
    {
        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Metadata.AddArtist(artist);
        await uow.Commit(TestContext.Current.CancellationToken);
    }

    private async Task Add(EntityGenre genre)
    {
        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Metadata.AddGenre(genre);
        await uow.Commit(TestContext.Current.CancellationToken);
    }

    private async Task Add(EntityAlbum album)
    {
        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Metadata.AddAlbum(album);
        await uow.Commit(TestContext.Current.CancellationToken);
    }

    private async Task LinkArtist(EntityMediaFile file, EntityArtist artist, int position)
    {
        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Metadata.AddMediaFileArtist(new() { MediaFileID = file.ID, ArtistID = artist.ID, Position = position });
        await uow.Commit(TestContext.Current.CancellationToken);
    }

    private async Task LinkGenre(EntityMediaFile file, EntityGenre genre, int position)
    {
        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Metadata.AddMediaFileGenre(new() { MediaFileID = file.ID, GenreID = genre.ID, Position = position });
        await uow.Commit(TestContext.Current.CancellationToken);
    }

    private async Task SetAlbum(EntityMediaFile file, EntityAlbum album)
    {
        file.AlbumID = album.ID;

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Media.UpdateMediaFile(file);
        await uow.Commit(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AddArtist_ThenFind_ReturnsIt()
    {
        EntityUser user = await CreateUser();
        EntityArtist artist = CreateArtist(user.ID, "Radiohead");
        await Add(artist);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        EntityArtist? found = await uow.Metadata.FindArtist(artist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal("Radiohead", found.Name);
        Assert.Equal("radiohead", found.NameKey);
        Assert.Equal(user.ID, found.UserID);
    }

    [Fact]
    public async Task AddGenre_ThenFind_ReturnsIt()
    {
        EntityUser user = await CreateUser();
        EntityGenre genre = CreateGenre(user.ID, "Rock");
        await Add(genre);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        EntityGenre? found = await uow.Metadata.FindGenre(genre.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal("Rock", found.Name);
        Assert.Equal("rock", found.NameKey);
    }

    [Fact]
    public async Task AddAlbum_WithArtists_ThenFind_ReturnsItWithTheArtistsInOrder()
    {
        EntityUser user = await CreateUser();
        EntityArtist first = CreateArtist(user.ID, "A");
        EntityArtist second = CreateArtist(user.ID, "B");
        await Add(first);
        await Add(second);
        EntityAlbum album = CreateAlbum(user.ID, "Together");
        album.ArtistLinkList = [new() { AlbumID = album.ID, ArtistID = second.ID, Position = 0 }, new() { AlbumID = album.ID, ArtistID = first.ID, Position = 1 }];
        await Add(album);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        EntityAlbum? found = await uow.Metadata.FindAlbum(album.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal("Together", found.Title);
        Assert.Equal([second.ID, first.ID], found.ArtistLinkList.Select(x => x.ArtistID).ToList());
    }

    [Fact]
    public async Task FindArtistByKey_FindsOnlyTheOwnersItem()
    {
        EntityUser owner = await CreateUser("owner@example.com");
        EntityUser other = await CreateUser("other@example.com");
        EntityArtist artist = CreateArtist(owner.ID, "Radiohead");
        await Add(artist);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.NotNull(await uow.Metadata.FindArtistByKey(owner.ID, "radiohead", TestContext.Current.CancellationToken));
        Assert.Null(await uow.Metadata.FindArtistByKey(other.ID, "radiohead", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task FindGenreByKey_FindsOnlyTheOwnersItem()
    {
        EntityUser owner = await CreateUser("owner@example.com");
        EntityUser other = await CreateUser("other@example.com");
        await Add(CreateGenre(owner.ID, "Rock"));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.NotNull(await uow.Metadata.FindGenreByKey(owner.ID, "rock", TestContext.Current.CancellationToken));
        Assert.Null(await uow.Metadata.FindGenreByKey(other.ID, "rock", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task FindAlbumByIdentityKey_FindsOnlyTheOwnersAlbum()
    {
        EntityUser owner = await CreateUser("owner@example.com");
        EntityUser other = await CreateUser("other@example.com");
        await Add(CreateAlbum(owner.ID, "Greatest Hits", "greatest hits|"));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.NotNull(await uow.Metadata.FindAlbumByIdentityKey(owner.ID, "greatest hits|", TestContext.Current.CancellationToken));
        Assert.Null(await uow.Metadata.FindAlbumByIdentityKey(other.ID, "greatest hits|", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task FindByKey_SeesAnItemAddedInTheSameUnitOfWorkBeforeTheCommit()
    {
        EntityUser user = await CreateUser();
        EntityArtist artist = CreateArtist(user.ID, "Radiohead");
        EntityGenre genre = CreateGenre(user.ID, "Rock");
        EntityAlbum album = CreateAlbum(user.ID, "OK Computer", "ok computer|");

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Metadata.AddArtist(artist);
        uow.Metadata.AddGenre(genre);
        uow.Metadata.AddAlbum(album);

        Assert.Equal(artist.ID, (await uow.Metadata.FindArtistByKey(user.ID, "radiohead", TestContext.Current.CancellationToken))?.ID);
        Assert.Equal(genre.ID, (await uow.Metadata.FindGenreByKey(user.ID, "rock", TestContext.Current.CancellationToken))?.ID);
        Assert.Equal(album.ID, (await uow.Metadata.FindAlbumByIdentityKey(user.ID, "ok computer|", TestContext.Current.CancellationToken))?.ID);
    }

    [Fact]
    public async Task FindByKey_DoesNotSeeAnotherUnitOfWorksPendingItem()
    {
        EntityUser user = await CreateUser();

        await using IUnitOfWork adding = _fixture.UoWFactory.Create();
        adding.Metadata.AddArtist(CreateArtist(user.ID, "Radiohead"));

        await using IUnitOfWork other = _fixture.UoWFactory.Create();

        Assert.Null(await other.Metadata.FindArtistByKey(user.ID, "radiohead", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task FindByKey_DoesNotSeeAPendingItemOfAnotherUser()
    {
        EntityUser owner = await CreateUser("owner@example.com");
        EntityUser other = await CreateUser("other@example.com");

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Metadata.AddArtist(CreateArtist(owner.ID, "Radiohead"));

        Assert.Null(await uow.Metadata.FindArtistByKey(other.ID, "radiohead", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Find_AnotherUsersItem_ReturnsNull()
    {
        EntityUser owner = await CreateUser("owner@example.com");
        EntityUser other = await CreateUser("other@example.com");
        EntityArtist artist = CreateArtist(owner.ID);
        EntityGenre genre = CreateGenre(owner.ID);
        EntityAlbum album = CreateAlbum(owner.ID);
        await Add(artist);
        await Add(genre);
        await Add(album);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Null(await uow.Metadata.FindArtist(artist.ID, other.ID, TestContext.Current.CancellationToken));
        Assert.Null(await uow.Metadata.FindGenre(genre.ID, other.ID, TestContext.Current.CancellationToken));
        Assert.Null(await uow.Metadata.FindAlbum(album.ID, other.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AddArtist_SameKeyForTwoUsers_IsAllowed()
    {
        EntityUser first = await CreateUser("first@example.com");
        EntityUser second = await CreateUser("second@example.com");

        await Add(CreateArtist(first.ID, "Radiohead"));
        await Add(CreateArtist(second.ID, "Radiohead"));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        Assert.Single(await uow.Metadata.GetArtistSummaryList(first.ID, TestContext.Current.CancellationToken));
        Assert.Single(await uow.Metadata.GetArtistSummaryList(second.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AddArtist_SecondWithTheSameKeyForOneUser_ThrowsMetadataKeyConflict()
    {
        EntityUser user = await CreateUser();
        await Add(CreateArtist(user.ID, "Radiohead"));

        await Assert.ThrowsAsync<MetadataKeyConflictException>(() => Add(CreateArtist(user.ID, "RADIOHEAD", "radiohead")));
    }

    [Fact]
    public async Task AddGenre_SecondWithTheSameKeyForOneUser_ThrowsMetadataKeyConflict()
    {
        EntityUser user = await CreateUser();
        await Add(CreateGenre(user.ID, "Rock"));

        await Assert.ThrowsAsync<MetadataKeyConflictException>(() => Add(CreateGenre(user.ID, "ROCK", "rock")));
    }

    [Fact]
    public async Task AddAlbum_SecondWithTheSameIdentityKeyForOneUser_ThrowsMetadataKeyConflict()
    {
        EntityUser user = await CreateUser();
        await Add(CreateAlbum(user.ID, "Greatest Hits", "greatest hits|"));

        await Assert.ThrowsAsync<MetadataKeyConflictException>(() => Add(CreateAlbum(user.ID, "Greatest hits", "greatest hits|")));
    }

    [Fact]
    public async Task Links_StoreTheirOrder()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile file = await CreateMediaFile(user.ID);
        EntityArtist first = CreateArtist(user.ID, "A");
        EntityArtist second = CreateArtist(user.ID, "B");
        EntityGenre genreFirst = CreateGenre(user.ID, "G1");
        EntityGenre genreSecond = CreateGenre(user.ID, "G2");
        await Add(first);
        await Add(second);
        await Add(genreFirst);
        await Add(genreSecond);
        await LinkArtist(file, second, 0);
        await LinkArtist(file, first, 1);
        await LinkGenre(file, genreSecond, 0);
        await LinkGenre(file, genreFirst, 1);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<EntityMediaFile> list = await uow.Metadata.GetMediaFileListByArtist(user.ID, first.ID, TestContext.Current.CancellationToken);

        EntityMediaFile loaded = Assert.Single(list);
        Assert.Equal(["B", "A"], loaded.ArtistLinkList.Select(x => x.Artist?.Name).ToList());
        Assert.Equal(["G2", "G1"], loaded.GenreLinkList.Select(x => x.Genre?.Name).ToList());
    }

    [Fact]
    public async Task GetMediaFileListByArtist_ReturnsOnlyTheOwnersFilesWithThatArtist()
    {
        EntityUser owner = await CreateUser("owner@example.com");
        EntityUser other = await CreateUser("other@example.com");
        EntityMediaFile linked = await CreateMediaFile(owner.ID);
        await CreateMediaFile(owner.ID);
        EntityArtist artist = CreateArtist(owner.ID);
        await Add(artist);
        await LinkArtist(linked, artist, 0);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        List<EntityMediaFile> ownerList = await uow.Metadata.GetMediaFileListByArtist(owner.ID, artist.ID, TestContext.Current.CancellationToken);
        List<EntityMediaFile> otherList = await uow.Metadata.GetMediaFileListByArtist(other.ID, artist.ID, TestContext.Current.CancellationToken);

        Assert.Equal([linked.ID], ownerList.Select(x => x.ID).ToList());
        Assert.Empty(otherList);
    }

    [Fact]
    public async Task GetMediaFileListByGenreAndAlbum_ReturnTheLinkedFiles()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile file = await CreateMediaFile(user.ID);
        EntityGenre genre = CreateGenre(user.ID);
        EntityAlbum album = CreateAlbum(user.ID);
        await Add(genre);
        await Add(album);
        await LinkGenre(file, genre, 0);
        await SetAlbum(file, album);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        List<EntityMediaFile> genreList = await uow.Metadata.GetMediaFileListByGenre(user.ID, genre.ID, TestContext.Current.CancellationToken);
        List<EntityMediaFile> albumList = await uow.Metadata.GetMediaFileListByAlbum(user.ID, album.ID, TestContext.Current.CancellationToken);

        Assert.Equal([file.ID], genreList.Select(x => x.ID).ToList());
        Assert.Equal([file.ID], albumList.Select(x => x.ID).ToList());
        Assert.Equal(album.ID, albumList[0].AlbumID);
    }

    [Fact]
    public async Task RemoveArtist_RemovesItsLinksButNotTheFiles()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile file = await CreateMediaFile(user.ID);
        EntityArtist artist = CreateArtist(user.ID);
        await Add(artist);
        await LinkArtist(file, artist, 0);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Metadata.RemoveArtist(artist);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using DataContext context = _fixture.CreateContext();
        Assert.Equal(0, await context.MediaFileArtist.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await context.MediaFile.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RemoveGenre_RemovesItsLinksButNotTheFiles()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile file = await CreateMediaFile(user.ID);
        EntityGenre genre = CreateGenre(user.ID);
        await Add(genre);
        await LinkGenre(file, genre, 0);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Metadata.RemoveGenre(genre);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using DataContext context = _fixture.CreateContext();
        Assert.Equal(0, await context.MediaFileGenre.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await context.MediaFile.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RemoveAlbum_ClearsTheAlbumOfItsFilesButKeepsThem()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile file = await CreateMediaFile(user.ID);
        EntityAlbum album = CreateAlbum(user.ID);
        await Add(album);
        await SetAlbum(file, album);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Metadata.RemoveAlbum(album);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using DataContext context = _fixture.CreateContext();
        Assert.Null((await context.MediaFile.SingleAsync(TestContext.Current.CancellationToken)).AlbumID);
        Assert.Equal(0, await context.Album.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RemoveArtist_RemovesItFromAlbumArtistLists()
    {
        EntityUser user = await CreateUser();
        EntityArtist artist = CreateArtist(user.ID);
        await Add(artist);
        EntityAlbum album = CreateAlbum(user.ID);
        album.ArtistLinkList = [new() { AlbumID = album.ID, ArtistID = artist.ID, Position = 0 }];
        await Add(album);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Metadata.RemoveArtist(artist);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using DataContext context = _fixture.CreateContext();
        Assert.Equal(0, await context.AlbumArtist.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await context.Album.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RemoveMediaFile_RemovesItsLinks()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile file = await CreateMediaFile(user.ID);
        EntityArtist artist = CreateArtist(user.ID);
        EntityGenre genre = CreateGenre(user.ID);
        await Add(artist);
        await Add(genre);
        await LinkArtist(file, artist, 0);
        await LinkGenre(file, genre, 0);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Media.RemoveMediaFile(file);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using DataContext context = _fixture.CreateContext();
        Assert.Equal(0, await context.MediaFileArtist.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, await context.MediaFileGenre.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await context.Artist.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await context.Genre.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetArtistSummaryList_CountsFilesPerArtist()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile fileOne = await CreateMediaFile(user.ID);
        EntityMediaFile fileTwo = await CreateMediaFile(user.ID);
        EntityArtist busy = CreateArtist(user.ID, "Busy");
        EntityArtist idle = CreateArtist(user.ID, "Idle");
        await Add(busy);
        await Add(idle);
        await LinkArtist(fileOne, busy, 0);
        await LinkArtist(fileTwo, busy, 0);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<ArtistSummary> list = await uow.Metadata.GetArtistSummaryList(user.ID, TestContext.Current.CancellationToken);

        Assert.Equal(2, list.Count);
        Assert.Equal(2, list.Single(x => x.Artist.ID == busy.ID).MediaFileCount);
        Assert.Equal(0, list.Single(x => x.Artist.ID == idle.ID).MediaFileCount);
    }

    [Fact]
    public async Task GetGenreAndAlbumSummaryList_CountFilesAndCarryTheAlbumArtists()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile file = await CreateMediaFile(user.ID);
        EntityGenre genre = CreateGenre(user.ID);
        EntityArtist artist = CreateArtist(user.ID);
        await Add(genre);
        await Add(artist);
        EntityAlbum album = CreateAlbum(user.ID);
        album.ArtistLinkList = [new() { AlbumID = album.ID, ArtistID = artist.ID, Position = 0 }];
        await Add(album);
        await LinkGenre(file, genre, 0);
        await SetAlbum(file, album);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<GenreSummary> genreList = await uow.Metadata.GetGenreSummaryList(user.ID, TestContext.Current.CancellationToken);
        List<AlbumSummary> albumList = await uow.Metadata.GetAlbumSummaryList(user.ID, TestContext.Current.CancellationToken);

        Assert.Equal(1, Assert.Single(genreList).MediaFileCount);
        AlbumSummary summary = Assert.Single(albumList);
        Assert.Equal(1, summary.MediaFileCount);
        Assert.Equal([artist.ID], summary.Album.ArtistLinkList.Select(x => x.ArtistID).ToList());
    }

    [Fact]
    public async Task GetSummaryList_IsNewestFirstAndOnlyTheOwners()
    {
        EntityUser owner = await CreateUser("owner@example.com");
        EntityUser other = await CreateUser("other@example.com");
        EntityArtist older = CreateArtist(owner.ID, "Older");
        EntityArtist newer = CreateArtist(owner.ID, "Newer");
        await Add(new EntityArtist { UserID = owner.ID, Name = "Older", NameKey = "older", TimeCreated = DateTime.UtcNow.AddMinutes(-5) });
        await Add(newer);
        await Add(CreateArtist(other.ID, "Foreign"));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<ArtistSummary> list = await uow.Metadata.GetArtistSummaryList(owner.ID, TestContext.Current.CancellationToken);

        Assert.Equal(["Newer", "Older"], list.Select(x => x.Artist.Name).ToList());
        Assert.NotNull(older);
    }

    [Fact]
    public async Task GetStampList_ReturnsIdAndModifiedTimeOfTheOwnersItems()
    {
        EntityUser owner = await CreateUser("owner@example.com");
        EntityUser other = await CreateUser("other@example.com");
        EntityArtist artist = CreateArtist(owner.ID);
        EntityGenre genre = CreateGenre(owner.ID);
        EntityAlbum album = CreateAlbum(owner.ID);
        await Add(artist);
        await Add(genre);
        await Add(album);
        await Add(CreateArtist(other.ID));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<MetadataStamp> artistList = await uow.Metadata.GetArtistStampList(owner.ID, TestContext.Current.CancellationToken);
        List<MetadataStamp> genreList = await uow.Metadata.GetGenreStampList(owner.ID, TestContext.Current.CancellationToken);
        List<MetadataStamp> albumList = await uow.Metadata.GetAlbumStampList(owner.ID, TestContext.Current.CancellationToken);

        Assert.Equal(artist.ID, Assert.Single(artistList).ID);
        Assert.Equal(genre.ID, Assert.Single(genreList).ID);
        Assert.Equal(album.ID, Assert.Single(albumList).ID);
    }

    [Fact]
    public async Task GetStampList_CarriesTheFileCountOfEachItem()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile file = await CreateMediaFile(user.ID);
        EntityMediaFile second = await CreateMediaFile(user.ID);
        EntityArtist artist = CreateArtist(user.ID);
        EntityGenre genre = CreateGenre(user.ID);
        EntityAlbum album = CreateAlbum(user.ID);
        EntityArtist idle = CreateArtist(user.ID, "Idle");
        await Add(artist);
        await Add(idle);
        await Add(genre);
        await Add(album);
        await LinkArtist(file, artist, 0);
        await LinkArtist(second, artist, 0);
        await LinkGenre(file, genre, 0);
        await SetAlbum(file, album);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<MetadataStamp> artistList = await uow.Metadata.GetArtistStampList(user.ID, TestContext.Current.CancellationToken);

        Assert.Equal(2, artistList.Single(x => x.ID == artist.ID).MediaFileCount);
        Assert.Equal(0, artistList.Single(x => x.ID == idle.ID).MediaFileCount);
        Assert.Equal(1, Assert.Single(await uow.Metadata.GetGenreStampList(user.ID, TestContext.Current.CancellationToken)).MediaFileCount);
        Assert.Equal(1, Assert.Single(await uow.Metadata.GetAlbumStampList(user.ID, TestContext.Current.CancellationToken)).MediaFileCount);
    }

    [Fact]
    public async Task UpdateArtist_SavesTheNewNameAndKey()
    {
        EntityUser user = await CreateUser();
        EntityArtist artist = CreateArtist(user.ID, "Old");
        await Add(artist);

        artist.Name = "New";
        artist.NameKey = "new";
        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Metadata.UpdateArtist(artist);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork read = _fixture.UoWFactory.Create();
        EntityArtist? found = await read.Metadata.FindArtist(artist.ID, user.ID, TestContext.Current.CancellationToken);
        Assert.Equal("New", found?.Name);
        Assert.Equal("new", found?.NameKey);
    }

    [Fact]
    public async Task UpdateAlbum_SavesTheRowAndNotItsArtistLinks()
    {
        EntityUser user = await CreateUser();
        EntityArtist artist = CreateArtist(user.ID);
        await Add(artist);
        EntityAlbum album = CreateAlbum(user.ID, "Old");
        album.ArtistLinkList = [new() { AlbumID = album.ID, ArtistID = artist.ID, Position = 0 }];
        await Add(album);

        album.Title = "New";
        album.TitleKey = "new";
        album.IdentityKey = "new|x";
        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Metadata.UpdateAlbum(album);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork read = _fixture.UoWFactory.Create();
        EntityAlbum? found = await read.Metadata.FindAlbum(album.ID, user.ID, TestContext.Current.CancellationToken);
        Assert.Equal("New", found?.Title);
        Assert.Equal("new|x", found?.IdentityKey);
        Assert.Single(found?.ArtistLinkList ?? []);
    }

    [Fact]
    public async Task RemoveAlbumArtist_ThenFind_LeavesTheOthers()
    {
        EntityUser user = await CreateUser();
        EntityArtist first = CreateArtist(user.ID, "A");
        EntityArtist second = CreateArtist(user.ID, "B");
        await Add(first);
        await Add(second);
        EntityAlbum album = CreateAlbum(user.ID);
        album.ArtistLinkList = [new() { AlbumID = album.ID, ArtistID = first.ID, Position = 0 }, new() { AlbumID = album.ID, ArtistID = second.ID, Position = 1 }];
        await Add(album);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Metadata.RemoveAlbumArtist(album.ArtistLinkList[0]);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork read = _fixture.UoWFactory.Create();
        EntityAlbum? found = await read.Metadata.FindAlbum(album.ID, user.ID, TestContext.Current.CancellationToken);
        Assert.Equal([second.ID], found?.ArtistLinkList.Select(x => x.ArtistID).ToList());
    }
}
