using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Handlers.Media;
using Noxy.NET.MediaPlayer.Application.Models.Upload;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class HandlerMediaUploadTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly ConfigStorage _storageConfig = new() { RootPath = "x", AllowedExtensionList = ["mp3", "flac"] };
    private readonly ConfigUpload _uploadConfig = new();
    private readonly FakeFileStorageService _storage = new();
    private readonly FakeUploadSignal _signal = new();
    private readonly FakeStore _store = new();
    private readonly HandlerMediaUpload _handler;

    public HandlerMediaUploadTest()
    {
        IOptions<ConfigStorage> storageOptions = Options.Create(_storageConfig);
        _handler = new(new FakeUnitOfWorkFactory(_store), new MediaFileValidationService(storageOptions), _storage, _signal, storageOptions, Options.Create(_uploadConfig), new UploadGate());
    }

    private static byte[] Mp3(int length = 100)
    {
        byte[] bytes = new byte[length];
        "ID3"u8.CopyTo(bytes);
        return bytes;
    }

    private Task<ResponseUploadAccepted> Enqueue(string fileName, byte[] content, string? title = null, Guid? userID = null)
    {
        return _handler.Handle(new(userID ?? UserID, fileName, title, new MemoryStream(content)), TestContext.Current.CancellationToken).AsTask();
    }

    // A file that is already in the library, as the worker would have left it.
    private EntityMediaFile Seed(Guid userID, string title, int length = 100, DateTime? timeCreated = null)
    {
        byte[] bytes = Mp3(length);
        EntityMediaFile file = new()
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

        EntityMediaUploadJob job = Assert.Single(_store.MediaUploadJobList);
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
    public async Task Enqueue_ATitleSent_IsRememberedAsSent()
    {
        await Enqueue("media.mp3", Mp3(), "My media");

        Assert.True(Assert.Single(_store.MediaUploadJobList).TitleWasSent);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Enqueue_NoTitleSent_IsRememberedAsNotSent(string? title)
    {
        await Enqueue("media.mp3", Mp3(), title);

        EntityMediaUploadJob job = Assert.Single(_store.MediaUploadJobList);
        Assert.False(job.TitleWasSent);
        Assert.Equal("media", job.Title);
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
        Task<ResponseUploadAccepted> first = _handler.Handle(new(UserID, "slow.mp3", null, arriving), TestContext.Current.CancellationToken).AsTask();
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
        Task<ResponseUploadAccepted> first = _handler.Handle(new(UserID, "slow.mp3", null, arriving), TestContext.Current.CancellationToken).AsTask();
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

    [Fact]
    public async Task Upload_Cancelled_ReleasesTheUsersSlotAndStagesNothing()
    {
        using CancellationTokenSource cancelled = new();
        await cancelled.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _handler.Handle(new(UserID, "media.mp3", null, new MemoryStream(Mp3())), cancelled.Token).AsTask());
        AssertNothingQueued();

        await Enqueue("next.mp3", Mp3());

        Assert.Single(_store.MediaUploadJobList);
    }

    [Fact]
    public async Task Upload_DoesNotDisposeTheContentStream()
    {
        bool disposed = false;
        await using FakeDisposeCallbackStream accepted = new(Mp3(), () => disposed = true);

        await _handler.Handle(new(UserID, "media.mp3", null, accepted), TestContext.Current.CancellationToken);
        Assert.False(disposed);

        await using FakeDisposeCallbackStream rejected = new(Mp3(), () => disposed = true);

        await Assert.ThrowsAsync<UnsupportedMediaTypeException>(() => _handler.Handle(new(UserID, "media.exe", null, rejected), TestContext.Current.CancellationToken).AsTask());
        Assert.False(disposed);
    }

    private void AssertNothingQueued()
    {
        Assert.Empty(_storage.Staged);
        Assert.Empty(_store.MediaUploadJobList);
        Assert.Equal(0, _store.CommitCount);
        Assert.Equal(0, _signal.NotifyCount);
    }
}
