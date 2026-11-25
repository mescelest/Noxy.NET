using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Models.Upload;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Upload;

public class UploadProcessorTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly ConfigStorage _config = new() { RootPath = "x", AllowedExtensionList = ["mp3"] };
    private readonly FakeStore _store = new();
    private readonly FakeFileStorageService _storage = new();
    private readonly FakePublisher _publisher = new();
    private readonly FakeMediaTagReadingService _tagReader = new();
    private readonly FakeAlbumArtService _albumArt = new();
    private readonly UploadProcessor _processor;

    private List<(Guid UserID, ResponseUploadResult Result)> NotificationList => [.. _publisher.GetList<NotificationUploadSucceeded>().Select(x => (x.UserID, x.Result)), .. _publisher.GetList<NotificationUploadFailed>().Select(x => (x.UserID, x.Result))];

    public UploadProcessorTest()
    {
        NameKeyService serviceNameKey = new();
        _processor = new(new FakeUnitOfWorkFactory(_store), _storage, _tagReader, new MetadataResolutionService(serviceNameKey, new CreditSplittingService(serviceNameKey)), _albumArt, _publisher, Options.Create(_config), NullLogger<UploadProcessor>.Instance);
    }

    // A job as Enqueue leaves it, with its file waiting in staging.
    private EntityMediaUploadJob AddJob(Guid? userID = null, long size = 100, string title = "Media", DateTime? timeCreated = null, bool staged = true, bool titleWasSent = true, string? hash = null)
    {
        EntityMediaUploadJob job = new()
        {
            TimeCreated = timeCreated ?? DateTime.UtcNow,
            UserID = userID ?? UserID,
            StagedFileName = $"{Guid.CreateVersion7()}.mp3",
            Title = title,
            TitleWasSent = titleWasSent,
            ContentType = "audio/mpeg",
            SizeBytes = size,
            ContentHash = hash ?? $"{Guid.NewGuid():N}{Guid.NewGuid():N}"
        };

        _store.MediaUploadJobList.Add(job);
        if (staged) _storage.Staged[job.StagedFileName] = new byte[size];
        return job;
    }

    private void AddLibraryFile(Guid userID, long size)
    {
        _store.MediaFileList.Add(new()
        {
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = size,
            ContentHash = new('d', 64),
            UserID = userID
        });
    }

    private Task<bool> ProcessNext(CancellationToken? cancellationToken = null)
    {
        return _processor.ProcessNext(cancellationToken ?? TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ProcessNext_NoJobs_ReturnsFalse()
    {
        Assert.False(await ProcessNext());

        Assert.Empty(NotificationList);
    }

    [Fact]
    public async Task ProcessNext_Success_CreatesMediaFileAndMetadataAndDeletesTheJob()
    {
        EntityMediaUploadJob job = AddJob(title: "My Media");

        Assert.True(await ProcessNext());

        EntityMediaFile file = Assert.Single(_store.MediaFileList);
        Assert.Equal(job.StagedFileName, file.FilePath);
        Assert.Equal("audio/mpeg", file.ContentType);
        Assert.Equal(100, file.SizeBytes);
        Assert.Equal(job.ContentHash, file.ContentHash);
        Assert.Equal(UserID, file.UserID);
        Assert.Equal("My Media", file.Metadata?.Title);
        Assert.Equal(file.ID, file.Metadata?.MediaFileID);
        Assert.Empty(_store.MediaUploadJobList);
        Assert.Contains(job.StagedFileName, _storage.Files.Keys);
        Assert.Empty(_storage.Staged);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task ProcessNext_SuccessPublishesOnlySucceeded_AndFailurePublishesOnlyFailed()
    {
        AddJob();
        await ProcessNext();

        Assert.Single(_publisher.GetList<NotificationUploadSucceeded>());
        Assert.Empty(_publisher.GetList<NotificationUploadFailed>());

        _config.MaxFilesPerUser = 1;
        AddJob();
        await ProcessNext();

        Assert.Single(_publisher.GetList<NotificationUploadSucceeded>());
        Assert.Equal(UploadFailureReasonEnum.LibraryFull, Assert.Single(_publisher.GetList<NotificationUploadFailed>()).Result.Reason);
    }

    [Fact]
    public async Task ProcessNext_Success_NotifiesTheOwnerOnce()
    {
        EntityMediaUploadJob job = AddJob(title: "My Media");

        await ProcessNext();

        (Guid userID, ResponseUploadResult result) = Assert.Single(NotificationList);
        Assert.Equal(UserID, userID);
        Assert.True(result.Succeeded);
        Assert.Equal(job.ID, result.JobID);
        Assert.Equal("My Media", result.Title);
        Assert.Equal(Assert.Single(_store.MediaFileList).ID, result.Media?.ID);
        Assert.Null(result.Reason);
    }

    private static MediaTagSet Tags(string? title = null, string[]? artist = null, string? album = null, string[]? albumArtist = null, string[]? genre = null)
    {
        return new(title, [.. artist ?? []], album, [.. albumArtist ?? []], [.. genre ?? []], null, null, null, null, null, null);
    }

    [Fact]
    public async Task ProcessNext_ATaggedFile_CreatesAndLinksItsItemsInTheSameSingleCommit()
    {
        AddJob();
        _tagReader.Result = Tags(artist: ["Main feat. Guest"], album: "OK Computer", albumArtist: ["Main"], genre: ["Rock"]);

        await ProcessNext();

        EntityMediaFile file = Assert.Single(_store.MediaFileList);
        Assert.Equal(["Main", "Guest"], file.ArtistLinkList.OrderBy(x => x.Position).Select(x => _store.ArtistList.Single(a => a.ID == x.ArtistID).Name).ToList());
        EntityAlbum album = Assert.Single(_store.AlbumList);
        Assert.Equal(album.ID, file.AlbumID);
        Assert.Equal("OK Computer", album.Title);
        Assert.Equal(["Main"], album.ArtistLinkList.Select(x => _store.ArtistList.Single(a => a.ID == x.ArtistID).Name).ToList());
        Assert.Equal("Rock", _store.GenreList.Single(x => x.ID == Assert.Single(file.GenreLinkList).GenreID).Name);
        Assert.Equal(2, _store.ArtistList.Count);
        Assert.Empty(_store.MediaUploadJobList);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task ProcessNext_AFileTheLibraryAlreadyHolds_IsRefusedAsADuplicate_AndNothingIsStored()
    {
        string hash = new('a', 64);
        _store.MediaFileList.Add(new() { FilePath = "held.mp3", ContentType = "audio/mpeg", SizeBytes = 100, ContentHash = hash, UserID = UserID });
        EntityMediaUploadJob job = AddJob(hash: hash);

        await ProcessNext();

        ResponseUploadResult result = Assert.Single(_publisher.GetList<NotificationUploadFailed>()).Result;
        Assert.False(result.Succeeded);
        Assert.Equal(UploadFailureReasonEnum.Duplicate, result.Reason);
        Assert.Equal(job.ID, result.JobID);
        Assert.Equal(_store.MediaFileList.Single().ID, result.Media?.ID);
        Assert.Single(_store.MediaFileList);
        Assert.Empty(_store.MediaUploadJobList);
        Assert.Empty(_storage.Staged);
        Assert.DoesNotContain(job.StagedFileName, _storage.Files.Keys);
    }

    [Fact]
    public async Task ProcessNext_TheSameFileSentTwiceInOneGo_StoresTheFirstAndRefusesTheSecond()
    {
        string hash = new('b', 64);
        AddJob(hash: hash);
        AddJob(hash: hash);

        await ProcessNext();
        await ProcessNext();

        Assert.Single(_store.MediaFileList);
        Assert.Single(_publisher.GetList<NotificationUploadSucceeded>());
        Assert.Equal(UploadFailureReasonEnum.Duplicate, Assert.Single(_publisher.GetList<NotificationUploadFailed>()).Result.Reason);
    }

    [Fact]
    public async Task ProcessNext_AFileAnotherUserHolds_IsNotADuplicate()
    {
        string hash = new('e', 64);
        _store.MediaFileList.Add(new() { FilePath = "theirs.mp3", ContentType = "audio/mpeg", SizeBytes = 100, ContentHash = hash, UserID = Guid.CreateVersion7() });
        AddJob(hash: hash);

        await ProcessNext();

        Assert.Equal(2, _store.MediaFileList.Count);
        Assert.Empty(_publisher.GetList<NotificationUploadFailed>());
    }

    [Fact]
    public async Task ProcessNext_ATaggedFile_PushesItsNewItemsWithOneFileEach()
    {
        AddJob();
        _tagReader.Result = Tags(artist: ["Main"], album: "OK Computer", albumArtist: ["Main", "Band"], genre: ["Rock"]);

        await ProcessNext();

        List<ResponseArtist> artistList = [.. _publisher.GetList<NotificationArtistUpdated>().Select(x => x.Artist)];
        Assert.Equal(1, artistList.Single(x => x.Name == "Main").MediaFileCount);
        Assert.Equal(0, artistList.Single(x => x.Name == "Band").MediaFileCount);
        Assert.Equal(1, Assert.Single(_publisher.GetList<NotificationGenreUpdated>()).Genre.MediaFileCount);
        Assert.Equal(1, Assert.Single(_publisher.GetList<NotificationAlbumUpdated>()).Album.MediaFileCount);
        Assert.All(_publisher.GetList<NotificationAlbumUpdated>(), x => Assert.Equal(UserID, x.UserID));
    }

    [Fact]
    public async Task ProcessNext_ASecondTrackOfTheSameAlbum_PushesTheItemsWithTheirNewCounts()
    {
        AddJob();
        _tagReader.Result = Tags(artist: ["Main"], album: "OK Computer", albumArtist: ["Main"], genre: ["Rock"]);
        await ProcessNext();
        _publisher.PublishedList.Clear();
        AddJob();

        await ProcessNext();

        Assert.Equal(2, _publisher.GetList<NotificationArtistUpdated>().Single(x => x.Artist.Name == "Main").Artist.MediaFileCount);
        Assert.Equal(2, Assert.Single(_publisher.GetList<NotificationGenreUpdated>()).Genre.MediaFileCount);
        Assert.Equal(2, Assert.Single(_publisher.GetList<NotificationAlbumUpdated>()).Album.MediaFileCount);
    }

    [Fact]
    public async Task ProcessNext_ItemsArePushedBeforeTheUploadResult()
    {
        AddJob();
        _tagReader.Result = Tags(artist: ["Main"]);

        await ProcessNext();

        int artistIndex = _publisher.PublishedList.FindIndex(x => x is NotificationArtistUpdated);
        int resultIndex = _publisher.PublishedList.FindIndex(x => x is NotificationUploadSucceeded);
        Assert.InRange(artistIndex, 0, resultIndex - 1);
    }

    [Fact]
    public async Task ProcessNext_AFileWithoutTags_PushesNoItems()
    {
        AddJob();

        await ProcessNext();

        Assert.Empty(_publisher.GetList<NotificationArtistUpdated>());
        Assert.Empty(_publisher.GetList<NotificationGenreUpdated>());
        Assert.Empty(_publisher.GetList<NotificationAlbumUpdated>());
    }

    [Fact]
    public async Task ProcessNext_ASecondTrackOfTheSameAlbum_ReusesTheAlbumAndTheArtists()
    {
        AddJob();
        AddJob();
        _tagReader.Result = Tags(artist: ["Main"], album: "OK Computer", albumArtist: ["Main"], genre: ["Rock"]);

        await ProcessNext();
        await ProcessNext();

        Assert.Equal(2, _store.MediaFileList.Count);
        Assert.Single(_store.AlbumList);
        Assert.Single(_store.ArtistList);
        Assert.Single(_store.GenreList);
        Assert.All(_store.MediaFileList, x => Assert.Equal(_store.AlbumList[0].ID, x.AlbumID));
    }

    [Fact]
    public async Task ProcessNext_AKeyConflictOnCommit_RunsAgainAndStillCreatesOneFile()
    {
        AddJob();
        _tagReader.Result = Tags(artist: ["Newcomer"]);
        _store.KeyConflictCommitCount = 1;

        await ProcessNext();

        Assert.Single(_store.MediaFileList);
        Assert.Single(_store.ArtistList);
        Assert.Empty(_store.MediaUploadJobList);
        Assert.Single(_publisher.GetList<NotificationUploadSucceeded>());
        Assert.Empty(_publisher.GetList<NotificationUploadFailed>());
    }

    [Fact]
    public async Task ProcessNext_ATitleTheUserSent_WinsOverTheEmbeddedTitle()
    {
        AddJob(title: "Sent title", titleWasSent: true);
        _tagReader.Result = Tags(title: "Embedded title");

        await ProcessNext();

        Assert.Equal("Sent title", Assert.Single(_store.MediaFileList).Metadata?.Title);
    }

    [Fact]
    public async Task ProcessNext_NoTitleSent_UsesTheEmbeddedTitle()
    {
        AddJob(title: "file name", titleWasSent: false);
        _tagReader.Result = Tags(title: "  Embedded title ");

        await ProcessNext();

        Assert.Equal("Embedded title", Assert.Single(_store.MediaFileList).Metadata?.Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ProcessNext_NoTitleSentAndNoEmbeddedTitle_UsesTheFileName(string? embedded)
    {
        AddJob(title: "file name", titleWasSent: false);
        _tagReader.Result = Tags(title: embedded);

        await ProcessNext();

        Assert.Equal("file name", Assert.Single(_store.MediaFileList).Metadata?.Title);
    }

    [Fact]
    public async Task ProcessNext_ALongEmbeddedTitle_IsCutTo255Characters()
    {
        AddJob(titleWasSent: false);
        _tagReader.Result = Tags(title: new('t', 400));

        await ProcessNext();

        Assert.Equal(255, Assert.Single(_store.MediaFileList).Metadata?.Title.Length);
    }

    [Fact]
    public async Task ProcessNext_StoresTheValuesTheTagsCarry()
    {
        AddJob();
        _tagReader.Result = new(null, [], null, [], [], 3, 2, 1999, "A comment", "A subtitle", TimeSpan.FromSeconds(215));

        await ProcessNext();

        EntityMediaFileMetadata metadata = Assert.IsType<EntityMediaFileMetadata>(Assert.Single(_store.MediaFileList).Metadata);
        Assert.Equal(3, metadata.TrackNumber);
        Assert.Equal(2, metadata.DiscNumber);
        Assert.Equal(1999, metadata.Year);
        Assert.Equal("A comment", metadata.Comment);
        Assert.Equal("A subtitle", metadata.Subtitle);
        Assert.Equal(TimeSpan.FromSeconds(215), metadata.Duration);
    }

    [Fact]
    public async Task ProcessNext_LongCommentAndSubtitle_AreCutToTheirColumnLengths()
    {
        AddJob();
        _tagReader.Result = new(null, [], null, [], [], null, null, null, new('c', 5000), new('s', 400), null);

        await ProcessNext();

        EntityMediaFileMetadata metadata = Assert.IsType<EntityMediaFileMetadata>(Assert.Single(_store.MediaFileList).Metadata);
        Assert.Equal(2048, metadata.Comment?.Length);
        Assert.Equal(256, metadata.Subtitle?.Length);
    }

    [Fact]
    public async Task ProcessNext_ImplausibleNumbers_AreNotStored()
    {
        AddJob();
        _tagReader.Result = new(null, [], null, [], [], -1, 0, int.MaxValue, null, null, null);

        await ProcessNext();

        EntityMediaFileMetadata metadata = Assert.IsType<EntityMediaFileMetadata>(Assert.Single(_store.MediaFileList).Metadata);
        Assert.Null(metadata.TrackNumber);
        Assert.Null(metadata.DiscNumber);
        Assert.Null(metadata.Year);
    }

    [Fact]
    public async Task ProcessNext_UnreadableTags_GiveALibraryEntryTitledFromTheFileNameWithNoItems()
    {
        AddJob(title: "file name", titleWasSent: false);

        await ProcessNext();

        EntityMediaFile file = Assert.Single(_store.MediaFileList);
        Assert.Equal("file name", file.Metadata?.Title);
        Assert.Empty(file.ArtistLinkList);
        Assert.Empty(file.GenreLinkList);
        Assert.Null(file.AlbumID);
        Assert.Empty(_store.ArtistList);
        Assert.Empty(_store.AlbumList);
        Assert.Empty(_store.GenreList);
    }

    [Fact]
    public async Task ProcessNext_AFailingTagRead_NeverFailsTheJob()
    {
        AddJob(title: "file name", titleWasSent: false);
        _tagReader.Failure = new InvalidOperationException("boom");

        await ProcessNext();

        Assert.Equal("file name", Assert.Single(_store.MediaFileList).Metadata?.Title);
        Assert.Single(_publisher.GetList<NotificationUploadSucceeded>());
        Assert.Empty(_publisher.GetList<NotificationUploadFailed>());
    }

    [Fact]
    public async Task ProcessNext_ReadsTheTagsOfTheStoredFileAndDeletesTheTemporaryCopy()
    {
        EntityMediaUploadJob job = AddJob();

        await ProcessNext();

        string path = Assert.Single(_tagReader.ReadList);
        Assert.EndsWith(Path.GetExtension(job.StagedFileName), path);
        Assert.Contains(path, _storage.DeletedTemporaryList);
        Assert.Empty(_storage.Temporary);
    }

    [Fact]
    public async Task ProcessNext_TheSuccessNotification_CarriesTheIDListsAndTheDerivedText()
    {
        AddJob();
        _tagReader.Result = Tags(artist: ["Main; Guest"], album: "OK Computer", albumArtist: ["Main"], genre: ["Rock", "Pop"]);

        await ProcessNext();

        ResponseMediaFile response = Assert.IsType<ResponseMediaFile>(Assert.Single(_publisher.GetList<NotificationUploadSucceeded>()).Result.Media);
        EntityMediaFile file = Assert.Single(_store.MediaFileList);
        Assert.Equal("Main; Guest", response.Artist);
        Assert.Equal("OK Computer", response.Album);
        Assert.Equal("Main", response.AlbumArtist);
        Assert.Equal("Rock; Pop", response.Genre);
        Assert.Equal(file.ArtistLinkList.OrderBy(x => x.Position).Select(x => x.ArtistID).ToList(), response.ArtistIDList);
        Assert.Equal(file.AlbumID, response.AlbumID);
        Assert.Equal([_store.ArtistList.Single(x => x.Name == "Main").ID], response.AlbumArtistIDList);
        Assert.Equal(2, response.GenreIDList.Count);
    }

    [Fact]
    public async Task ProcessNext_AMissingStagedFile_StillFailsAsFileMissing()
    {
        AddJob(staged: false);

        await ProcessNext();

        Assert.Equal(UploadFailureReasonEnum.FileMissing, Assert.Single(_publisher.GetList<NotificationUploadFailed>()).Result.Reason);
        Assert.Empty(_store.MediaFileList);
    }

    [Fact]
    public async Task ProcessNext_ProcessesOldestFirst()
    {
        DateTime now = DateTime.UtcNow;
        EntityMediaUploadJob newer = AddJob(title: "newer", timeCreated: now);
        EntityMediaUploadJob older = AddJob(title: "older", timeCreated: now.AddMinutes(-5));

        await ProcessNext();

        Assert.Equal(older.StagedFileName, Assert.Single(_store.MediaFileList).FilePath);
        Assert.Equal(newer.ID, Assert.Single(_store.MediaUploadJobList).ID);
    }

    [Fact]
    public async Task ProcessNext_AtTheFileLimit_FailsWithLibraryFull_DeletesStagedAndJob_AndNotifies()
    {
        _config.MaxFilesPerUser = 1;
        AddLibraryFile(UserID, 10);
        EntityMediaUploadJob job = AddJob();

        Assert.True(await ProcessNext());

        Assert.Single(_store.MediaFileList);
        Assert.Empty(_store.MediaUploadJobList);
        Assert.Empty(_storage.Staged);
        Assert.Contains(job.StagedFileName, _storage.DeletedStagedList);
        (Guid userID, ResponseUploadResult result) = Assert.Single(NotificationList);
        Assert.Equal(UserID, userID);
        Assert.False(result.Succeeded);
        Assert.Equal(UploadFailureReasonEnum.LibraryFull, result.Reason);
        Assert.Equal(job.ID, result.JobID);
        Assert.Null(result.Media);
    }

    [Theory]
    [InlineData(51, false)]
    [InlineData(50, true)]
    public async Task ProcessNext_ByteLimit_FailsWhenTheFileWouldOverflow_AndSucceedsWhenItExactlyFits(long jobSize, bool succeeds)
    {
        _config.MaxBytesPerUser = 150;
        AddLibraryFile(UserID, 100);
        AddJob(size: jobSize);

        await ProcessNext();

        Assert.Equal(succeeds, Assert.Single(NotificationList).Result.Succeeded);
        Assert.Equal(succeeds ? 2 : 1, _store.MediaFileList.Count);
    }

    [Fact]
    public async Task ProcessNext_ByteLimitIsPerUser()
    {
        _config.MaxBytesPerUser = 150;
        AddLibraryFile(Guid.CreateVersion7(), 150);
        AddJob(size: 100);

        await ProcessNext();

        Assert.True(Assert.Single(NotificationList).Result.Succeeded);
    }

    [Fact]
    public async Task ProcessNext_StagedAndLibraryFileBothMissing_FailsWithFileMissing()
    {
        AddJob(staged: false);

        await ProcessNext();

        ResponseUploadResult result = Assert.Single(NotificationList).Result;
        Assert.False(result.Succeeded);
        Assert.Equal(UploadFailureReasonEnum.FileMissing, result.Reason);
        Assert.Empty(_store.MediaFileList);
        Assert.Empty(_store.MediaUploadJobList);
    }

    [Fact]
    public async Task ProcessNext_FileAlreadyPromoted_StillCompletes()
    {
        EntityMediaUploadJob job = AddJob();
        _storage.Files[job.StagedFileName] = _storage.Staged[job.StagedFileName];
        _storage.Staged.Clear();

        await ProcessNext();

        Assert.Equal(job.StagedFileName, Assert.Single(_store.MediaFileList).FilePath);
        Assert.Empty(_store.MediaUploadJobList);
        Assert.True(Assert.Single(NotificationList).Result.Succeeded);
    }

    [Fact]
    public async Task ProcessNext_CommitFailsAfterPromote_DeletesTheLibraryFileAndFailsTheJob()
    {
        EntityMediaUploadJob job = AddJob();
        _store.FailCommitCount = 1;

        await ProcessNext();

        Assert.Empty(_store.MediaFileList);
        Assert.Empty(_store.MediaUploadJobList);
        Assert.Empty(_storage.Files);
        Assert.Contains(job.StagedFileName, _storage.DeletedList);
        ResponseUploadResult result = Assert.Single(NotificationList).Result;
        Assert.False(result.Succeeded);
        Assert.Equal(UploadFailureReasonEnum.Unknown, result.Reason);
    }

    [Fact]
    public async Task ProcessNext_UnexpectedError_FailsWithUnknown_AndCleansUp()
    {
        EntityMediaUploadJob job = AddJob();
        _storage.PromoteFailure = new IOException("disk gone");

        await ProcessNext();

        Assert.Equal(UploadFailureReasonEnum.Unknown, Assert.Single(NotificationList).Result.Reason);
        Assert.Empty(_store.MediaUploadJobList);
        Assert.Contains(job.StagedFileName, _storage.DeletedStagedList);
    }

    [Fact]
    public async Task ProcessNext_Cancelled_RethrowsAndLeavesTheJobProcessing()
    {
        EntityMediaUploadJob job = AddJob();
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();
        _storage.PromoteFailure = new OperationCanceledException(cts.Token);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ProcessNext(cts.Token));

        Assert.Equal(MediaUploadJobStatusEnum.Processing, Assert.Single(_store.MediaUploadJobList).Status);
        Assert.Contains(job.StagedFileName, _storage.Staged.Keys);
        Assert.Empty(NotificationList);
    }

    [Fact]
    public async Task ProcessNext_TwoJobsSameUserLimitOne_FirstSucceedsSecondFailsLibraryFull()
    {
        _config.MaxFilesPerUser = 1;
        DateTime now = DateTime.UtcNow;
        AddJob(title: "first", timeCreated: now.AddMinutes(-1));
        AddJob(title: "second", timeCreated: now);

        await ProcessNext();
        await ProcessNext();

        Assert.Equal("first", Assert.Single(_store.MediaFileList).Metadata?.Title);
        Assert.Equal([true, false], NotificationList.Select(x => x.Result.Succeeded));
        Assert.Equal(UploadFailureReasonEnum.LibraryFull, NotificationList[1].Result.Reason);
        Assert.Empty(_store.MediaUploadJobList);
    }

    [Fact]
    public async Task ProcessNext_ClaimLostToAnotherWorker_ReturnsTrueAndDoesNothing()
    {
        AddJob();
        _store.LoseJobClaim = true;

        Assert.True(await ProcessNext());

        Assert.Empty(_store.MediaFileList);
        Assert.Single(_store.MediaUploadJobList);
        Assert.Empty(NotificationList);
    }

    [Fact]
    public async Task ProcessNext_FailureCleanup_StagedDeleteThrows_StillDeletesTheLibraryFile()
    {
        EntityMediaUploadJob job = AddJob();
        _store.FailCommitCount = 1;
        _storage.DeleteStagedFailure = new IOException("locked");

        await ProcessNext();

        Assert.Contains(job.StagedFileName, _storage.DeletedList);
        Assert.Empty(_storage.Files);
        Assert.Empty(_store.MediaUploadJobList);
    }

    [Fact]
    public async Task ProcessNext_JobCannotBeRemoved_KeepsTheFilesAndStaysSilentSoARestartCanRetry()
    {
        EntityMediaUploadJob job = AddJob();
        // The commit that finishes the job fails, and then so does the commit that removes it after the failure.
        _store.FailCommitCount = 2;

        Assert.True(await ProcessNext());

        Assert.Single(_store.MediaUploadJobList);
        Assert.Contains(job.StagedFileName, _storage.Files.Keys);
        Assert.Empty(_storage.DeletedList);
        Assert.Empty(_storage.DeletedStagedList);
        Assert.Empty(NotificationList);
    }

    [Fact]
    public async Task ProcessNext_JobCannotBeRemoved_ARestartThenCompletesIt()
    {
        EntityMediaUploadJob job = AddJob();
        _store.FailCommitCount = 2;
        await ProcessNext();

        await _processor.Recover(TestContext.Current.CancellationToken);
        await ProcessNext();

        Assert.Equal(job.StagedFileName, Assert.Single(_store.MediaFileList).FilePath);
        Assert.Empty(_store.MediaUploadJobList);
        Assert.True(Assert.Single(NotificationList).Result.Succeeded);
    }

    [Fact]
    public async Task ProcessNext_WithAPicture_GivesTheNewAlbumItsPicture()
    {
        AddJob();
        byte[] picture = [7, 7, 7];
        _tagReader.Result = _tagReader.Result with { Album = "OK Computer", Picture = picture };

        await ProcessNext();

        EntityAlbum album = Assert.Single(_store.AlbumList);
        (Guid userID, Guid albumID, byte[] applied) = Assert.Single(_albumArt.ApplyList);
        Assert.Equal(UserID, userID);
        Assert.Equal(album.ID, albumID);
        Assert.Equal(picture, applied);
    }

    [Fact]
    public async Task ProcessNext_WithAPictureOfAnAlbumThatHasNone_OffersItToTheAlbum()
    {
        AddJob();
        _tagReader.Result = _tagReader.Result with { Album = "OK Computer" };
        await ProcessNext();
        Assert.Empty(_albumArt.ApplyList);

        AddJob();
        _tagReader.Result = _tagReader.Result with { Picture = [1] };
        await ProcessNext();

        Assert.Equal(Assert.Single(_store.AlbumList).ID, Assert.Single(_albumArt.ApplyList).AlbumID);
    }

    [Fact]
    public async Task ProcessNext_WithAPictureButNoAlbum_OffersNothing()
    {
        AddJob();
        _tagReader.Result = _tagReader.Result with { Picture = [1] };

        await ProcessNext();

        Assert.Empty(_albumArt.ApplyList);
    }

    [Fact]
    public async Task ProcessNext_WithNoPicture_OffersNothing()
    {
        AddJob();
        _tagReader.Result = _tagReader.Result with { Album = "OK Computer" };

        await ProcessNext();

        Assert.Empty(_albumArt.ApplyList);
    }

    [Fact]
    public async Task ProcessNext_WhenTheArtStepThrows_StillSucceeds()
    {
        AddJob();
        _tagReader.Result = _tagReader.Result with { Album = "OK Computer", Picture = [1] };
        _albumArt.Failure = new InvalidOperationException("The picture step failed.");

        await ProcessNext();

        Assert.Single(_store.MediaFileList);
        Assert.True(Assert.Single(NotificationList).Result.Succeeded);
    }
}
