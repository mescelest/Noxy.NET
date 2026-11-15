using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Application.BackgroundServices;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class MetadataBackfillServiceTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Original = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly FakeTimeProvider _time = new();
    private readonly FakeLogger<MetadataBackfillService> _logger = new();
    private readonly MetadataBackfillService _service;

    public MetadataBackfillServiceTest()
    {
        _time.Now = new(2027, 5, 6, 7, 8, 9, TimeSpan.Zero);
        NameKeyService serviceNameKey = new();
        _service = new(new FakeUnitOfWorkFactory(_store), new MetadataResolutionService(serviceNameKey, new CreditSplittingService(serviceNameKey)), _time, _logger);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private EntityMediaFile SeedFile(string? artist = null, string? album = null, string? albumArtist = null, string? genre = null, Guid? userID = null, string title = "Title")
    {
        EntityMediaFile file = new() { FilePath = $"{Guid.CreateVersion7()}.mp3", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = new string('a', 64), UserID = userID ?? UserID, TimeModified = Original };
        file.Metadata = new() { Title = title, Artist = artist, Album = album, AlbumArtist = albumArtist, Genre = genre, MediaFileID = file.ID };
        _store.MediaFileList.Add(file);
        return file;
    }

    private EntityMediaFile Stored(EntityMediaFile file)
    {
        return _store.MediaFileList.Single(x => x.ID == file.ID);
    }

    private EntityMediaFileMetadata StoredMetadata(EntityMediaFile file)
    {
        return Stored(file).Metadata ?? throw new InvalidOperationException("The seeded file has no metadata.");
    }

    private EntityMediaFile Loaded(EntityMediaFile file)
    {
        return _store.CopyMediaFile(Stored(file));
    }

    [Fact]
    public async Task Run_CreatesAndLinksTheItemsFromTheLegacyTextByTheRulesOfAnUpload()
    {
        EntityMediaFile file = SeedFile("Main feat. Guest", "OK Computer", "Main", "Rock; Pop");

        int linked = await _service.Run(Token);

        Assert.Equal(1, linked);
        EntityMediaFile result = Loaded(file);
        Assert.Equal(["Main", "Guest"], result.GetArtistNameList());
        Assert.Equal("OK Computer", result.Album?.Title);
        Assert.Equal(["Main"], result.GetAlbumArtistNameList());
        Assert.Equal(["Rock", "Pop"], result.GetGenreNameList());
        Assert.Equal(2, _store.ArtistList.Count);
        Assert.Single(_store.AlbumList);
        Assert.Equal(2, _store.GenreList.Count);
    }

    [Fact]
    public async Task Run_ClearsTheLegacyTextAndKeepsTheTitleAndTheOtherValues()
    {
        EntityMediaFile file = SeedFile("A", "Album", "A", "Rock", title: "My Title");
        StoredMetadata(file).Year = 1999;
        StoredMetadata(file).Comment = "Comment";

        await _service.Run(Token);

        EntityMediaFileMetadata metadata = Assert.IsType<EntityMediaFileMetadata>(Stored(file).Metadata);
        Assert.Null(metadata.Artist);
        Assert.Null(metadata.Album);
        Assert.Null(metadata.AlbumArtist);
        Assert.Null(metadata.Genre);
        Assert.Equal("My Title", metadata.Title);
        Assert.Equal(1999, metadata.Year);
        Assert.Equal("Comment", metadata.Comment);
    }

    [Fact]
    public async Task Run_ThePlainTextOfTheResponse_IsTheSameBeforeAndAfter()
    {
        EntityMediaFile file = SeedFile("Earth, Wind & Fire", "Greatest Hits", "Earth, Wind & Fire", "R&B/Soul");
        Dictionary<string, string?> before = Describe(file);

        await _service.Run(Token);

        Assert.Equal(before, Describe(file));
    }

    private Dictionary<string, string?> Describe(EntityMediaFile file)
    {
        ResponseMediaFile response = Loaded(file).ToResponse();
        return new() { ["artist"] = response.Artist, ["album"] = response.Album, ["albumArtist"] = response.AlbumArtist, ["genre"] = response.Genre };
    }

    [Fact]
    public async Task Run_TheFilesThatChangedGetANewModifiedTime_AndTheOthersDoNot()
    {
        EntityMediaFile legacy = SeedFile("A");
        EntityMediaFile plain = SeedFile();

        await _service.Run(Token);

        Assert.Equal(_time.Now.UtcDateTime, Stored(legacy).TimeModified);
        Assert.Equal(Original, Stored(plain).TimeModified);
    }

    [Fact]
    public async Task Run_AFileAlreadyLinked_IsNotLinkedAgain_ButItsLegacyTextIsCleared()
    {
        EntityArtist existing = new() { UserID = UserID, Name = "Existing", NameKey = "existing" };
        _store.ArtistList.Add(existing);
        EntityMediaFile file = SeedFile("Legacy artist");
        Stored(file).ArtistLinkList = [new() { MediaFileID = file.ID, ArtistID = existing.ID, Position = 0 }];

        await _service.Run(Token);

        Assert.Equal([existing.ID], Stored(file).ArtistLinkList.Select(x => x.ArtistID).ToList());
        Assert.Single(_store.ArtistList);
        Assert.Null(Stored(file).Metadata?.Artist);
    }

    [Fact]
    public async Task Run_Twice_GivesTheSameItemsAndLinks_AndTheSecondRunSavesNothing()
    {
        SeedFile("A; B", "Album", "A", "Rock");
        SeedFile("A", "Album", "A", "Rock");

        await _service.Run(Token);
        List<Guid> artistIDList = [.. _store.ArtistList.Select(x => x.ID)];
        List<Guid> linkIDList = [.. _store.MediaFileList.SelectMany(x => x.ArtistLinkList).Select(x => x.ID)];
        int commits = _store.CommitCount;
        int linked = await _service.Run(Token);

        Assert.Equal(0, linked);
        Assert.Equal(commits, _store.CommitCount);
        Assert.Equal(artistIDList, _store.ArtistList.Select(x => x.ID).ToList());
        Assert.Equal(linkIDList, _store.MediaFileList.SelectMany(x => x.ArtistLinkList).Select(x => x.ID).ToList());
        Assert.Equal(2, _store.ArtistList.Count);
        Assert.Single(_store.AlbumList);
        Assert.Single(_store.GenreList);
    }

    [Fact]
    public async Task Run_TheSameArtistTextOnManyFiles_MakesOneArtist()
    {
        SeedFile("Radiohead");
        SeedFile("radiohead ");
        SeedFile("RADIOHEAD");

        await _service.Run(Token);

        Assert.Single(_store.ArtistList);
        Assert.All(_store.MediaFileList, x => Assert.Single(x.ArtistLinkList));
    }

    [Fact]
    public async Task Run_TwoUsersWithTheSameArtistText_GetTwoSeparateArtists()
    {
        EntityMediaFile mine = SeedFile("Radiohead");
        EntityMediaFile theirs = SeedFile("Radiohead", userID: Guid.CreateVersion7());

        await _service.Run(Token);

        Assert.Equal(2, _store.ArtistList.Count);
        Assert.NotEqual(Stored(mine).ArtistLinkList[0].ArtistID, Stored(theirs).ArtistLinkList[0].ArtistID);
        Assert.Equal(UserID, _store.ArtistList.Single(x => x.ID == Stored(mine).ArtistLinkList[0].ArtistID).UserID);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(";")]
    [InlineData(" ; ")]
    public async Task Run_EmptyWhitespaceOrSeparatorOnlyText_CreatesNothingBroken(string text)
    {
        EntityMediaFile file = SeedFile(text, text, text, text);

        await _service.Run(Token);

        Assert.Empty(_store.ArtistList);
        Assert.Empty(_store.AlbumList);
        Assert.Empty(_store.GenreList);
        Assert.Empty(Stored(file).ArtistLinkList);
        Assert.Null(Stored(file).AlbumID);
        Assert.Null(Stored(file).Metadata?.Artist);
        Assert.Null(Stored(file).Metadata?.Genre);
    }

    [Fact]
    public async Task Run_VeryLongText_IsCutTo255Characters()
    {
        string longText = new('x', 400);
        EntityMediaFile file = SeedFile(longText, longText, null, longText);

        await _service.Run(Token);

        Assert.Equal(255, _store.ArtistList.Single().Name.Length);
        Assert.Equal(255, _store.AlbumList.Single().Title.Length);
        Assert.Equal(255, _store.GenreList.Single().Name.Length);
        Assert.Single(Stored(file).ArtistLinkList);
    }

    [Fact]
    public async Task Run_AnAlbumArtistWithoutAnAlbum_KeepsItsLegacyTextSoNothingIsLost()
    {
        EntityMediaFile file = SeedFile(albumArtist: "Lonely");

        await _service.Run(Token);

        Assert.Equal("Lonely", Stored(file).Metadata?.AlbumArtist);
        Assert.Equal("Lonely", Loaded(file).ToResponse().AlbumArtist);
        Assert.Empty(_store.ArtistList);
        Assert.Equal(Original, Stored(file).TimeModified);
    }

    [Fact]
    public async Task Run_ProcessesInBatchesOf200Files_EachBatchOneCommit()
    {
        for (int index = 0; index < 450; index++)
        {
            SeedFile($"Artist {index % 7}");
        }

        int linked = await _service.Run(Token);

        Assert.Equal(450, linked);
        Assert.Equal(3, _store.CommitCount);
        Assert.Equal(7, _store.ArtistList.Count);
        Assert.All(_store.MediaFileList, x => Assert.Single(x.ArtistLinkList));
    }

    [Fact]
    public async Task Run_AFailingBatch_IsLoggedAndTheNextBatchesStillRun_AndTheNextStartResumesIt()
    {
        for (int index = 0; index < 250; index++)
        {
            SeedFile($"Artist {index}");
        }

        _store.FailCommitCount = 1;
        int first = await _service.Run(Token);

        Assert.Equal(50, first);
        Assert.Contains(_logger.EntryList, x => x.Level == LogLevel.Error);
        Assert.Equal(200, _store.MediaFileList.Count(x => x.ArtistLinkList.Count == 0));

        int second = await _service.Run(Token);

        Assert.Equal(200, second);
        Assert.All(_store.MediaFileList, x => Assert.Single(x.ArtistLinkList));
        Assert.Equal(250, _store.ArtistList.Count);
    }

    [Fact]
    public async Task Run_ALostRaceToCreateAnArtist_RunsTheBatchAgain()
    {
        SeedFile("Newcomer");
        _store.KeyConflictCommitCount = 1;

        int linked = await _service.Run(Token);

        Assert.Equal(1, linked);
        Assert.Single(_store.ArtistList);
    }

    [Fact]
    public async Task Run_WithNoLegacyFiles_DoesNothing()
    {
        SeedFile();

        int linked = await _service.Run(Token);

        Assert.Equal(0, linked);
        Assert.Equal(0, _store.CommitCount);
    }

    [Fact]
    public async Task Run_LogsHowManyFilesItLinked()
    {
        SeedFile("A");
        SeedFile("B");

        await _service.Run(Token);

        Assert.Contains(_logger.EntryList, x => x.Level == LogLevel.Information && x.Message.Contains('2'));
    }

    [Fact]
    public async Task Run_ACancellation_StopsIt()
    {
        SeedFile("A");
        using CancellationTokenSource source = new();
        await source.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _service.Run(source.Token));
    }
}
