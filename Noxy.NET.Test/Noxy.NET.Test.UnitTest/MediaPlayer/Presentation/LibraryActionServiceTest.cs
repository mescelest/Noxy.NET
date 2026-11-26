using System.Net;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class LibraryActionServiceTest
{
    private readonly FakeDownloadService _download = new();
    private readonly FakeQueueService _queue = new();
    private readonly FakePlaybackControlService _control = new();
    private readonly FakeApiClient _api = new();
    private readonly FakeSyncService _sync = new();
    private readonly FakeMetadataStore _store = new();
    private readonly FakeFileSaver _saver = new();
    private readonly FakeFileStore _files = new();
    private readonly FakeFolderOpener _folder = new();
    private readonly FakeFileTagService _fileTag = new();
    private readonly LibraryActionService _service;

    public LibraryActionServiceTest()
    {
        _service = new(_download, _queue, _control, _api, _sync, _store, _saver, _files, _folder, _fileTag);
    }

    private static List<Guid> Ids(int count)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Guid.CreateVersion7())];
    }

    [Fact]
    public async Task Download_QueuesTheListForTheBulkDownload_WhichSkipsTheOnesAlreadyOnTheDevice()
    {
        List<Guid> idList = Ids(3);

        BulkResult result = await _service.Download(idList);

        Assert.Equal(idList, _download.QueueMissingList);
        Assert.Equal(3, result.SucceededCount);
        Assert.Empty(result.FailedIDList);
    }

    [Fact]
    public async Task RemoveFromDevice_RemovesEachFromTheDownloadService_AndNeverCallsTheServer()
    {
        List<Guid> idList = Ids(2);

        BulkResult result = await _service.RemoveFromDevice(idList);

        Assert.Equal(idList.Order(), _download.RemoveList.Order());
        Assert.Equal(2, result.SucceededCount);
        Assert.Empty(_api.DeleteMediaList);
    }

    [Fact]
    public async Task RemoveFromDevice_OfThePlayingMediaFile_StopsThePlayerFirst_SoTheFileIsReleased()
    {
        Guid mediaID = Guid.CreateVersion7();
        QueueItem playing = new(Guid.CreateVersion7(), mediaID);
        _queue.Queue = LiveQueue.From(new()
        {
            QueueRevision = 1,
            Source = null,
            HistoryList = [],
            Current = playing,
            UserQueueList = [],
            SourceList = []
        }, 50);
        List<string> order = [];
        _control.RemoveFromQueueHandler = id => order.Add($"queue {id}");
        _download.RemoveHandler = id => order.Add($"file {id}");

        await _service.RemoveFromDevice([mediaID]);

        Assert.Equal([$"queue {playing.ItemID}", $"file {mediaID}"], order);
    }

    [Fact]
    public async Task RemoveFromDevice_TakesEveryQueuedCopyOutOfTheQueue()
    {
        Guid mediaID = Guid.CreateVersion7();
        QueueItem inHistory = new(Guid.CreateVersion7(), mediaID);
        QueueItem inUserQueue = new(Guid.CreateVersion7(), mediaID);
        QueueItem other = new(Guid.CreateVersion7(), Guid.CreateVersion7());
        _queue.Queue = LiveQueue.From(new()
        {
            QueueRevision = 1,
            Source = null,
            HistoryList = [inHistory],
            Current = other,
            UserQueueList = [inUserQueue],
            SourceList = []
        }, 50);

        await _service.RemoveFromDevice([mediaID]);

        Assert.Equal([$"RemoveFromQueue {inHistory.ItemID}", $"RemoveFromQueue {inUserQueue.ItemID}"], _control.CallList);
    }

    [Fact]
    public async Task DeleteFromLibrary_DeletesEachOnTheServer_AndReportsTheOnesThatFailed()
    {
        List<Guid> idList = Ids(3);
        _api.DeleteMediaFailure = id => id == idList[1] ? new ApiException(HttpStatusCode.NotFound, "The media file was not found.") : null;

        BulkResult result = await _service.DeleteFromLibrary(idList);

        Assert.Equal(idList, _api.DeleteMediaList);
        Assert.Equal(2, result.SucceededCount);
        Assert.Equal([idList[1]], result.FailedIDList);
        Assert.Equal("The media file was not found.", result.Message);
    }

    [Fact]
    public async Task DeleteFromLibrary_WhenOffline_DeletesNothing_AndSaysWhy()
    {
        _sync.IsOffline = true;
        List<Guid> idList = Ids(2);

        BulkResult result = await _service.DeleteFromLibrary(idList);

        Assert.Empty(_api.DeleteMediaList);
        Assert.Equal(0, result.SucceededCount);
        Assert.Equal(idList, result.FailedIDList);
        Assert.Equal(PresentationTexts.ActionMessageOffline, result.Message);
    }

    [Fact]
    public async Task DeleteFromLibrary_WhenTheNetworkFails_ReportsTheOfflineSentence_NotTheExceptionText()
    {
        _api.DeleteMediaFailure = _ => new HttpRequestException("connect to http://10.0.0.5 failed");

        BulkResult result = await _service.DeleteFromLibrary(Ids(1));

        Assert.Equal(PresentationTexts.ActionMessageOffline, result.Message);
    }

    [Fact]
    public async Task AddToPlaylist_SendsOneRequestWithAllTheMediaFiles()
    {
        Guid playlistID = Guid.CreateVersion7();
        List<Guid> idList = Ids(3);

        BulkResult result = await _service.AddToPlaylist(playlistID, idList);

        (Guid PlaylistID, RequestAddPlaylistEntry Request) call = Assert.Single(_api.AddEntryCallList);
        Assert.Equal(playlistID, call.PlaylistID);
        Assert.Equal(idList, call.Request.MediaFileIDList);
        Assert.Equal(3, result.SucceededCount);
    }

    [Fact]
    public async Task AddToPlaylist_ForAFullPlaylist_ReportsTheServersMessage()
    {
        _api.AddEntryFailure = new ApiException(HttpStatusCode.BadRequest, "The playlist is full.");
        List<Guid> idList = Ids(2);

        BulkResult result = await _service.AddToPlaylist(Guid.CreateVersion7(), idList);

        Assert.Equal(0, result.SucceededCount);
        Assert.Equal(idList, result.FailedIDList);
        Assert.Equal("The playlist is full.", result.Message);
    }

    [Fact]
    public async Task AddToPlaylist_WhenOffline_SendsNothing()
    {
        _sync.IsOffline = true;

        BulkResult result = await _service.AddToPlaylist(Guid.CreateVersion7(), Ids(1));

        Assert.Empty(_api.AddEntryCallList);
        Assert.Equal(PresentationTexts.ActionMessageOffline, result.Message);
    }

    [Theory]
    [InlineData(LibraryActionEnum.DeleteFromLibrary)]
    [InlineData(LibraryActionEnum.AddToPlaylist)]
    [InlineData(LibraryActionEnum.SaveAsFile)]
    [InlineData(LibraryActionEnum.Edit)]
    public void GetDisabledReason_IsNullOnline_AndTheOfflineSentenceOffline(LibraryActionEnum action)
    {
        Assert.Null(_service.GetDisabledReason(action));

        _sync.IsOffline = true;

        Assert.Equal(PresentationTexts.ActionMessageOffline, _service.GetDisabledReason(action));
    }

    [Theory]
    [InlineData(LibraryActionEnum.Download)]
    [InlineData(LibraryActionEnum.RemoveFromDevice)]
    [InlineData(LibraryActionEnum.OpenFolder)]
    [InlineData(LibraryActionEnum.UpdateFileMetadata)]
    public void GetDisabledReason_IsNullOffline_ForTheLocalActions(LibraryActionEnum action)
    {
        _sync.IsOffline = true;

        Assert.Null(_service.GetDisabledReason(action));
    }

    [Fact]
    public async Task OpenContainingFolder_ShowsTheFileOfTheFirstMediaFileThatIsOnTheDevice()
    {
        List<Guid> idList = Ids(3);
        _files.Hold(idList[1], [1]);
        _files.Hold(idList[2], [2]);

        BulkResult result = await _service.OpenContainingFolder(idList);

        Assert.Equal(await _files.GetPath(idList[1]), Assert.Single(_folder.RevealedList));
        Assert.Equal(1, result.SucceededCount);
        Assert.Empty(result.FailedIDList);
    }

    [Fact]
    public async Task OpenContainingFolder_WithNothingOnTheDevice_ShowsNothing_AndFailsTheWholeList()
    {
        List<Guid> idList = Ids(2);

        BulkResult result = await _service.OpenContainingFolder(idList);

        Assert.Empty(_folder.RevealedList);
        Assert.Equal(0, result.SucceededCount);
        Assert.Equal(idList, result.FailedIDList);
        Assert.Equal(PresentationTexts.ActionMessageNotOnDevice, result.Message);
    }

    [Fact]
    public async Task OpenContainingFolder_WithAnEmptyList_DoesNothing()
    {
        BulkResult result = await _service.OpenContainingFolder([]);

        Assert.Empty(_folder.RevealedList);
        Assert.Equal(0, result.SucceededCount);
        Assert.Empty(result.FailedIDList);
    }

    [Fact]
    public async Task OpenContainingFolder_WhenTheFileManagerCannotBeOpened_FailsTheList_WithTheOpenMessage()
    {
        List<Guid> idList = Ids(1);
        _files.Hold(idList[0], [1]);
        _folder.Failure = new IOException("no explorer");

        BulkResult result = await _service.OpenContainingFolder(idList);

        Assert.Equal(0, result.SucceededCount);
        Assert.Equal(idList, result.FailedIDList);
        Assert.Equal(PresentationTexts.ActionMessageOpenFolderFailed, result.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CanOpenFolder_FollowsWhetherThePlatformCanShowAFolder(bool supported)
    {
        _folder.IsSupported = supported;

        Assert.Equal(supported, _service.CanOpenFolder);
    }

    [Fact]
    public async Task PlayNext_KeepsTheOrderOfTheList_ByPuttingTheLastOneInFirst()
    {
        List<Guid> idList = Ids(3);

        BulkResult result = await _service.PlayNext(idList);

        Assert.Equal([$"PlayNext {idList[2]}", $"PlayNext {idList[1]}", $"PlayNext {idList[0]}"], _control.CallList);
        Assert.Equal(3, result.SucceededCount);
    }

    [Fact]
    public async Task AddToQueue_AddsInTheOrderOfTheList()
    {
        List<Guid> idList = Ids(2);

        await _service.AddToQueue(idList);

        Assert.Equal([$"AddToQueue {idList[0]}", $"AddToQueue {idList[1]}"], _control.CallList);
    }

    [Theory]
    [InlineData(LibraryActionEnum.PlayNext)]
    [InlineData(LibraryActionEnum.AddToQueue)]
    public void TheQueueActions_WorkOffline(LibraryActionEnum action)
    {
        _sync.IsOffline = true;

        Assert.Null(_service.GetDisabledReason(action));
    }

    private async Task<Guid> AddMedia(string title, string contentType = "audio/mpeg")
    {
        Guid id = Guid.CreateVersion7();
        await _store.SaveMediaFile(new() { ID = id, Title = title, ContentType = contentType, SizeBytes = 1, TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow });
        _api.TaggedFileHandler = file => Task.FromResult(new ApiFileResponse(HttpStatusCode.OK, new MemoryStream(file.ToByteArray()), null, 16));

        return id;
    }

    [Fact]
    public async Task SaveAsFile_StreamsEachMediaFileToTheSaver_UsingItsTitleAsTheSuggestedName()
    {
        Guid first = await AddMedia("First: song/1");
        Guid second = await AddMedia("Second", "audio/flac");

        BulkResult result = await _service.SaveAsFile([first, second]);

        Assert.Equal(["First_ song_1.mp3", "Second.flac"], _saver.SavedList.Select(x => x.Name));
        Assert.Equal(first.ToByteArray(), _saver.SavedList[0].Bytes);
        Assert.Equal(2, result.SucceededCount);
        Assert.Empty(result.FailedIDList);
    }

    [Fact]
    public async Task SaveAsFile_NamesAFileLikeTheDownloaderDoes_WithTheSameExtensionForAnUnknownType()
    {
        Guid id = await AddMedia("Mystery", "audio/x-unknown");

        await _service.SaveAsFile([id]);

        Assert.Equal("Mystery.bin", Assert.Single(_saver.SavedList).Name);
    }

    [Fact]
    public async Task SaveAsFile_WhenTheUserCancels_IsNotAFailure_AndAsksNoMore()
    {
        Guid first = await AddMedia("First");
        Guid second = await AddMedia("Second");
        _saver.AnswerList.Add(false);

        BulkResult result = await _service.SaveAsFile([first, second]);

        Assert.Single(_saver.AskedList);
        Assert.Equal(0, result.SucceededCount);
        Assert.Empty(result.FailedIDList);
        Assert.Null(result.Message);
    }

    [Fact]
    public async Task SaveAsFile_WhenOffline_SavesNothing_AndSaysWhy()
    {
        _sync.IsOffline = true;
        Guid id = await AddMedia("First");

        BulkResult result = await _service.SaveAsFile([id]);

        Assert.Empty(_saver.AskedList);
        Assert.Empty(_api.TaggedFileRequestList);
        Assert.Equal(PresentationTexts.ActionMessageOffline, result.Message);
    }

    [Fact]
    public async Task SaveAsFile_WhenTheServerRefuses_ReportsItAsFailed_AndGoesOn()
    {
        Guid first = await AddMedia("First");
        Guid second = await AddMedia("Second");
        Func<Guid, Task<ApiFileResponse>> ok = _api.TaggedFileHandler;
        _api.TaggedFileHandler = id => id == first ? throw new ApiException(HttpStatusCode.NotFound, "The media file was not found.") : ok(id);

        BulkResult result = await _service.SaveAsFile([first, second]);

        Assert.Equal(1, result.SucceededCount);
        Assert.Equal([first], result.FailedIDList);
        Assert.Equal("The media file was not found.", result.Message);
    }

    [Fact]
    public async Task AnEmptyIdList_ChangesNothing_AndReportsSuccessWithZero()
    {
        _sync.IsOffline = true;

        foreach (BulkResult result in new[] { await _service.Download([]), await _service.RemoveFromDevice([]), await _service.DeleteFromLibrary([]), await _service.AddToPlaylist(Guid.CreateVersion7(), []) })
        {
            Assert.Equal(0, result.SucceededCount);
            Assert.Empty(result.FailedIDList);
            Assert.Null(result.Message);
        }

        Assert.Empty(_download.QueueMissingList);
        Assert.Empty(_download.RemoveList);
        Assert.Empty(_api.DeleteMediaList);
        Assert.Empty(_api.AddEntryCallList);
    }

    [Fact]
    public async Task UpdateFileMetadata_ReturnsTheResultOfTheFileTagService()
    {
        Guid id = Guid.CreateVersion7();
        _fileTag.Result = new(1, [], null);

        BulkResult result = await _service.UpdateFileMetadata([id]);

        Assert.Equal(1, result.SucceededCount);
        Assert.Equal([[id]], _fileTag.UpdateList);
    }

    [Fact]
    public async Task UpdateFileMetadata_OfAnEmptyList_DoesNothing()
    {
        BulkResult result = await _service.UpdateFileMetadata([]);

        Assert.Equal(0, result.SucceededCount);
        Assert.Empty(_fileTag.UpdateList);
    }

    [Fact]
    public async Task UpdateAllFiles_UpdatesTheOutdatedFiles()
    {
        _fileTag.Result = new(4, [], null);

        BulkResult result = await _service.UpdateAllFiles();

        Assert.Equal(4, result.SucceededCount);
        Assert.Equal(1, _fileTag.UpdateOutdatedCount);
    }
}
