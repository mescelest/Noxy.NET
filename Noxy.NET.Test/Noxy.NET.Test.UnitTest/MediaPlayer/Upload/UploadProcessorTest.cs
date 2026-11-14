using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Upload;

public class UploadProcessorTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly ConfigStorage _config = new() { RootPath = "x", AllowedExtensions = ["mp3"] };
    private readonly FakeStore _store = new();
    private readonly FakeFileStorageService _storage = new();
    private readonly FakeUploadNotifier _notifier = new();
    private readonly UploadProcessor _processor;

    public UploadProcessorTest()
    {
        _processor = new(new FakeUnitOfWorkFactory(_store), _storage, _notifier, Options.Create(_config), NullLogger<UploadProcessor>.Instance);
    }

    // A job as Enqueue leaves it, with its file waiting in staging.
    private MediaUploadJob AddJob(Guid? userID = null, long size = 100, string title = "Song", DateTime? timeCreated = null, bool staged = true)
    {
        MediaUploadJob job = new()
        {
            TimeCreated = timeCreated ?? DateTime.UtcNow,
            UserID = userID ?? UserID,
            StagedFileName = $"{Guid.CreateVersion7()}.mp3",
            Title = title,
            ContentType = "audio/mpeg",
            SizeBytes = size,
            ContentHash = new string('c', 64)
        };

        _store.MediaUploadJobs.Add(job);
        if (staged) _storage.Staged[job.StagedFileName] = new byte[size];
        return job;
    }

    private void AddLibraryFile(Guid userID, long size)
    {
        _store.MediaFiles.Add(new()
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

        Assert.Empty(_notifier.Notifications);
    }

    [Fact]
    public async Task ProcessNext_Success_CreatesMediaFileAndMetadataAndDeletesTheJob()
    {
        MediaUploadJob job = AddJob(title: "My Song");

        Assert.True(await ProcessNext());

        MediaFile file = Assert.Single(_store.MediaFiles);
        Assert.Equal(job.StagedFileName, file.FilePath);
        Assert.Equal("audio/mpeg", file.ContentType);
        Assert.Equal(100, file.SizeBytes);
        Assert.Equal(job.ContentHash, file.ContentHash);
        Assert.Equal(UserID, file.UserID);
        Assert.Equal("My Song", file.Metadata?.Title);
        Assert.Equal(file.ID, file.Metadata?.MediaFileID);
        Assert.Empty(_store.MediaUploadJobs);
        Assert.Contains(job.StagedFileName, _storage.Files.Keys);
        Assert.Empty(_storage.Staged);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task ProcessNext_Success_NotifiesTheOwnerOnce()
    {
        MediaUploadJob job = AddJob(title: "My Song");

        await ProcessNext();

        (Guid userID, ResponseUploadResult result) = Assert.Single(_notifier.Notifications);
        Assert.Equal(UserID, userID);
        Assert.True(result.Succeeded);
        Assert.Equal(job.ID, result.JobID);
        Assert.Equal("My Song", result.Title);
        Assert.Equal(Assert.Single(_store.MediaFiles).ID, result.Media?.ID);
        Assert.Null(result.Reason);
    }

    [Fact]
    public async Task ProcessNext_ProcessesOldestFirst()
    {
        DateTime now = DateTime.UtcNow;
        MediaUploadJob newer = AddJob(title: "newer", timeCreated: now);
        MediaUploadJob older = AddJob(title: "older", timeCreated: now.AddMinutes(-5));

        await ProcessNext();

        Assert.Equal(older.StagedFileName, Assert.Single(_store.MediaFiles).FilePath);
        Assert.Equal(newer.ID, Assert.Single(_store.MediaUploadJobs).ID);
    }

    [Fact]
    public async Task ProcessNext_AtTheFileLimit_FailsWithLibraryFull_DeletesStagedAndJob_AndNotifies()
    {
        _config.MaxFilesPerUser = 1;
        AddLibraryFile(UserID, 10);
        MediaUploadJob job = AddJob();

        Assert.True(await ProcessNext());

        Assert.Single(_store.MediaFiles);
        Assert.Empty(_store.MediaUploadJobs);
        Assert.Empty(_storage.Staged);
        Assert.Contains(job.StagedFileName, _storage.DeletedStaged);
        (Guid userID, ResponseUploadResult result) = Assert.Single(_notifier.Notifications);
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

        Assert.Equal(succeeds, Assert.Single(_notifier.Notifications).Result.Succeeded);
        Assert.Equal(succeeds ? 2 : 1, _store.MediaFiles.Count);
    }

    [Fact]
    public async Task ProcessNext_ByteLimitIsPerUser()
    {
        _config.MaxBytesPerUser = 150;
        AddLibraryFile(Guid.CreateVersion7(), 150);
        AddJob(size: 100);

        await ProcessNext();

        Assert.True(Assert.Single(_notifier.Notifications).Result.Succeeded);
    }

    [Fact]
    public async Task ProcessNext_StagedAndLibraryFileBothMissing_FailsWithFileMissing()
    {
        AddJob(staged: false);

        await ProcessNext();

        ResponseUploadResult result = Assert.Single(_notifier.Notifications).Result;
        Assert.False(result.Succeeded);
        Assert.Equal(UploadFailureReason.FileMissing, result.Reason);
        Assert.Empty(_store.MediaFiles);
        Assert.Empty(_store.MediaUploadJobs);
    }

    [Fact]
    public async Task ProcessNext_FileAlreadyPromoted_StillCompletes()
    {
        MediaUploadJob job = AddJob();
        _storage.Files[job.StagedFileName] = _storage.Staged[job.StagedFileName];
        _storage.Staged.Clear();

        await ProcessNext();

        Assert.Equal(job.StagedFileName, Assert.Single(_store.MediaFiles).FilePath);
        Assert.Empty(_store.MediaUploadJobs);
        Assert.True(Assert.Single(_notifier.Notifications).Result.Succeeded);
    }

    [Fact]
    public async Task ProcessNext_CommitFailsAfterPromote_DeletesTheLibraryFileAndFailsTheJob()
    {
        MediaUploadJob job = AddJob();
        _store.FailCommitCount = 1;

        await ProcessNext();

        Assert.Empty(_store.MediaFiles);
        Assert.Empty(_store.MediaUploadJobs);
        Assert.Empty(_storage.Files);
        Assert.Contains(job.StagedFileName, _storage.Deleted);
        ResponseUploadResult result = Assert.Single(_notifier.Notifications).Result;
        Assert.False(result.Succeeded);
        Assert.Equal(UploadFailureReason.Unknown, result.Reason);
    }

    [Fact]
    public async Task ProcessNext_UnexpectedError_FailsWithUnknown_AndCleansUp()
    {
        MediaUploadJob job = AddJob();
        _storage.PromoteFailure = new IOException("disk gone");

        await ProcessNext();

        Assert.Equal(UploadFailureReason.Unknown, Assert.Single(_notifier.Notifications).Result.Reason);
        Assert.Empty(_store.MediaUploadJobs);
        Assert.Contains(job.StagedFileName, _storage.DeletedStaged);
    }

    [Fact]
    public async Task ProcessNext_NotifierThrows_JobStillCompletes()
    {
        AddJob();
        _notifier.Failure = new InvalidOperationException("no connection");

        Assert.True(await ProcessNext());

        Assert.Single(_store.MediaFiles);
        Assert.Empty(_store.MediaUploadJobs);
    }

    [Fact]
    public async Task ProcessNext_NotifierThrowsOnAFailedJob_JobIsStillRemoved()
    {
        _config.MaxFilesPerUser = 1;
        AddLibraryFile(UserID, 10);
        AddJob();
        _notifier.Failure = new InvalidOperationException("no connection");

        Assert.True(await ProcessNext());

        Assert.Empty(_store.MediaUploadJobs);
        Assert.Empty(_storage.Staged);
    }

    [Fact]
    public async Task ProcessNext_Cancelled_RethrowsAndLeavesTheJobProcessing()
    {
        MediaUploadJob job = AddJob();
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();
        _storage.PromoteFailure = new OperationCanceledException(cts.Token);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ProcessNext(cts.Token));

        Assert.Equal(MediaUploadJobStatus.Processing, Assert.Single(_store.MediaUploadJobs).Status);
        Assert.Contains(job.StagedFileName, _storage.Staged.Keys);
        Assert.Empty(_notifier.Notifications);
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

        Assert.Equal("first", Assert.Single(_store.MediaFiles).Metadata?.Title);
        Assert.Equal([true, false], _notifier.Notifications.Select(x => x.Result.Succeeded));
        Assert.Equal(UploadFailureReason.LibraryFull, _notifier.Notifications[1].Result.Reason);
        Assert.Empty(_store.MediaUploadJobs);
    }

    [Fact]
    public async Task ProcessNext_ClaimLostToAnotherWorker_ReturnsTrueAndDoesNothing()
    {
        AddJob();
        _store.LoseJobClaim = true;

        Assert.True(await ProcessNext());

        Assert.Empty(_store.MediaFiles);
        Assert.Single(_store.MediaUploadJobs);
        Assert.Empty(_notifier.Notifications);
    }

    [Fact]
    public async Task ProcessNext_FailureCleanup_StagedDeleteThrows_StillDeletesTheLibraryFile()
    {
        MediaUploadJob job = AddJob();
        _store.FailCommitCount = 1;
        _storage.DeleteStagedFailure = new IOException("locked");

        await ProcessNext();

        Assert.Contains(job.StagedFileName, _storage.Deleted);
        Assert.Empty(_storage.Files);
        Assert.Empty(_store.MediaUploadJobs);
    }

    [Fact]
    public async Task ProcessNext_JobCannotBeRemoved_KeepsTheFilesAndStaysSilentSoARestartCanRetry()
    {
        MediaUploadJob job = AddJob();
        // The commit that finishes the job fails, and then so does the commit that removes it after the failure.
        _store.FailCommitCount = 2;

        Assert.True(await ProcessNext());

        Assert.Single(_store.MediaUploadJobs);
        Assert.Contains(job.StagedFileName, _storage.Files.Keys);
        Assert.Empty(_storage.Deleted);
        Assert.Empty(_storage.DeletedStaged);
        Assert.Empty(_notifier.Notifications);
    }

    [Fact]
    public async Task ProcessNext_JobCannotBeRemoved_ARestartThenCompletesIt()
    {
        MediaUploadJob job = AddJob();
        _store.FailCommitCount = 2;
        await ProcessNext();

        await _processor.Recover(TestContext.Current.CancellationToken);
        await ProcessNext();

        Assert.Equal(job.StagedFileName, Assert.Single(_store.MediaFiles).FilePath);
        Assert.Empty(_store.MediaUploadJobs);
        Assert.True(Assert.Single(_notifier.Notifications).Result.Succeeded);
    }
}
