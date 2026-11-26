using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FileReconcilerTest
{
    private readonly FakeFileStore _storeFile = new();
    private readonly FakeMetadataStore _storeMetadata = new();
    private readonly FakeDownloadService _serviceDownload = new();
    private readonly FileReconciler _reconciler;

    public FileReconcilerTest()
    {
        _reconciler = new(_storeFile, _storeMetadata, _serviceDownload);
    }

    private static ResponseMediaFile Media()
    {
        return new()
        {
            ID = Guid.CreateVersion7(),
            Title = "Media",
            ContentType = "audio/mpeg",
            SizeBytes = 3,
            TimeCreated = DateTime.UtcNow,
            TimeModified = DateTime.UtcNow
        };
    }

    // A media file in the library whose bytes are on the device.
    private async Task<ResponseMediaFile> OnDevice()
    {
        ResponseMediaFile media = Media();
        await _storeMetadata.SaveMediaFile(media);
        _storeFile.Hold(media.ID, [1, 2, 3]);
        await _storeMetadata.SaveFileState(new(media.ID, FileSyncStateEnum.OnDevice, null));

        return media;
    }

    private async Task AddPartial(Guid id)
    {
        await _storeFile.AppendPartial(id, new MemoryStream([9, 9]));
    }

    private async Task<FileState?> State(Guid id)
    {
        return (await _storeMetadata.GetFileStateList()).SingleOrDefault(x => x.MediaFileID == id);
    }

    [Fact]
    public async Task Apply_ARemovedMediaFile_LosesItsFilePartialStateAndRunningDownload()
    {
        ResponseMediaFile media = await OnDevice();
        await AddPartial(media.ID);

        await _reconciler.Apply(new([], [media.ID]));

        Assert.Equal([media.ID], _serviceDownload.CancelList);
        Assert.Null(await _storeFile.GetPath(media.ID));
        Assert.Empty(await _storeFile.GetPartialIDList());
        Assert.Null(await State(media.ID));
    }

    [Fact]
    public async Task Apply_AnAddedMediaFile_ChangesNothing()
    {
        ResponseMediaFile other = await OnDevice();
        ResponseMediaFile added = Media();

        await _reconciler.Apply(new([added], []));

        Assert.Empty(_serviceDownload.CancelList);
        Assert.NotNull(await _storeFile.GetPath(other.ID));
        Assert.Equal(FileSyncStateEnum.OnDevice, (await State(other.ID))?.State);
        Assert.Null(await State(added.ID));
    }

    [Fact]
    public async Task Apply_LeavesTheOtherMediaFilesAlone()
    {
        ResponseMediaFile removed = await OnDevice();
        ResponseMediaFile kept = await OnDevice();

        await _reconciler.Apply(new([], [removed.ID]));

        Assert.NotNull(await _storeFile.GetPath(kept.ID));
        Assert.Equal(FileSyncStateEnum.OnDevice, (await State(kept.ID))?.State);
    }

    [Fact]
    public async Task Repair_AnOnDeviceStateWhoseFileIsMissing_BecomesNotDownloaded()
    {
        ResponseMediaFile media = Media();
        await _storeMetadata.SaveMediaFile(media);
        await _storeMetadata.SaveFileState(new(media.ID, FileSyncStateEnum.OnDevice, null));

        await _reconciler.Repair();

        Assert.Null(await State(media.ID));
        Assert.Null(await _storeFile.GetPath(media.ID));
    }

    [Fact]
    public async Task Repair_AHeldFileWithNoState_IsDeleted()
    {
        ResponseMediaFile media = Media();
        await _storeMetadata.SaveMediaFile(media);
        _storeFile.Hold(media.ID, [1, 2, 3]);

        await _reconciler.Repair();

        Assert.Empty(await _storeFile.GetHeldIDList());
    }

    [Fact]
    public async Task Repair_AHeldFileWhoseMediaFileIsGone_IsDeleted_WithItsState()
    {
        Guid id = Guid.CreateVersion7();
        _storeFile.Hold(id, [1, 2, 3]);
        await _storeMetadata.SaveFileState(new(id, FileSyncStateEnum.OnDevice, null));

        await _reconciler.Repair();

        Assert.Empty(await _storeFile.GetHeldIDList());
        Assert.Null(await State(id));
    }

    [Fact]
    public async Task Repair_AHeldFileWithAnOnDeviceState_IsLeftAlone()
    {
        ResponseMediaFile media = await OnDevice();

        await _reconciler.Repair();

        Assert.NotNull(await _storeFile.GetPath(media.ID));
        Assert.Equal(new(media.ID, FileSyncStateEnum.OnDevice, null), await State(media.ID));
    }

    [Fact]
    public async Task Repair_KeepsAHeldFileWhoseMediaFileWasModified()
    {
        ResponseMediaFile media = await OnDevice();
        ResponseMediaFile modified = new() { ID = media.ID, Title = "Renamed", ContentType = media.ContentType, SizeBytes = media.SizeBytes, TimeCreated = media.TimeCreated, TimeModified = media.TimeModified.AddMinutes(5) };
        await _storeMetadata.SaveMediaFile(modified);

        await _reconciler.Repair();

        Assert.NotNull(await _storeFile.GetPath(media.ID));
        Assert.Equal(new(media.ID, FileSyncStateEnum.OnDevice, null), await State(media.ID));
    }

    [Fact]
    public async Task Repair_EveryPartialFileIsDeleted_AndADownloadingStateLeftByAnEarlierRunIsReset()
    {
        ResponseMediaFile media = Media();
        await _storeMetadata.SaveMediaFile(media);
        await AddPartial(media.ID);
        await AddPartial(Guid.CreateVersion7());
        await _storeMetadata.SaveFileState(new(media.ID, FileSyncStateEnum.Downloading, null));

        await _reconciler.Repair();

        Assert.Empty(await _storeFile.GetPartialIDList());
        Assert.Null(await State(media.ID));
    }

    [Fact]
    public async Task Repair_AFailedState_IsKept_ItStillSaysWhyTheDownloadFailed()
    {
        ResponseMediaFile media = Media();
        await _storeMetadata.SaveMediaFile(media);
        FileState failed = new(media.ID, FileSyncStateEnum.Failed, "The server no longer has this media file.");
        await _storeMetadata.SaveFileState(failed);

        await _reconciler.Repair();

        Assert.Equal(failed, await State(media.ID));
    }

    [Fact]
    public async Task Repair_OnAnEmptyStore_ChangesNothing()
    {
        await _reconciler.Repair();

        Assert.Empty(await _storeFile.GetHeldIDList());
        Assert.Empty(await _storeFile.GetPartialIDList());
        Assert.Empty(await _storeMetadata.GetFileStateList());
        Assert.Empty(_serviceDownload.CancelList);
    }
}
