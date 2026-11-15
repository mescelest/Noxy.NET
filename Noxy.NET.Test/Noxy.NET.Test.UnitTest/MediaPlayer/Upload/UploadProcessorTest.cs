using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Models.Upload;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Upload;

public class UploadProcessorTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly ConfigStorage _config = new() { RootPath = "x", AllowedExtensionList = ["mp3"] };
    private readonly FakeStore _store = new();
    private readonly FakeFileStorageService _storage = new();
    private readonly FakePublisher _publisher = new();
    private readonly UploadProcessor _processor;

    private List<(Guid UserID, ResponseUploadResult Result)> NotificationList => [.. _publisher.GetList<NotificationUploadSucceeded>().Select(x => (x.UserID, x.Result)), .. _publisher.GetList<NotificationUploadFailed>().Select(x => (x.UserID, x.Result))];

    public UploadProcessorTest()
    {
        _processor = new(new FakeUnitOfWorkFactory(_store), _storage, _publisher, Options.Create(_config), NullLogger<UploadProcessor>.Instance);
    }

    // A job as Enqueue leaves it, with its file waiting in staging.
    private EntityMediaUploadJob AddJob(Guid? userID = null, long size = 100, string title = "Media", DateTime? timeCreated = null, bool staged = true)
    {
        EntityMediaUploadJob job = new()
        {
            TimeCreated = timeCreated ?? DateTime.UtcNow,
            UserID = userID ?? UserID,
            StagedFileName = $"{Guid.CreateVersion7()}.mp3",
            Title = title,
            ContentType = "audio/mpeg",
            SizeBytes = size,
            ContentHash = new string('c', 64)
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
            ContentHash = new string('d', 64),
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
        Assert.Equal(UploadFailureReason.LibraryFull, Assert.Single(_publisher.GetList<NotificationUploadFailed>()).Result.Reason);
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
        Assert.Equal(UploadFailureReason.LibraryFull, result.Reason);
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
        Assert.Equal(UploadFailureReason.FileMissing, result.Reason);
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
        Assert.Equal(UploadFailureReason.Unknown, result.Reason);
    }

    [Fact]
    public async Task ProcessNext_UnexpectedError_FailsWithUnknown_AndCleansUp()
    {
        EntityMediaUploadJob job = AddJob();
        _storage.PromoteFailure = new IOException("disk gone");

        await ProcessNext();

        Assert.Equal(UploadFailureReason.Unknown, Assert.Single(NotificationList).Result.Reason);
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

        Assert.Equal(MediaUploadJobStatus.Processing, Assert.Single(_store.MediaUploadJobList).Status);
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
        Assert.Equal(UploadFailureReason.LibraryFull, NotificationList[1].Result.Reason);
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
}
