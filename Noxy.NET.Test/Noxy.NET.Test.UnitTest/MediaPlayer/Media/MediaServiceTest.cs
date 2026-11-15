using System.Security.Cryptography;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Gates;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Application.Validators;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class MediaServiceTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly ConfigStorage _storageConfig = new() { RootPath = "x", AllowedExtensionList = ["mp3", "flac"] };
    private readonly ConfigUpload _uploadConfig = new();
    private readonly FakeFileStorageService _storage = new();
    private readonly FakeUploadSignal _signal = new();
    private readonly FakeStore _store = new();
    private readonly MediaService _service;

    public MediaServiceTest()
    {
        IOptions<ConfigStorage> storageOptions = Options.Create(_storageConfig);
        _service = new(new FakeUnitOfWorkFactory(_store), new MediaFileValidator(storageOptions), _storage, new FakeMediaTagWriter(), _signal, storageOptions, Options.Create(_uploadConfig), new UploadGate(), new FakeMediaNotifier(), new FakePlaylistNotifier(),
            new ETagService(), new FakeTimeProvider(),
            NullLogger<MediaService>.Instance);
    }

    private static byte[] Mp3(int length = 100)
    {
        byte[] bytes = new byte[length];
        "ID3"u8.CopyTo(bytes);
        return bytes;
    }

    private Task<ResponseUploadAccepted> Enqueue(string fileName, byte[] content, string? title = null, Guid? userID = null)
    {
        return _service.Enqueue(userID ?? UserID, fileName, title, new MemoryStream(content), TestContext.Current.CancellationToken);
    }

    // A file that is already in the library, as the worker would have left it.
    private MediaFile Seed(Guid userID, string title, int length = 100, DateTime? timeCreated = null)
    {
        byte[] bytes = Mp3(length);
        MediaFile file = new()
        {
            TimeCreated = timeCreated ?? DateTime.UtcNow,
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = length,
            ContentHash = Convert.ToHexStringLower(SHA256.HashData(bytes)),
            UserID = userID
        };
        file.Metadata = new() { Title = title, MediaFileID = file.ID };

        _storage.Files[file.FilePath] = bytes;
        _store.MediaFileList.Add(file);
        return file;
    }

    private void SeedPendingJob(Guid userID, long sizeBytes = 10)
    {
        _store.MediaUploadJobList.Add(new()
        {
            UserID = userID,
            StagedFileName = $"{Guid.CreateVersion7()}.mp3",
            Title = "Waiting",
            ContentType = "audio/mpeg",
            SizeBytes = sizeBytes,
            ContentHash = new string('a', 64)
        });
    }

    [Fact]
    public async Task Enqueue_StagesTheWholeFileIncludingTheHeader()
    {
        byte[] content = Mp3();

        await Enqueue("media.mp3", content);

        Assert.Equal(content, Assert.Single(_storage.Staged).Value);
        Assert.Empty(_storage.Files);
    }

    [Fact]
    public async Task Enqueue_CreatesAPendingJobWithTheFileFacts()
    {
        byte[] content = Mp3();

        ResponseUploadAccepted response = await Enqueue("media.test.mp3", content, "My media");

        MediaUploadJob job = Assert.Single(_store.MediaUploadJobList);
        Assert.Equal(response.JobID, job.ID);
        Assert.Equal(UserID, job.UserID);
        Assert.Equal(_storage.Staged.Keys.Single(), job.StagedFileName);
        Assert.Equal("My media", job.Title);
        Assert.Equal("audio/mpeg", job.ContentType);
        Assert.Equal(100, job.SizeBytes);
        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(content)), job.ContentHash);
        Assert.Equal(MediaUploadJobStatus.Pending, job.Status);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task Enqueue_NoTitle_UsesTheFileNameWithoutItsExtension()
    {
        await Enqueue("media.mp3", Mp3());

        Assert.Equal("media", Assert.Single(_store.MediaUploadJobList).Title);
    }

    [Fact]
    public async Task Enqueue_TooLongTitleOrFileName_IsCutToTheColumnLength()
    {
        await Enqueue("a.mp3", Mp3(), new string('t', 400));
        await Enqueue(new string('a', 600) + ".mp3", Mp3());

        Assert.All(_store.MediaUploadJobList, job => Assert.Equal(255, job.Title.Length));
    }

    [Fact]
    public async Task Enqueue_NotifiesTheSignalOnce()
    {
        await Enqueue("media.mp3", Mp3());

        Assert.Equal(1, _signal.NotifyCount);
    }

    [Fact]
    public async Task Enqueue_DoesNotCreateAMediaFileYet()
    {
        await Enqueue("media.mp3", Mp3());

        Assert.Empty(_store.MediaFileList);
    }

    [Theory]
    [InlineData("media.exe")]
    [InlineData("media.flac")]
    public async Task Enqueue_ExtensionNotMatchingTheHeader_ThrowsAndStagesNothing(string fileName)
    {
        await Assert.ThrowsAsync<UnsupportedMediaTypeException>(() => Enqueue(fileName, Mp3()));

        AssertNothingQueued();
    }

    [Fact]
    public async Task Enqueue_EmptyFile_ThrowsAndStagesNothing()
    {
        await Assert.ThrowsAsync<UnsupportedMediaTypeException>(() => Enqueue("media.mp3", []));

        AssertNothingQueued();
    }

    [Fact]
    public async Task Enqueue_ShortFileWithAWrongHeader_ThrowsAndStagesNothing()
    {
        await Assert.ThrowsAsync<UnsupportedMediaTypeException>(() => Enqueue("media.mp3", "abc"u8.ToArray()));

        AssertNothingQueued();
    }

    [Fact]
    public async Task Enqueue_MediaPlusPendingAtTheFileLimit_ThrowsAndStagesNothingMore()
    {
        _storageConfig.MaxFilesPerUser = 2;
        Seed(UserID, "one");
        await Enqueue("two.mp3", Mp3());

        await Assert.ThrowsAsync<LibraryFullException>(() => Enqueue("three.mp3", Mp3()));

        Assert.Single(_storage.Staged);
        Assert.Single(_store.MediaUploadJobList);
        Assert.Equal(1, _signal.NotifyCount);
    }

    [Fact]
    public async Task Enqueue_FileLimitIsPerUser()
    {
        _storageConfig.MaxFilesPerUser = 1;
        Seed(UserID, "mine");

        await Enqueue("theirs.mp3", Mp3(), userID: Guid.CreateVersion7());

        Assert.Single(_store.MediaUploadJobList);
    }

    [Fact]
    public async Task Enqueue_ByteLimitAlreadyReached_ThrowsLibraryFullAndStagesNothing()
    {
        _storageConfig.MaxBytesPerUser = 150;
        Seed(UserID, "full", 150);

        await Assert.ThrowsAsync<LibraryFullException>(() => Enqueue("media.mp3", Mp3(10)));

        AssertNothingQueued();
    }

    [Fact]
    public async Task Enqueue_PendingJobsCountTowardsTheByteLimit()
    {
        _storageConfig.MaxBytesPerUser = 150;
        Seed(UserID, "mine", 50);
        SeedPendingJob(UserID, 100);

        await Assert.ThrowsAsync<LibraryFullException>(() => Enqueue("media.mp3", Mp3(10)));

        Assert.Empty(_storage.Staged);
    }

    [Fact]
    public async Task Enqueue_FileLargerThanTheRemainingQuota_ThrowsLibraryFullAndLeavesNothingStaged()
    {
        _storageConfig.MaxBytesPerUser = 150;
        Seed(UserID, "mine");

        await Assert.ThrowsAsync<LibraryFullException>(() => Enqueue("media.mp3", Mp3(51)));

        AssertNothingQueued();
    }

    [Fact]
    public async Task Enqueue_FileExactlyFillingTheQuota_Succeeds()
    {
        _storageConfig.MaxBytesPerUser = 150;
        Seed(UserID, "mine");

        await Enqueue("media.mp3", Mp3(50));

        Assert.Single(_store.MediaUploadJobList);
    }

    [Fact]
    public async Task Enqueue_QuotaIsPerUser()
    {
        _storageConfig.MaxBytesPerUser = 150;
        Seed(UserID, "mine", 150);

        await Enqueue("media.mp3", Mp3(), userID: Guid.CreateVersion7());

        Assert.Single(_store.MediaUploadJobList);
    }

    [Fact]
    public async Task Enqueue_OverTheConfiguredFileSizeWithNoQuota_StillPropagatesFileTooLarge()
    {
        _storage.StageFailure = new FileTooLargeException(10);

        await Assert.ThrowsAsync<FileTooLargeException>(() => Enqueue("media.mp3", Mp3()));
    }

    [Fact]
    public async Task Enqueue_AtThePendingCap_ThrowsUploadQueueFull()
    {
        _uploadConfig.MaxPendingPerUser = 2;
        SeedPendingJob(UserID);
        SeedPendingJob(UserID);

        await Assert.ThrowsAsync<UploadQueueFullException>(() => Enqueue("media.mp3", Mp3()));

        Assert.Empty(_storage.Staged);
        Assert.Equal(2, _store.MediaUploadJobList.Count);
    }

    [Fact]
    public async Task Enqueue_PendingCapIsPerUser()
    {
        _uploadConfig.MaxPendingPerUser = 1;
        SeedPendingJob(UserID);

        await Enqueue("media.mp3", Mp3(), userID: Guid.CreateVersion7());

        Assert.Equal(2, _store.MediaUploadJobList.Count);
    }

    [Fact]
    public async Task Enqueue_CommitFails_DeletesTheStagedFileAndDoesNotNotify()
    {
        _store.FailCommit = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => Enqueue("media.mp3", Mp3()));

        Assert.Empty(_storage.Staged);
        Assert.Single(_storage.DeletedStagedList);
        Assert.Empty(_store.MediaUploadJobList);
        Assert.Equal(0, _signal.NotifyCount);
    }

    [Fact]
    public async Task Enqueue_StagingFails_LeavesNoJob()
    {
        _storage.StageFailure = new FileTooLargeException(10);

        await Assert.ThrowsAsync<FileTooLargeException>(() => Enqueue("media.mp3", Mp3()));

        AssertNothingQueued();
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(10);
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, "The condition was not met in time.");
            await Task.Delay(5, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Enqueue_WhileTheSameUsersOtherUploadIsStillArriving_ThrowsUploadInProgress()
    {
        FakeBlockingStream arriving = new(Mp3());
        Task<ResponseUploadAccepted> first = _service.Enqueue(UserID, "slow.mp3", null, arriving, TestContext.Current.CancellationToken);
        await WaitUntil(() => arriving.IsWaiting);

        await Assert.ThrowsAsync<UploadInProgressException>(() => Enqueue("second.mp3", Mp3()));
        Assert.Empty(_storage.Staged);

        arriving.Release();
        await first;
        Assert.Single(_store.MediaUploadJobList);
    }

    [Fact]
    public async Task Enqueue_AnotherUsersUploadIsNotBlockedByAnArrivingOne()
    {
        FakeBlockingStream arriving = new(Mp3());
        Task<ResponseUploadAccepted> first = _service.Enqueue(UserID, "slow.mp3", null, arriving, TestContext.Current.CancellationToken);
        await WaitUntil(() => arriving.IsWaiting);

        await Enqueue("other.mp3", Mp3(), userID: Guid.CreateVersion7());

        arriving.Release();
        await first;
        Assert.Equal(2, _store.MediaUploadJobList.Count);
    }

    [Fact]
    public async Task Enqueue_AfterOneFinishes_TheSameUsersNextIsAccepted()
    {
        await Enqueue("one.mp3", Mp3());
        await Enqueue("two.mp3", Mp3());

        Assert.Equal(2, _store.MediaUploadJobList.Count);
    }

    [Fact]
    public async Task Enqueue_FreesTheUsersSlotWhenItFails()
    {
        await Assert.ThrowsAsync<UnsupportedMediaTypeException>(() => Enqueue("bad.exe", Mp3()));
        _store.FailCommit = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Enqueue("media.mp3", Mp3()));
        _store.FailCommit = false;

        await Enqueue("good.mp3", Mp3());

        Assert.Single(_store.MediaUploadJobList);
    }

    private void AssertNothingQueued()
    {
        Assert.Empty(_storage.Staged);
        Assert.Empty(_store.MediaUploadJobList);
        Assert.Equal(0, _store.CommitCount);
        Assert.Equal(0, _signal.NotifyCount);
    }

    [Fact]
    public async Task GetMediaFileList_ReturnsOnlyTheUsersFilesNewestFirst()
    {
        DateTime now = DateTime.UtcNow;
        MediaFile older = Seed(UserID, "old", timeCreated: now.AddMinutes(-5));
        MediaFile newer = Seed(UserID, "new", timeCreated: now);
        Seed(Guid.CreateVersion7(), "other");

        List<ResponseMediaFile> list = await _service.GetMediaFileList(UserID, 0, 10, TestContext.Current.CancellationToken);

        Assert.Equal([newer.ID, older.ID], list.Select(x => x.ID));
        Assert.Equal(["new", "old"], list.Select(x => x.Title));
    }

    [Fact]
    public async Task GetMediaFileList_AppliesSkipAndTake()
    {
        DateTime now = DateTime.UtcNow;
        Seed(UserID, "media0", timeCreated: now.AddMinutes(-2));
        Seed(UserID, "media1", timeCreated: now.AddMinutes(-1));
        Seed(UserID, "media2", timeCreated: now);

        List<ResponseMediaFile> page = await _service.GetMediaFileList(UserID, 1, 1, TestContext.Current.CancellationToken);

        Assert.Equal("media1", Assert.Single(page).Title);
    }

    [Fact]
    public async Task GetMediaFileList_ReturnsTheHashAndUtcTimes()
    {
        MediaFile file = Seed(UserID, "media", timeCreated: new(2026, 1, 2, 3, 4, 5, DateTimeKind.Unspecified));

        ResponseMediaFile response = Assert.Single(await _service.GetMediaFileList(UserID, 0, 10, TestContext.Current.CancellationToken));

        Assert.Equal(file.ContentHash, response.ContentHash);
        Assert.Equal(DateTimeKind.Utc, response.TimeCreated.Kind);
        Assert.Equal(DateTimeKind.Utc, response.TimeModified.Kind);
    }

    [Fact]
    public async Task Open_ReturnsTheContentNamedAfterTheTitle()
    {
        MediaFile file = Seed(UserID, "My Media", 50);

        MediaDownload download = await _service.Open(UserID, file.ID, TestContext.Current.CancellationToken);
        await using Stream stream = download.Content;
        using MemoryStream copy = new();
        await stream.CopyToAsync(copy, TestContext.Current.CancellationToken);

        Assert.Equal(Mp3(50), copy.ToArray());
        Assert.Equal("audio/mpeg", download.ContentType);
        Assert.Equal("My Media.mp3", download.FileName);
    }

    [Fact]
    public async Task Open_ReturnsTheStoredContentHash()
    {
        MediaFile file = Seed(UserID, "My Media", 50);

        MediaDownload download = await _service.Open(UserID, file.ID, TestContext.Current.CancellationToken);
        await download.Content.DisposeAsync();

        Assert.Equal(file.ContentHash, download.ContentHash);
    }

    [Fact]
    public async Task Open_UnknownId_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<MediaNotFoundException>(() => _service.Open(UserID, Guid.CreateVersion7(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Open_AnotherUsersFile_ThrowsNotFound()
    {
        MediaFile file = Seed(UserID, "media");

        await Assert.ThrowsAsync<MediaNotFoundException>(() => _service.Open(Guid.CreateVersion7(), file.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Open_FileMissingFromStorage_ThrowsNotFound()
    {
        MediaFile file = Seed(UserID, "media");
        _storage.Files.Clear();

        await Assert.ThrowsAsync<MediaNotFoundException>(() => _service.Open(UserID, file.ID, TestContext.Current.CancellationToken));
    }
}
