using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class MetadataResolutionServiceTest
{
    private readonly FakeStore _store = new();
    private readonly MetadataResolutionService _service;
    private readonly Guid _userID = Guid.CreateVersion7();

    public MetadataResolutionServiceTest()
    {
        NameKeyService serviceNameKey = new();
        _service = new(serviceNameKey, new CreditSplittingService(serviceNameKey));
    }

    private static MediaTagSet Tags(string[]? artist = null, string? album = null, string[]? albumArtist = null, string[]? genre = null)
    {
        return new(null, [.. artist ?? []], album, [.. albumArtist ?? []], [.. genre ?? []], null, null, null, null, null, null);
    }

    private async Task<ResolvedMetadata> ResolveAndCommit(MediaTagSet tags, Guid? userID = null)
    {
        FakeUnitOfWork uow = new(_store);
        ResolvedMetadata result = await _service.Resolve(uow, userID ?? _userID, tags, TestContext.Current.CancellationToken);
        await uow.Commit(TestContext.Current.CancellationToken);
        return result;
    }

    [Fact]
    public async Task Resolve_NewNames_CreatesTheItemsAndSavesNothingItself()
    {
        FakeUnitOfWork uow = new(_store);

        ResolvedMetadata result = await _service.Resolve(uow, _userID, Tags(["Radiohead"], "OK Computer", ["Radiohead"], ["Rock"]), TestContext.Current.CancellationToken);

        Assert.Equal(["Radiohead"], result.ArtistList.Select(x => x.Name).ToList());
        Assert.Equal("OK Computer", result.Album?.Title);
        Assert.Equal(["Rock"], result.GenreList.Select(x => x.Name).ToList());
        Assert.Empty(_store.ArtistList);
        Assert.Empty(_store.AlbumList);
        Assert.Empty(_store.GenreList);
        Assert.Equal(0, _store.CommitCount);

        await uow.Commit(TestContext.Current.CancellationToken);

        Assert.Single(_store.ArtistList);
        Assert.Single(_store.AlbumList);
        Assert.Single(_store.GenreList);
    }

    [Fact]
    public async Task Resolve_NewItems_CarryTheUserTheNameAndTheKey()
    {
        ResolvedMetadata result = await ResolveAndCommit(Tags(["  Massive   Attack "], genre: ["Trip Hop"]));

        EntityArtist artist = Assert.Single(result.ArtistList);
        Assert.Equal(_userID, artist.UserID);
        Assert.Equal("Massive   Attack", artist.Name);
        Assert.Equal("massive attack", artist.NameKey);
        Assert.Equal("trip hop", Assert.Single(result.GenreList).NameKey);
    }

    [Fact]
    public async Task Resolve_TheSameNamesInADifferentSpellingAndCase_ReusesTheExistingItems()
    {
        ResolvedMetadata first = await ResolveAndCommit(Tags(["Radiohead"], "OK Computer", genre: ["Rock"]));

        ResolvedMetadata second = await ResolveAndCommit(Tags(["radiohead "], "ok   COMPUTER", genre: ["ROCK"]));

        Assert.Equal(first.ArtistList[0].ID, second.ArtistList[0].ID);
        Assert.Equal(first.Album?.ID, second.Album?.ID);
        Assert.Equal(first.GenreList[0].ID, second.GenreList[0].ID);
        Assert.Single(_store.ArtistList);
        Assert.Single(_store.AlbumList);
        Assert.Single(_store.GenreList);
        Assert.Equal("Radiohead", _store.ArtistList[0].Name);
    }

    [Fact]
    public async Task Resolve_TheSameNameForAnotherUser_IsAnotherItem()
    {
        Guid otherUserID = Guid.CreateVersion7();
        await ResolveAndCommit(Tags(["Radiohead"], "OK Computer", genre: ["Rock"]));

        await ResolveAndCommit(Tags(["Radiohead"], "OK Computer", genre: ["Rock"]), otherUserID);

        Assert.Equal(2, _store.ArtistList.Count);
        Assert.Equal(2, _store.AlbumList.Count);
        Assert.Equal(2, _store.GenreList.Count);
    }

    [Fact]
    public async Task Resolve_AlbumsWithTheSameTitleByDifferentAlbumArtists_AreTwoAlbums()
    {
        ResolvedMetadata first = await ResolveAndCommit(Tags(album: "Greatest Hits", albumArtist: ["Queen"]));
        ResolvedMetadata second = await ResolveAndCommit(Tags(album: "Greatest Hits", albumArtist: ["ABBA"]));

        Assert.NotEqual(first.Album?.ID, second.Album?.ID);
        Assert.Equal(2, _store.AlbumList.Count);
    }

    [Fact]
    public async Task Resolve_TheSameTitleAndAlbumArtists_IsOneAlbumWhateverTheTracksArtistsAre()
    {
        ResolvedMetadata first = await ResolveAndCommit(Tags(["Queen"], "Greatest Hits", ["Queen"]));
        ResolvedMetadata second = await ResolveAndCommit(Tags(["Queen", "David Bowie"], "Greatest Hits", ["queen"]));

        Assert.Equal(first.Album?.ID, second.Album?.ID);
        Assert.Single(_store.AlbumList);
    }

    [Fact]
    public async Task Resolve_TheSameTitleWithNoAlbumArtist_IsOneAlbumWhateverTheTracksArtistsAre()
    {
        ResolvedMetadata first = await ResolveAndCommit(Tags(["A"], "Compilation"));
        ResolvedMetadata second = await ResolveAndCommit(Tags(["B"], "Compilation"));

        Assert.Equal(first.Album?.ID, second.Album?.ID);
        Assert.Single(_store.AlbumList);
        Assert.Empty(_store.AlbumList[0].ArtistLinkList);
    }

    [Fact]
    public async Task Resolve_TheSameAlbumArtistsInAnotherOrder_IsTheSameAlbum()
    {
        ResolvedMetadata first = await ResolveAndCommit(Tags(album: "Duets", albumArtist: ["A", "B"]));
        ResolvedMetadata second = await ResolveAndCommit(Tags(album: "Duets", albumArtist: ["B", "A"]));

        Assert.Equal(first.Album?.ID, second.Album?.ID);
    }

    [Fact]
    public async Task Resolve_AnAlbum_StoresItsArtistsInOrderAndTheIdentityKeyFromTheSortedIDs()
    {
        ResolvedMetadata result = await ResolveAndCommit(Tags(album: "Duets", albumArtist: ["Zed", "Amy"]));

        EntityAlbum album = Assert.IsType<EntityAlbum>(result.Album);
        Assert.Equal(["Zed", "Amy"], album.ArtistLinkList.OrderBy(x => x.Position).Select(x => _store.ArtistList.Single(a => a.ID == x.ArtistID).Name).ToList());
        Assert.Equal([0, 1], album.ArtistLinkList.Select(x => x.Position).Order().ToList());
        string sortedIDList = string.Join(",", album.ArtistLinkList.Select(x => x.ArtistID).Order());
        Assert.Equal($"duets|{sortedIDList}", album.IdentityKey);
        Assert.Equal("duets", album.TitleKey);
    }

    [Fact]
    public async Task Resolve_AnAlbumWithNoTitle_IsNotCreatedAndItsAlbumArtistsAreNotEither()
    {
        ResolvedMetadata result = await ResolveAndCommit(Tags(album: "   ", albumArtist: ["Queen"]));

        Assert.Null(result.Album);
        Assert.Empty(_store.AlbumList);
        Assert.Empty(_store.ArtistList);
    }

    [Theory]
    [InlineData(";")]
    [InlineData(" ; ;")]
    public async Task Resolve_AnAlbumTitleMadeOnlyOfSeparators_IsNotCreated(string title)
    {
        ResolvedMetadata result = await ResolveAndCommit(Tags(album: title, albumArtist: ["Queen"]));

        Assert.Null(result.Album);
        Assert.Empty(_store.AlbumList);
    }

    [Fact]
    public async Task Resolve_AnAlbumTitleThatContainsASeparator_IsKeptWhole()
    {
        ResolvedMetadata result = await ResolveAndCommit(Tags(album: "Rock; Roll"));

        Assert.Equal("Rock; Roll", result.Album?.Title);
    }

    [Fact]
    public async Task Resolve_ACreditWithAFeaturedArtist_LinksTwoArtistsInThatOrder()
    {
        ResolvedMetadata result = await ResolveAndCommit(Tags(["Main feat. Guest"]));

        Assert.Equal(["Main", "Guest"], result.ArtistList.Select(x => x.Name).ToList());
    }

    [Fact]
    public async Task Resolve_ARealNameWithPunctuation_IsOneArtist()
    {
        ResolvedMetadata result = await ResolveAndCommit(Tags(["AC/DC", "Earth, Wind & Fire"]));

        Assert.Equal(["AC/DC", "Earth, Wind & Fire"], result.ArtistList.Select(x => x.Name).ToList());
    }

    [Fact]
    public async Task Resolve_AGenreCredit_IsSplitTheSameWay()
    {
        ResolvedMetadata result = await ResolveAndCommit(Tags(genre: ["Rock; Pop", "R&B/Soul"]));

        Assert.Equal(["Rock", "Pop", "R&B/Soul"], result.GenreList.Select(x => x.Name).ToList());
    }

    [Fact]
    public async Task Resolve_TheSameArtistNamedTwice_LinksOnce()
    {
        ResolvedMetadata result = await ResolveAndCommit(Tags(["Radiohead", "radiohead", "Radiohead; RADIOHEAD"]));

        Assert.Single(result.ArtistList);
        Assert.Single(_store.ArtistList);
    }

    [Fact]
    public async Task Resolve_BlankValues_AreIgnored()
    {
        ResolvedMetadata result = await ResolveAndCommit(Tags(["", "  ", ";", "A"], genre: ["", " ; "]));

        Assert.Equal(["A"], result.ArtistList.Select(x => x.Name).ToList());
        Assert.Empty(result.GenreList);
        Assert.Empty(_store.GenreList);
    }

    [Fact]
    public async Task Resolve_NothingAtAll_ResolvesToNothing()
    {
        ResolvedMetadata result = await ResolveAndCommit(Tags());

        Assert.Empty(result.ArtistList);
        Assert.Null(result.Album);
        Assert.Empty(result.GenreList);
    }

    [Fact]
    public async Task Resolve_NamesLongerThan255Characters_AreCutBeforeKeying()
    {
        string longName = new('x', 400);
        string longGenre = new('g', 300);
        string longTitle = new('t', 256);

        ResolvedMetadata first = await ResolveAndCommit(Tags([longName], longTitle, genre: [longGenre]));
        ResolvedMetadata second = await ResolveAndCommit(Tags([new string('x', 255) + "different tail"], longTitle[..255], genre: [new('g', 255)]));

        Assert.Equal(255, first.ArtistList[0].Name.Length);
        Assert.Equal(255, first.Album?.Title.Length);
        Assert.Equal(255, first.GenreList[0].Name.Length);
        Assert.Equal(first.ArtistList[0].ID, second.ArtistList[0].ID);
        Assert.Equal(first.Album?.ID, second.Album?.ID);
        Assert.Equal(first.GenreList[0].ID, second.GenreList[0].ID);
    }

    [Fact]
    public async Task Resolve_AnArtistThatIsAlsoAnAlbumArtist_IsOneItem()
    {
        ResolvedMetadata result = await ResolveAndCommit(Tags(["Queen"], "Greatest Hits", ["queen"]));

        Assert.Single(_store.ArtistList);
        Assert.Equal(result.ArtistList[0].ID, result.Album?.ArtistLinkList[0].ArtistID);
    }

    [Fact]
    public async Task Resolve_TwiceInOneUnitOfWork_NamingTheSameNewArtist_CreatesOne()
    {
        FakeUnitOfWork uow = new(_store);

        ResolvedMetadata first = await _service.Resolve(uow, _userID, Tags(["Radiohead"], genre: ["Rock"]), TestContext.Current.CancellationToken);
        ResolvedMetadata second = await _service.Resolve(uow, _userID, Tags(["radiohead"], genre: ["rock"]), TestContext.Current.CancellationToken);
        await uow.Commit(TestContext.Current.CancellationToken);

        Assert.Equal(first.ArtistList[0].ID, second.ArtistList[0].ID);
        Assert.Equal(first.GenreList[0].ID, second.GenreList[0].ID);
        Assert.Single(_store.ArtistList);
        Assert.Single(_store.GenreList);
    }

    [Fact]
    public async Task Resolve_ALoserOfAConcurrentInsert_FailsItsCommitAndTheRetryUsesTheWinner()
    {
        FakeUnitOfWork loser = new(_store);
        ResolvedMetadata losing = await _service.Resolve(loser, _userID, Tags(["Radiohead"], "OK Computer", genre: ["Rock"]), TestContext.Current.CancellationToken);

        // A competing request commits the same names first.
        ResolvedMetadata winning = await ResolveAndCommit(Tags(["Radiohead"], "OK Computer", genre: ["Rock"]));

        await Assert.ThrowsAsync<MetadataKeyConflictException>(() => loser.Commit(TestContext.Current.CancellationToken));

        FakeUnitOfWork retry = new(_store);
        ResolvedMetadata retried = await _service.Resolve(retry, _userID, Tags(["Radiohead"], "OK Computer", genre: ["Rock"]), TestContext.Current.CancellationToken);
        await retry.Commit(TestContext.Current.CancellationToken);

        Assert.NotEqual(losing.ArtistList[0].ID, winning.ArtistList[0].ID);
        Assert.Equal(winning.ArtistList[0].ID, retried.ArtistList[0].ID);
        Assert.Equal(winning.Album?.ID, retried.Album?.ID);
        Assert.Equal(winning.GenreList[0].ID, retried.GenreList[0].ID);
        Assert.Single(_store.ArtistList);
        Assert.Single(_store.AlbumList);
        Assert.Single(_store.GenreList);
    }

    [Fact]
    public async Task Resolve_AnExistingAlbum_ComesBackWithItsArtistLinks()
    {
        await ResolveAndCommit(Tags(album: "Duets", albumArtist: ["A", "B"]));

        ResolvedMetadata again = await ResolveAndCommit(Tags(album: "Duets", albumArtist: ["A", "B"]));

        Assert.Equal(2, again.Album?.ArtistLinkList.Count);
    }
}
