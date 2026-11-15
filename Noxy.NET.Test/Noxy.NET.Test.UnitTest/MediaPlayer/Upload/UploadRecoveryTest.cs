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

public class UploadRecoveryTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly FakeFileStorageService _storage = new();
    private readonly FakePublisher _publisher = new();
    private readonly UploadProcessor _processor;

    private List<(Guid UserID, ResponseUploadResult Result)> NotificationList => [.. _publisher.GetList<NotificationUploadSucceeded>().Select(x => (x.UserID, x.Result)), .. _publisher.GetList<NotificationUploadFailed>().Select(x => (x.UserID, x.Result))];

    public UploadRecoveryTest()
    {
        ConfigStorage config = new() { RootPath = "x", AllowedExtensionList = ["mp3"] };
        _processor = new(new FakeUnitOfWorkFactory(_store), _storage, _publisher, Options.Create(config), NullLogger<UploadProcessor>.Instance);
    }

    private EntityMediaUploadJob AddJob(MediaUploadJobStatus status, bool staged = true)
    {
        EntityMediaUploadJob job = new()
        {
            UserID = UserID,
            StagedFileName = $"{Guid.CreateVersion7()}.mp3",
            Title = "Media",
            ContentType = "audio/mpeg",
            SizeBytes = 10,
            ContentHash = new('c', 64),
            Status = status
        };

        _store.MediaUploadJobList.Add(job);
        if (staged) _storage.Staged[job.StagedFileName] = new byte[10];
        return job;
    }

    [Fact]
    public async Task Recover_ResetsProcessingJobsToPending()
    {
        AddJob(MediaUploadJobStatus.Processing);
        AddJob(MediaUploadJobStatus.Pending);

        await _processor.Recover(TestContext.Current.CancellationToken);

        Assert.All(_store.MediaUploadJobList, job => Assert.Equal(MediaUploadJobStatus.Pending, job.Status));
    }

    [Fact]
    public async Task Recover_DeletesStagedFilesWithNoJob()
    {
        _storage.Staged["orphan.mp3"] = [1];

        await _processor.Recover(TestContext.Current.CancellationToken);

        Assert.Empty(_storage.Staged);
        Assert.Contains("orphan.mp3", _storage.DeletedStagedList);
    }

    [Fact]
    public async Task Recover_KeepsStagedFilesThatHaveAJob()
    {
        EntityMediaUploadJob job = AddJob(MediaUploadJobStatus.Pending);
        _storage.Staged["orphan.mp3"] = [1];

        await _processor.Recover(TestContext.Current.CancellationToken);

        Assert.Equal([job.StagedFileName], _storage.Staged.Keys);
    }

    [Fact]
    public async Task Recover_NothingToRecover_DoesNothing()
    {
        await _processor.Recover(TestContext.Current.CancellationToken);

        Assert.Empty(_storage.DeletedStagedList);
        Assert.Empty(NotificationList);
    }

    [Fact]
    public async Task Recover_PromotedButUncommittedJob_CompletesOnTheNextProcessNext()
    {
        // The crash happened after the file was moved into the library but before the commit.
        EntityMediaUploadJob job = AddJob(MediaUploadJobStatus.Processing, staged: false);
        _storage.Files[job.StagedFileName] = new byte[10];

        await _processor.Recover(TestContext.Current.CancellationToken);
        Assert.True(await _processor.ProcessNext(TestContext.Current.CancellationToken));

        Assert.Equal(job.StagedFileName, Assert.Single(_store.MediaFileList).FilePath);
        Assert.Empty(_store.MediaUploadJobList);
        Assert.True(Assert.Single(NotificationList).Result.Succeeded);
        Assert.Equal([job.StagedFileName], _storage.Files.Keys);
    }

    [Fact]
    public async Task Recover_ThenProcessNext_ProcessesAJobThatWasInterrupted()
    {
        EntityMediaUploadJob job = AddJob(MediaUploadJobStatus.Processing);

        await _processor.Recover(TestContext.Current.CancellationToken);
        await _processor.ProcessNext(TestContext.Current.CancellationToken);

        Assert.Equal(job.StagedFileName, Assert.Single(_store.MediaFileList).FilePath);
    }
}
