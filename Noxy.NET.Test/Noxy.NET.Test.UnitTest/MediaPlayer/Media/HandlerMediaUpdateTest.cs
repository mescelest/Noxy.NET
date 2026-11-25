using System.Security.Cryptography;
using Noxy.NET.MediaPlayer.Application.Commands.Media;
using Noxy.NET.MediaPlayer.Application.Handlers.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class HandlerMediaUpdateTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerMediaUpdate _handler;

    public HandlerMediaUpdateTest()
    {
        NameKeyService serviceNameKey = new();
        _handler = new(new FakeUnitOfWorkFactory(_store), new MetadataResolutionService(serviceNameKey, new CreditSplittingService(serviceNameKey)), _publisher, _time);
    }

    private EntityMediaFile Seed(Guid? userID = null, string title = "Media", DateTime? timeCreated = null, DateTime? timeModified = null)
    {
        byte[] bytes = [1, 2, 3];
        EntityMediaFile file = new()
        {
            TimeCreated = timeCreated ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TimeModified = timeModified ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = bytes.Length,
            ContentHash = Convert.ToHexStringLower(SHA256.HashData(bytes)),
            UserID = userID ?? UserID
        };
        file.Metadata = new() { Title = title, Year = 1999, MediaFileID = file.ID };

        _store.MediaFileList.Add(file);
        return file;
    }

    private EntityArtist SeedArtist(string name, Guid? userID = null)
    {
        EntityArtist artist = new() { UserID = userID ?? UserID, Name = name, NameKey = name.ToLowerInvariant() };
        _store.ArtistList.Add(artist);
        return artist;
    }

    private EntityGenre SeedGenre(string name, Guid? userID = null)
    {
        EntityGenre genre = new() { UserID = userID ?? UserID, Name = name, NameKey = name.ToLowerInvariant() };
        _store.GenreList.Add(genre);
        return genre;
    }

    private EntityAlbum SeedAlbum(string title, params EntityArtist[] artistList)
    {
        EntityAlbum album = new() { UserID = UserID, Title = title, TitleKey = title.ToLowerInvariant(), IdentityKey = EntityAlbum.BuildIdentityKey(title.ToLowerInvariant(), artistList.Select(x => x.ID)) };
        album.ArtistLinkList = [.. artistList.Select((x, index) => new EntityAlbumArtist { AlbumID = album.ID, ArtistID = x.ID, Position = index })];
        _store.AlbumList.Add(album);
        return album;
    }

    private static void Link(EntityMediaFile file, EntityArtist[]? artistList = null, EntityGenre[]? genreList = null, EntityAlbum? album = null)
    {
        file.ArtistLinkList = [.. (artistList ?? []).Select((x, index) => new EntityMediaFileArtist { MediaFileID = file.ID, ArtistID = x.ID, Position = index })];
        file.GenreLinkList = [.. (genreList ?? []).Select((x, index) => new EntityMediaFileGenre { MediaFileID = file.ID, GenreID = x.ID, Position = index })];
        file.AlbumID = album?.ID;
    }

    private EntityMediaFile Stored(EntityMediaFile file)
    {
        return _store.MediaFileList.Single(x => x.ID == file.ID);
    }

    private EntityMediaFileMetadata StoredMetadata(EntityMediaFile file)
    {
        return Stored(file).Metadata ?? throw new InvalidOperationException("The seeded file has no metadata.");
    }

    private List<string> StoredArtistNameList(EntityMediaFile file)
    {
        return [.. Stored(file).ArtistLinkList.OrderBy(x => x.Position).Select(x => _store.ArtistList.Single(a => a.ID == x.ArtistID).Name)];
    }

    // The changes are given as a command with placeholder user and ID, which are filled in here like the controller fills them from the token and the route.
    private async Task<ResponseMediaFile> Update(EntityMediaFile file, CommandMediaUpdate changes, Guid? userID = null)
    {
        return await _handler.Handle(changes with { UserID = userID ?? UserID, ID = file.ID }, TestContext.Current.CancellationToken);
    }

    private static CommandMediaUpdate Changes()
    {
        return new(Guid.Empty, Guid.Empty);
    }

    [Fact]
    public async Task Update_ChangesOnlyTheSentTags()
    {
        EntityMediaFile file = Seed();

        ResponseMediaFile response = await Update(file, Changes() with { Subtitle = new("Sub") });

        Assert.Equal("Sub", response.Subtitle);
        Assert.Equal("Media", response.Title);
        Assert.Equal(1999, response.Year);
        Assert.Equal("Sub", Stored(file).Metadata?.Subtitle);
    }

    [Fact]
    public async Task Update_StoresEveryEditableTag()
    {
        EntityMediaFile file = Seed();

        ResponseMediaFile response = await Update(file, Changes() with
        {
            Title = new("T"),
            Artist = new("Ar"),
            Album = new("Al"),
            AlbumArtist = new("AA"),
            Subtitle = new("S"),
            Comment = new("C"),
            Genre = new("G"),
            TrackNumber = new(4),
            DiscNumber = new(2),
            Year = new(2024)
        });

        Assert.Equal(["T", "Ar", "Al", "AA", "S", "C", "G"], [response.Title, response.Artist, response.Album, response.AlbumArtist, response.Subtitle, response.Comment, response.Genre]);
        Assert.Equal([4, 2, 2024], [response.TrackNumber, response.DiscNumber, response.Year]);
        Assert.Equal(2024, Stored(file).Metadata?.Year);
    }

    [Fact]
    public async Task Update_NullClearsATag()
    {
        EntityMediaFile file = Seed();
        StoredMetadata(file).Subtitle = "Sub";

        ResponseMediaFile response = await Update(file, Changes() with { Subtitle = new(null), Year = new(null) });

        Assert.Null(response.Subtitle);
        Assert.Null(response.Year);
        Assert.Null(Stored(file).Metadata?.Subtitle);
        Assert.Null(Stored(file).Metadata?.Year);
    }

    [Fact]
    public async Task Update_TextIsTrimmed()
    {
        EntityMediaFile file = Seed();

        ResponseMediaFile response = await Update(file, Changes() with { Title = new("  New Title  "), Album = new("  Other ") });

        Assert.Equal("New Title", response.Title);
        Assert.Equal("Other", response.Album);
    }

    [Fact]
    public async Task Update_ArtistText_LinksTheArtistAndCreatesItWhenNew()
    {
        EntityMediaFile file = Seed();

        ResponseMediaFile response = await Update(file, Changes() with { Artist = new("New Artist") });

        Assert.Equal("New Artist", response.Artist);
        Assert.Equal(["New Artist"], StoredArtistNameList(file));
        Assert.Single(_store.ArtistList);
        Assert.Single(response.ArtistIDList);
    }

    [Fact]
    public async Task Update_ArtistText_SplitsLikeAnUploadAndKeepsTheOrder()
    {
        EntityMediaFile file = Seed();

        ResponseMediaFile response = await Update(file, Changes() with { Artist = new("A; B feat. C") });

        Assert.Equal(["A", "B", "C"], StoredArtistNameList(file));
        Assert.Equal("A; B; C", response.Artist);
    }

    [Fact]
    public async Task Update_ArtistText_ReusesAnExistingArtistWhateverTheSpelling()
    {
        EntityArtist existing = SeedArtist("Radiohead");
        EntityMediaFile file = Seed();

        ResponseMediaFile response = await Update(file, Changes() with { Artist = new("  RADIOHEAD ") });

        Assert.Equal([existing.ID], response.ArtistIDList);
        Assert.Single(_store.ArtistList);
        Assert.Equal("Radiohead", response.Artist);
    }

    [Fact]
    public async Task Update_ARealNameWithPunctuation_StaysOneArtist()
    {
        EntityMediaFile file = Seed();

        await Update(file, Changes() with { Artist = new("Earth, Wind & Fire") });

        Assert.Equal(["Earth, Wind & Fire"], StoredArtistNameList(file));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(";")]
    public async Task Update_BlankOrNullArtistText_ClearsTheLinks(string? value)
    {
        EntityArtist artist = SeedArtist("Artist");
        EntityMediaFile file = Seed();
        Link(file, [artist]);

        ResponseMediaFile response = await Update(file, Changes() with { Artist = new(value) });

        Assert.Null(response.Artist);
        Assert.Empty(response.ArtistIDList);
        Assert.Empty(Stored(file).ArtistLinkList);
        Assert.Single(_store.ArtistList);
    }

    [Fact]
    public async Task Update_TheSameArtistTextTwice_ChangesNothingTheSecondTime()
    {
        EntityMediaFile file = Seed();
        await Update(file, Changes() with { Artist = new("A; B") });
        int commits = _store.CommitCount;
        DateTime modified = Stored(file).TimeModified;
        _time.Now = new(2031, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _publisher.PublishedList.Clear();

        await Update(file, Changes() with { Artist = new("a;b") });

        Assert.Equal(commits, _store.CommitCount);
        Assert.Equal(modified, Stored(file).TimeModified);
        Assert.Empty(_publisher.PublishedList);
        Assert.Equal(2, _store.ArtistList.Count);
    }

    [Fact]
    public async Task Update_GenreText_LinksSeveralGenres()
    {
        EntityMediaFile file = Seed();

        ResponseMediaFile response = await Update(file, Changes() with { Genre = new("Rock; Pop") });

        Assert.Equal("Rock; Pop", response.Genre);
        Assert.Equal(2, response.GenreIDList.Count);
        Assert.Equal(2, _store.GenreList.Count);
    }

    [Fact]
    public async Task Update_AlbumText_LinksTheAlbumKeepingTheCurrentAlbumArtists()
    {
        EntityArtist artist = SeedArtist("Queen");
        EntityAlbum album = SeedAlbum("Greatest Hits", artist);
        EntityMediaFile file = Seed();
        Link(file, album: album);

        ResponseMediaFile response = await Update(file, Changes() with { Album = new("Live") });

        Assert.Equal("Live", response.Album);
        Assert.Equal("Queen", response.AlbumArtist);
        Assert.NotEqual(album.ID, response.AlbumID);
        Assert.Equal([artist.ID], response.AlbumArtistIDList);
        Assert.Equal(2, _store.AlbumList.Count);
    }

    [Fact]
    public async Task Update_AlbumArtistText_MovesTheFileToTheAlbumWithThatTitleAndThoseArtists()
    {
        EntityArtist queen = SeedArtist("Queen");
        EntityAlbum album = SeedAlbum("Greatest Hits", queen);
        EntityMediaFile file = Seed();
        Link(file, album: album);

        ResponseMediaFile response = await Update(file, Changes() with { AlbumArtist = new("ABBA") });

        Assert.Equal("Greatest Hits", response.Album);
        Assert.Equal("ABBA", response.AlbumArtist);
        Assert.NotEqual(album.ID, response.AlbumID);
    }

    [Fact]
    public async Task Update_AlbumArtistTextWithNoAlbumOnTheFile_ChangesNoAlbum()
    {
        EntityMediaFile file = Seed();

        ResponseMediaFile response = await Update(file, Changes() with { AlbumArtist = new("ABBA") });

        Assert.Null(response.AlbumID);
        Assert.Empty(_store.AlbumList);
    }

    [Fact]
    public async Task Update_NullAlbumText_ClearsTheAlbum()
    {
        EntityAlbum album = SeedAlbum("Greatest Hits");
        EntityMediaFile file = Seed();
        Link(file, album: album);

        ResponseMediaFile response = await Update(file, Changes() with { Album = new(null) });

        Assert.Null(response.AlbumID);
        Assert.Null(response.Album);
        Assert.Null(Stored(file).AlbumID);
    }

    [Fact]
    public async Task Update_ArtistIDList_SetsTheLinksInThatOrder()
    {
        EntityArtist first = SeedArtist("First");
        EntityArtist second = SeedArtist("Second");
        EntityMediaFile file = Seed();

        ResponseMediaFile response = await Update(file, Changes() with { ArtistIDList = new([second.ID, first.ID, second.ID]) });

        Assert.Equal([second.ID, first.ID], response.ArtistIDList);
        Assert.Equal("Second; First", response.Artist);
        Assert.Equal(["Second", "First"], StoredArtistNameList(file));
    }

    [Fact]
    public async Task Update_AnEmptyOrNullArtistIDList_ClearsTheLinks()
    {
        EntityArtist artist = SeedArtist("Artist");
        EntityMediaFile file = Seed();
        Link(file, [artist]);

        ResponseMediaFile response = await Update(file, Changes() with { ArtistIDList = new(null) });

        Assert.Empty(response.ArtistIDList);
        Assert.Empty(Stored(file).ArtistLinkList);
    }

    [Fact]
    public async Task Update_GenreIDList_SetsTheLinks()
    {
        EntityGenre genre = SeedGenre("Rock");
        EntityMediaFile file = Seed();

        ResponseMediaFile response = await Update(file, Changes() with { GenreIDList = new([genre.ID]) });

        Assert.Equal([genre.ID], response.GenreIDList);
        Assert.Equal("Rock", response.Genre);
    }

    [Fact]
    public async Task Update_AlbumID_SetsTheAlbum_AndNullClearsIt()
    {
        EntityArtist artist = SeedArtist("Queen");
        EntityAlbum album = SeedAlbum("Greatest Hits", artist);
        EntityMediaFile file = Seed();

        ResponseMediaFile set = await Update(file, Changes() with { AlbumID = new(album.ID) });
        ResponseMediaFile cleared = await Update(file, Changes() with { AlbumID = new(null) });

        Assert.Equal(album.ID, set.AlbumID);
        Assert.Equal("Greatest Hits", set.Album);
        Assert.Equal("Queen", set.AlbumArtist);
        Assert.Null(cleared.AlbumID);
        Assert.Null(Stored(file).AlbumID);
    }

    [Fact]
    public async Task Update_AlbumArtistIDList_MovesTheFileToTheAlbumWithThoseArtists()
    {
        EntityArtist queen = SeedArtist("Queen");
        EntityArtist abba = SeedArtist("ABBA");
        EntityAlbum album = SeedAlbum("Greatest Hits", queen);
        EntityMediaFile file = Seed();
        Link(file, album: album);

        ResponseMediaFile response = await Update(file, Changes() with { AlbumArtistIDList = new([abba.ID]) });

        Assert.Equal("Greatest Hits", response.Album);
        Assert.Equal([abba.ID], response.AlbumArtistIDList);
        Assert.NotEqual(album.ID, response.AlbumID);
    }

    [Fact]
    public async Task Update_AnUnknownArtistID_ThrowsNotFound_AndChangesNothing()
    {
        EntityMediaFile file = Seed();

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => Update(file, Changes() with { ArtistIDList = new([Guid.CreateVersion7()]), Subtitle = new("Sub") }));

        Assert.Null(Stored(file).Metadata?.Subtitle);
        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Update_AnotherUsersItemID_ThrowsNotFound()
    {
        EntityArtist foreign = SeedArtist("Foreign", Guid.CreateVersion7());
        EntityGenre foreignGenre = SeedGenre("Foreign", Guid.CreateVersion7());
        EntityMediaFile file = Seed();

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => Update(file, Changes() with { ArtistIDList = new([foreign.ID]) }));
        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => Update(file, Changes() with { GenreIDList = new([foreignGenre.ID]) }));
        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => Update(file, Changes() with { AlbumID = new(Guid.CreateVersion7()) }));
        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => Update(file, Changes() with { AlbumArtistIDList = new([foreign.ID]) }));

        Assert.Empty(Stored(file).ArtistLinkList);
        Assert.Equal(0, _store.CommitCount);
    }

    [Theory]
    [MemberData(nameof(ConflictingChangeList))]
    public async Task Update_TextAndIDsForTheSameField_IsRejected(CommandMediaUpdate changes)
    {
        EntityMediaFile file = Seed();

        await Assert.ThrowsAsync<ArgumentException>(() => Update(file, changes));

        Assert.Equal(0, _store.CommitCount);
    }

    public static TheoryData<CommandMediaUpdate> ConflictingChangeList()
    {
        Guid id = Guid.CreateVersion7();

        return new()
        {
            new CommandMediaUpdate(Guid.Empty, Guid.Empty) { Artist = new("A"), ArtistIDList = new([id]) },
            new CommandMediaUpdate(Guid.Empty, Guid.Empty) { Genre = new("G"), GenreIDList = new([id]) },
            new CommandMediaUpdate(Guid.Empty, Guid.Empty) { AlbumArtist = new("A"), AlbumArtistIDList = new([id]) },
            new CommandMediaUpdate(Guid.Empty, Guid.Empty) { Album = new("A"), AlbumID = new(id) },
            new CommandMediaUpdate(Guid.Empty, Guid.Empty) { AlbumArtist = new("A"), AlbumID = new(id) },
            new CommandMediaUpdate(Guid.Empty, Guid.Empty) { AlbumArtistIDList = new([id]), AlbumID = new(id) }
        };
    }

    [Fact]
    public async Task Update_TheSameIDsAgain_ChangesNothing()
    {
        EntityArtist artist = SeedArtist("Artist");
        EntityMediaFile file = Seed();
        Link(file, [artist]);
        _time.Now = new(2031, 1, 1, 0, 0, 0, TimeSpan.Zero);

        await Update(file, Changes() with { ArtistIDList = new([artist.ID]) });

        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Update_SettingItemsClearsTheLegacyTextOfThatKind()
    {
        EntityMediaFile file = Seed();
        StoredMetadata(file).Artist = "Legacy artist";
        StoredMetadata(file).Genre = "Legacy genre";
        StoredMetadata(file).Album = "Legacy album";
        StoredMetadata(file).AlbumArtist = "Legacy album artist";

        ResponseMediaFile response = await Update(file, Changes() with { Artist = new(null) });

        Assert.Null(response.Artist);
        Assert.Null(Stored(file).Metadata?.Artist);
        Assert.Equal("Legacy genre", response.Genre);
        Assert.Equal("Legacy album", response.Album);
        Assert.Equal("Legacy album artist", response.AlbumArtist);
    }

    [Fact]
    public async Task Update_AnAlbumEditClearsBothLegacyAlbumTexts()
    {
        EntityMediaFile file = Seed();
        StoredMetadata(file).Album = "Legacy album";
        StoredMetadata(file).AlbumArtist = "Legacy album artist";

        ResponseMediaFile response = await Update(file, Changes() with { Album = new("New album") });

        Assert.Equal("New album", response.Album);
        Assert.Null(response.AlbumArtist);
        Assert.Null(Stored(file).Metadata?.AlbumArtist);
    }

    [Fact]
    public async Task Update_AKeyConflictOnCommit_RunsAgainAndStillSucceeds()
    {
        EntityMediaFile file = Seed();
        _store.KeyConflictCommitCount = 1;

        ResponseMediaFile response = await Update(file, Changes() with { Artist = new("Newcomer") });

        Assert.Equal("Newcomer", response.Artist);
        Assert.Single(_store.ArtistList);
        Assert.Single(_publisher.GetList<NotificationMediaUpdated>());
    }

    [Fact]
    public async Task Update_BumpsTimeModifiedToTheClock_AndKeepsTimeCreated()
    {
        EntityMediaFile file = Seed();
        _time.Now = new(2027, 5, 6, 7, 8, 9, TimeSpan.Zero);

        ResponseMediaFile response = await Update(file, Changes() with { Title = new("Changed") });

        Assert.Equal(_time.Now.UtcDateTime, response.TimeModified);
        Assert.Equal(_time.Now.UtcDateTime, Stored(file).TimeModified);
        Assert.Equal(new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), response.TimeCreated);
    }

    [Fact]
    public async Task Update_ALinkChange_BumpsTimeModifiedAndPushesTheNewIDsAndText()
    {
        EntityMediaFile file = Seed();
        _time.Now = new(2027, 5, 6, 7, 8, 9, TimeSpan.Zero);

        ResponseMediaFile response = await Update(file, Changes() with { Artist = new("A; B"), Genre = new("Rock") });

        Assert.Equal(_time.Now.UtcDateTime, Stored(file).TimeModified);
        NotificationMediaUpdated pushed = Assert.Single(_publisher.GetList<NotificationMediaUpdated>());
        Assert.Equal(response.ArtistIDList, pushed.MediaFile.ArtistIDList);
        Assert.Equal("A; B", pushed.MediaFile.Artist);
        Assert.Equal("Rock", pushed.MediaFile.Genre);
    }

    [Fact]
    public async Task Update_ANewArtist_PushesItWithOneFile_AndTheOldArtistWithItsNewCount()
    {
        EntityArtist old = SeedArtist("Artist");
        EntityMediaFile file = Seed();
        Link(file, [old]);

        await Update(file, Changes() with { Artist = new("Artist2") });

        Guid newID = _store.ArtistList.Single(x => x.Name == "Artist2").ID;
        List<ResponseArtist> pushedList = [.. _publisher.GetList<NotificationArtistUpdated>().Select(x => x.Artist)];
        Assert.Equal(1, pushedList.Single(x => x.ID == newID).MediaFileCount);
        Assert.Equal(0, pushedList.Single(x => x.ID == old.ID).MediaFileCount);
        Assert.Equal(2, pushedList.Count);
        Assert.All(_publisher.GetList<NotificationArtistUpdated>(), x => Assert.Equal(UserID, x.UserID));
    }

    [Fact]
    public async Task Update_AnArtistAnotherFileKeeps_IsPushedWithTheRemainingCount()
    {
        EntityArtist old = SeedArtist("Artist");
        EntityMediaFile file = Seed();
        EntityMediaFile other = Seed();
        Link(file, [old]);
        Link(other, [old]);

        await Update(file, Changes() with { Artist = new("Artist2") });

        ResponseArtist pushed = _publisher.GetList<NotificationArtistUpdated>().Select(x => x.Artist).Single(x => x.ID == old.ID);
        Assert.Equal(1, pushed.MediaFileCount);
    }

    [Fact]
    public async Task Update_AnExistingArtistTheFileJoins_IsPushedWithOneMoreFile()
    {
        EntityArtist target = SeedArtist("Target");
        EntityMediaFile member = Seed();
        Link(member, [target]);
        EntityMediaFile file = Seed();

        await Update(file, Changes() with { Artist = new("Target") });

        ResponseArtist pushed = Assert.Single(_publisher.GetList<NotificationArtistUpdated>()).Artist;
        Assert.Equal(target.ID, pushed.ID);
        Assert.Equal(2, pushed.MediaFileCount);
    }

    [Fact]
    public async Task Update_ANewGenre_PushesItAndTheOldGenre()
    {
        EntityGenre old = SeedGenre("Rock");
        EntityMediaFile file = Seed();
        Link(file, genreList: [old]);

        await Update(file, Changes() with { Genre = new("Jazz") });

        Guid newID = _store.GenreList.Single(x => x.Name == "Jazz").ID;
        List<ResponseGenre> pushedList = [.. _publisher.GetList<NotificationGenreUpdated>().Select(x => x.Genre)];
        Assert.Equal(1, pushedList.Single(x => x.ID == newID).MediaFileCount);
        Assert.Equal(0, pushedList.Single(x => x.ID == old.ID).MediaFileCount);
    }

    [Fact]
    public async Task Update_ANewAlbum_PushesItAndTheOldAlbum()
    {
        EntityArtist artist = SeedArtist("Artist");
        EntityAlbum old = SeedAlbum("Old album", artist);
        EntityMediaFile file = Seed();
        Link(file, [artist], album: old);

        await Update(file, Changes() with { Album = new("New album") });

        Guid newID = _store.AlbumList.Single(x => x.Title == "New album").ID;
        List<ResponseAlbum> pushedList = [.. _publisher.GetList<NotificationAlbumUpdated>().Select(x => x.Album)];
        Assert.Equal(1, pushedList.Single(x => x.ID == newID).MediaFileCount);
        Assert.Equal(0, pushedList.Single(x => x.ID == old.ID).MediaFileCount);
    }

    [Fact]
    public async Task Update_ANewAlbumArtist_PushesItBeforeTheAlbumThatListsIt()
    {
        EntityMediaFile file = Seed();

        await Update(file, Changes() with { Album = new("Album"), AlbumArtist = new("Band") });

        Guid bandID = _store.ArtistList.Single(x => x.Name == "Band").ID;
        Assert.Equal(0, _publisher.GetList<NotificationArtistUpdated>().Single(x => x.Artist.ID == bandID).Artist.MediaFileCount);
        Assert.Equal(1, Assert.Single(_publisher.GetList<NotificationAlbumUpdated>()).Album.MediaFileCount);
        Assert.True(_publisher.PublishedList.FindIndex(x => x is NotificationArtistUpdated) < _publisher.PublishedList.FindIndex(x => x is NotificationAlbumUpdated));
    }

    [Fact]
    public async Task Update_OnlyTheTitle_PushesNoItems()
    {
        EntityArtist artist = SeedArtist("Artist");
        EntityMediaFile file = Seed();
        Link(file, [artist]);

        await Update(file, Changes() with { Title = new("Changed") });

        Assert.Empty(_publisher.GetList<NotificationArtistUpdated>());
        Assert.Empty(_publisher.GetList<NotificationGenreUpdated>());
        Assert.Empty(_publisher.GetList<NotificationAlbumUpdated>());
    }

    [Fact]
    public async Task Update_SameArtistsInAnotherSpelling_PushesNoItems()
    {
        EntityArtist artist = SeedArtist("Artist");
        EntityMediaFile file = Seed();
        Link(file, [artist]);

        await Update(file, Changes() with { Artist = new("artist") });

        Assert.Empty(_publisher.GetList<NotificationArtistUpdated>());
    }

    [Fact]
    public async Task Update_CommitsOnceAndPushesTheNewStateToTheOwner()
    {
        EntityMediaFile file = Seed();

        ResponseMediaFile response = await Update(file, Changes() with { Title = new("Changed") });

        Assert.Equal(1, _store.CommitCount);
        NotificationMediaUpdated pushed = Assert.Single(_publisher.GetList<NotificationMediaUpdated>());
        Assert.Equal(UserID, pushed.UserID);
        Assert.Equal(file.ID, pushed.MediaFile.ID);
        Assert.Equal("Changed", pushed.MediaFile.Title);
        Assert.Equal(response.TimeModified, pushed.MediaFile.TimeModified);
    }

    [Fact]
    public async Task Update_NothingChanged_DoesNotCommitBumpOrPush_AndReturnsTheMediaFile()
    {
        EntityMediaFile file = Seed();
        DateTime before = file.TimeModified;
        _time.Now = new(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);

        ResponseMediaFile same = await Update(file, Changes() with { Title = new("Media"), Year = new(1999) });
        ResponseMediaFile empty = await Update(file, Changes());

        Assert.Equal(0, _store.CommitCount);
        Assert.Empty(_publisher.GetList<NotificationMediaUpdated>());
        Assert.Equal(before, Stored(file).TimeModified);
        Assert.Equal("Media", same.Title);
        Assert.Equal(file.ID, empty.ID);
    }

    [Fact]
    public async Task Update_AnotherUsersMediaFile_ThrowsNotFound_AndPushesNothing()
    {
        EntityMediaFile file = Seed();

        await Assert.ThrowsAsync<MediaNotFoundException>(() => Update(file, Changes() with { Title = new("Hijacked") }, Guid.CreateVersion7()));

        Assert.Equal("Media", Stored(file).Metadata?.Title);
        Assert.Empty(_publisher.GetList<NotificationMediaUpdated>());
        Assert.Equal(0, _store.CommitCount);
    }

    [Fact]
    public async Task Update_UnknownId_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<MediaNotFoundException>(() => _handler.Handle(new(UserID, Guid.CreateVersion7()) { Title = new("x") }, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Update_MediaFileWithoutMetadata_ThrowsNotFound()
    {
        EntityMediaFile file = Seed();
        file.Metadata = null;

        await Assert.ThrowsAsync<MediaNotFoundException>(() => Update(file, Changes() with { Title = new("x") }));
    }

    [Fact]
    public async Task Update_NoOpEdit_DoesNotChangeTheLibraryETag()
    {
        EntityMediaFile file = Seed();
        HandlerMediaLibrary library = new(new FakeUnitOfWorkFactory(_store), new ETagService());
        string before = (await library.Handle(new(UserID, null), TestContext.Current.CancellationToken)).ETag;

        await Update(file, Changes() with { Title = new("Media") });

        Assert.Equal(before, (await library.Handle(new(UserID, null), TestContext.Current.CancellationToken)).ETag);
    }

    [Fact]
    public async Task Update_ALinkEdit_ChangesTheLibraryETag()
    {
        EntityMediaFile file = Seed();
        HandlerMediaLibrary library = new(new FakeUnitOfWorkFactory(_store), new ETagService());
        string before = (await library.Handle(new(UserID, null), TestContext.Current.CancellationToken)).ETag;
        _time.Now = new(2031, 1, 1, 0, 0, 0, TimeSpan.Zero);

        await Update(file, Changes() with { Artist = new("New") });

        Assert.NotEqual(before, (await library.Handle(new(UserID, null), TestContext.Current.CancellationToken)).ETag);
    }

    [Fact]
    public async Task Update_WhenTheCommitFails_PublishesNothing()
    {
        EntityMediaFile file = Seed();
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Update(file, Changes() with { Title = new("Changed") }));

        Assert.Empty(_publisher.PublishedList);
    }

    [Fact]
    public async Task Update_PublishesOnlyAfterTheCommit()
    {
        EntityMediaFile file = Seed();
        int commits = _store.CommitCount;
        int commitsAtPublish = -1;
        _publisher.OnPublish = _ => commitsAtPublish = _store.CommitCount;

        await Update(file, Changes() with { Title = new("Changed") });

        Assert.Equal(commits + 1, commitsAtPublish);
    }

    [Fact]
    public async Task Update_PassesTheCancellationTokenToThePublish()
    {
        EntityMediaFile file = Seed();
        using CancellationTokenSource source = new();

        await _handler.Handle(new(UserID, file.ID) { Title = new("Changed") }, source.Token);

        Assert.Equal(source.Token, Assert.Single(_publisher.TokenList));
    }
}
