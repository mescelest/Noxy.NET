using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Upload;

public class UploadRecoveryTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakeFileStorageService _storage = new();
    private readonly FakeUploadNotifier _notifier = new();
    private readonly UploadProcessor _processor;

    public UploadRecoveryTest()
    {
        ConfigStorage config = new() { RootPath = "x", AllowedExtensions = ["mp3"] };
        _processor = new(new FakeUnitOfWorkFactory(_store), _storage, _notifier, Options.Create(config), NullLogger<UploadProcessor>.Instance);
    }

    private MediaUploadJob AddJob(MediaUploadJobStatus status = MediaUploadJobStatus.Pending, bool staged = true)
    {
        MediaUploadJob job = new()
        {
            UserID = UserID,
            StagedFileName = $"{Guid.CreateVersion7()}.mp3",
            Title = "Song",
            ContentType = "audio/mpeg",
            SizeBytes = 10,
            ContentHash = new string('c', 64),
            Status = status
        };

        _store.MediaUploadJobs.Add(job);
        if (staged) _storage.Staged[job.StagedFileName] = new byte[10];
        return job;
    }

    [Fact]
    public async Task Recover_ResetsProcessingJobsToPending()
    {
        AddJob(MediaUploadJobStatus.Processing);
        AddJob(MediaUploadJobStatus.Pending);

        await _processor.Recover(TestContext.Current.CancellationToken);

        Assert.All(_store.MediaUploadJobs, job => Assert.Equal(MediaUploadJobStatus.Pending, job.Status));
    }

    [Fact]
    public async Task Recover_DeletesStagedFilesWithNoJob()
    {
        _storage.Staged["orphan.mp3"] = [1];

        await _processor.Recover(TestContext.Current.CancellationToken);

        Assert.Empty(_storage.Staged);
        Assert.Contains("orphan.mp3", _storage.DeletedStaged);
    }

    [Fact]
    public async Task Recover_KeepsStagedFilesThatHaveAJob()
    {
        MediaUploadJob job = AddJob();
        _storage.Staged["orphan.mp3"] = [1];

        await _processor.Recover(TestContext.Current.CancellationToken);

        Assert.Equal([job.StagedFileName], _storage.Staged.Keys);
    }

    [Fact]
    public async Task Recover_NothingToRecover_DoesNothing()
    {
        await _processor.Recover(TestContext.Current.CancellationToken);

        Assert.Empty(_storage.DeletedStaged);
        Assert.Empty(_notifier.Notifications);
    }

    [Fact]
    public async Task Recover_PromotedButUncommittedJob_CompletesOnTheNextProcessNext()
    {
        // The crash happened after the file was moved into the library but before the commit.
        MediaUploadJob job = AddJob(MediaUploadJobStatus.Processing, staged: false);
        _storage.Files[job.StagedFileName] = new byte[10];

        await _processor.Recover(TestContext.Current.CancellationToken);
        Assert.True(await _processor.ProcessNext(TestContext.Current.CancellationToken));

        Assert.Equal(job.StagedFileName, Assert.Single(_store.MediaFiles).FilePath);
        Assert.Empty(_store.MediaUploadJobs);
        Assert.True(Assert.Single(_notifier.Notifications).Result.Succeeded);
        Assert.Equal([job.StagedFileName], _storage.Files.Keys);
    }

    [Fact]
    public async Task Recover_ThenProcessNext_ProcessesAJobThatWasInterrupted()
    {
        MediaUploadJob job = AddJob(MediaUploadJobStatus.Processing);

        await _processor.Recover(TestContext.Current.CancellationToken);
        await _processor.ProcessNext(TestContext.Current.CancellationToken);

        Assert.Equal(job.StagedFileName, Assert.Single(_store.MediaFiles).FilePath);
    }
}
