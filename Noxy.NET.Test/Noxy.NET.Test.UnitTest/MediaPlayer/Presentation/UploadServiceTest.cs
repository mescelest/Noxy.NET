using System.Net;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class UploadServiceTest
{
    private readonly FakeApiClient _api = new();
    private readonly FakeSyncService _sync = new();
    private readonly FakeMetadataStore _storeMetadata = new();
    private readonly UploadService _service;
    private readonly List<UploadBatchResult> _completedList = [];

    public UploadServiceTest()
    {
        _service = new(_api, _sync, _storeMetadata);
        _service.BatchCompleted += _completedList.Add;
    }

    private static UploadFile File(string name)
    {
        return new(name, "audio/mpeg", () => new MemoryStream());
    }

    private static ResponseMediaFile Media(string title)
    {
        return new() { ID = Guid.CreateVersion7(), Title = title, ContentType = "audio/mpeg", SizeBytes = 1, TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow };
    }

    private static ResponseUploadResult Succeeded(Guid jobID, ResponseMediaFile media)
    {
        return new() { JobID = jobID, Title = media.Title, Succeeded = true, Media = media };
    }

    private static ResponseUploadResult Failed(Guid jobID, string title, UploadFailureReasonEnum? reason)
    {
        return new() { JobID = jobID, Title = title, Succeeded = false, Reason = reason };
    }

    private ResponseMediaFile Report(int index, string title)
    {
        ResponseMediaFile media = Media(title);
        _sync.RaiseUploadResult(Succeeded(_api.UploadCallList[index].JobID, media));

        return media;
    }

    #region -- Sending --

    [Fact]
    public async Task Upload_SendsEveryFileInTheOrderGiven_WithItsNameAndContentType()
    {
        await _service.Upload([File("a.mp3"), File("b.mp3"), File("c.mp3")]);

        Assert.Equal(["a.mp3", "b.mp3", "c.mp3"], _api.UploadCallList.Select(x => x.FileName));
        Assert.All(_api.UploadCallList, x => Assert.Equal("audio/mpeg", x.ContentType));
    }


    [Fact]
    public async Task Upload_ReadsEveryPickedFile_BeforeSendingTheFirst()
    {
        List<string> orderList = [];

        UploadFile Picked(string name) => new(name, "audio/mpeg", () =>
        {
            orderList.Add($"read {name}");
            return new MemoryStream([1]);
        });

        _api.UploadHandler = name =>
        {
            orderList.Add($"send {name}");
            return new() { JobID = Guid.NewGuid() };
        };

        await _service.Upload([Picked("a"), Picked("b"), Picked("c")]);

        Assert.Equal(["read a", "read b", "read c", "send a", "send b", "send c"], orderList);
    }

    [Fact]
    public async Task Upload_SendsTheBytesOfEachFile()
    {
        await _service.Upload([new("a", "audio/mpeg", () => new MemoryStream([1, 2, 3])), new("b", "audio/mpeg", () => new MemoryStream([4, 5]))]);

        Assert.Equal([[1, 2, 3], [4, 5]], _api.UploadContentList);
    }

    [Fact]
    public async Task Upload_WhenAPickedFileCannotBeRead_ReportsIt_AndSendsTheRest()
    {
        UploadFile broken = new("b", "audio/mpeg", () => throw new IOException("The file is gone."));

        await _service.Upload([File("a"), broken, File("c")]);
        Report(0, "a");
        Report(1, "c");

        UploadBatchResult result = Assert.Single(_completedList);
        Assert.Equal(2, result.SucceededCount);
        UploadFailure failure = Assert.Single(result.FailureList);
        Assert.Equal("b", failure.Name);
        Assert.Equal(["a", "c"], _api.UploadCallList.Select(x => x.FileName));
    }

    [Fact]
    public async Task Upload_WhenASendFailsUnexpectedly_ReportsIt_AndSendsTheRest()
    {
        _api.UploadHandler = name => name == "b" ? throw new IOException("The connection was reset.") : new() { JobID = Guid.NewGuid() };

        await _service.Upload([File("a"), File("b"), File("c")]);
        Report(0, "a");
        Report(1, "c");

        UploadBatchResult result = Assert.Single(_completedList);
        Assert.Equal(2, result.SucceededCount);
        Assert.Equal("b", Assert.Single(result.FailureList).Name);
    }

    [Fact]
    public async Task Upload_RemovesItsTemporaryFiles_WhenDone()
    {
        string pattern = $"{PresentationConstants.UploadStagingFolderPrefix}*";
        int before = Directory.GetDirectories(Path.GetTempPath(), pattern).Length;

        await _service.Upload([File("a"), File("b")]);

        Assert.Equal(before, Directory.GetDirectories(Path.GetTempPath(), pattern).Length);
    }

    [Fact]
    public async Task Upload_OfNoFiles_SendsNothing_AndReportsNothing()
    {
        List<int> startedList = [];
        _service.Started += startedList.Add;

        await _service.Upload([]);

        Assert.Empty(_api.UploadCallList);
        Assert.Empty(_completedList);
        Assert.Empty(startedList);
    }

    [Fact]
    public async Task Upload_SaysItStarted_WithTheNumberOfFiles_BeforeTheFirstSend()
    {
        List<(int Count, int Sent)> startedList = [];
        _service.Started += count => startedList.Add((count, _api.UploadCallList.Count));

        await _service.Upload([File("a.mp3"), File("b.mp3")]);

        Assert.Equal([(2, 0)], startedList);
    }

    [Fact]
    public async Task Upload_DoesNotReportTheBatch_UntilTheServerHasAnsweredForEveryFile()
    {
        await _service.Upload([File("a.mp3"), File("b.mp3")]);
        Report(0, "a");

        Assert.Empty(_completedList);

        Report(1, "b");

        UploadBatchResult result = Assert.Single(_completedList);
        Assert.Equal(2, result.SucceededCount);
        Assert.Empty(result.FailureList);
        Assert.Null(result.PlaylistID);
    }

    [Fact]
    public async Task FilesTheLibraryAlreadyHolds_AreCountedApart_NeitherAsAddedNorAsFailures()
    {
        await _service.Upload([File("a.mp3"), File("b.mp3"), File("c.mp3"), File("d.mp3"), File("e.mp3")]);
        Report(0, "a");
        _sync.RaiseUploadResult(Failed(_api.UploadCallList[1].JobID, "b", UploadFailureReasonEnum.Duplicate));
        Report(2, "c");
        _sync.RaiseUploadResult(Failed(_api.UploadCallList[3].JobID, "d", UploadFailureReasonEnum.Duplicate));
        Report(4, "e");

        UploadBatchResult result = Assert.Single(_completedList);
        Assert.Equal(3, result.SucceededCount);
        Assert.Equal(2, result.DuplicateCount);
        Assert.Empty(result.FailureList);
    }

    #endregion

    #region -- Failures --

    [Fact]
    public async Task AFileTheServerRefuses_IsAFailureWithTheServersTitle_AndTheRestStillGo()
    {
        _api.UploadHandler = name => name == "big.mp3" ? throw new ApiException(HttpStatusCode.RequestEntityTooLarge, "The file is too big.") : new() { JobID = Guid.NewGuid() };

        await _service.Upload([File("big.mp3"), File("ok.mp3")]);
        Report(0, "ok");

        UploadBatchResult result = Assert.Single(_completedList);
        Assert.Equal(1, result.SucceededCount);
        UploadFailure failure = Assert.Single(result.FailureList);
        Assert.Equal(("big.mp3", "The file is too big."), (failure.Name, failure.Message));
    }

    [Fact]
    public async Task WhenTheServerCannotBeReached_EveryFileFails_WithTheOfflineSentence_AndTheBatchEnds()
    {
        _api.UploadHandler = _ => throw new HttpRequestException("down");

        await _service.Upload([File("a.mp3"), File("b.mp3")]);

        UploadBatchResult result = Assert.Single(_completedList);
        Assert.Equal(0, result.SucceededCount);
        Assert.Equal(["a.mp3", "b.mp3"], result.FailureList.Select(x => x.Name));
        Assert.All(result.FailureList, x => Assert.Equal(PresentationTexts.ActionMessageOffline, x.Message));
    }

    // The texts come from the resource file, which an attribute cannot hold.
    public static TheoryData<UploadFailureReasonEnum?, string> ReasonCaseList => new()
    {
        { UploadFailureReasonEnum.LibraryFull, PresentationTexts.UploadMessageLibraryFull },
        { UploadFailureReasonEnum.FileMissing, PresentationTexts.UploadMessageFileMissing },
        { UploadFailureReasonEnum.Unknown, PresentationTexts.UploadMessageUnknown },
        { null, PresentationTexts.UploadMessageUnknown }
    };

    [Theory]
    [MemberData(nameof(ReasonCaseList))]
    public async Task AFileTheServerLaterRejects_FailsWithTheReasonInWords(UploadFailureReasonEnum? reason, string expected)
    {
        await _service.Upload([File("a.mp3")]);

        _sync.RaiseUploadResult(Failed(_api.UploadCallList[0].JobID, "A song", reason));

        UploadFailure failure = Assert.Single(Assert.Single(_completedList).FailureList);
        Assert.Equal(("A song", expected), (failure.Name, failure.Message));
    }

    #endregion

    #region -- Results --

    [Fact]
    public async Task ARunThatIsReportedBeforeTheAnswerToTheRequestReaches_IsStillCounted()
    {
        _api.UploadHandler = _ =>
        {
            Guid jobID = Guid.NewGuid();
            _sync.RaiseUploadResult(Succeeded(jobID, Media("fast")));

            return new() { JobID = jobID };
        };

        await _service.Upload([File("fast.mp3")]);

        Assert.Equal(1, Assert.Single(_completedList).SucceededCount);
    }

    [Fact]
    public async Task AResultForAnotherDevicesUpload_IsIgnored()
    {
        await _service.Upload([File("a.mp3")]);

        _sync.RaiseUploadResult(Succeeded(Guid.NewGuid(), Media("theirs")));

        Assert.Empty(_completedList);
    }

    [Fact]
    public async Task AResultForAnotherDevicesUpload_ArrivingMidUpload_DoesNotStayAroundToBeMistaken()
    {
        Guid foreign = Guid.NewGuid();
        _api.UploadHandler = _ =>
        {
            _sync.RaiseUploadResult(Succeeded(foreign, Media("theirs")));

            return new() { JobID = Guid.NewGuid() };
        };
        await _service.Upload([File("a.mp3")]);

        _api.UploadHandler = _ => new() { JobID = foreign };
        await _service.Upload([File("b.mp3")]);

        Assert.Empty(_completedList);
    }

    #endregion

    #region -- To a playlist --

    [Fact]
    public async Task AnUploadToAPlaylist_AddsEachStoredFileToIt_AndReportsThePlaylist()
    {
        Guid playlistID = Guid.NewGuid();
        await _service.Upload([File("a.mp3"), File("b.mp3")], playlistID);

        ResponseMediaFile first = Report(0, "a");
        ResponseMediaFile second = Report(1, "b");

        Assert.Equal([playlistID, playlistID], _api.AddEntryCallList.Select(x => x.PlaylistID));
        Assert.Equal([first.ID, second.ID], _api.AddEntryCallList.SelectMany(x => x.Request.MediaFileIDList));
        UploadBatchResult result = Assert.Single(_completedList);
        Assert.Equal((playlistID, 2), (result.PlaylistID, result.SucceededCount));
    }

    private async Task<Guid> AddPlaylist(bool allowDuplicates, params Guid[] mediaFileIDList)
    {
        Guid playlistID = Guid.NewGuid();
        List<ResponsePlaylistEntry> entryList = [];
        foreach (Guid mediaFileID in mediaFileIDList) entryList.Add(new() { ID = Guid.NewGuid(), MediaFileID = mediaFileID, Position = entryList.Count, MediaFile = Media("held") });

        await _storeMetadata.SavePlaylistDetail(new()
        {
            ID = playlistID, Name = "List", AllowDuplicates = allowDuplicates, EntryCount = entryList.Count, TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow, TimeLastPlayed = null, EntryList = entryList
        });

        return playlistID;
    }

    // The server's answer for a file the library already holds: it names that file.
    private void ReportDuplicate(int index, ResponseMediaFile existing)
    {
        _sync.RaiseUploadResult(new() { JobID = _api.UploadCallList[index].JobID, Title = existing.Title, Succeeded = false, Reason = UploadFailureReasonEnum.Duplicate, Media = existing });
    }

    [Fact]
    public async Task AFileTheLibraryAlreadyHolds_SentToAPlaylist_GoesIntoThePlaylistAndNotIntoTheLibraryAgain()
    {
        ResponseMediaFile existing = Media("held");
        Guid playlistID = await AddPlaylist(true);
        await _service.Upload([File("new.mp3"), File("held.mp3")], playlistID);

        ResponseMediaFile added = Report(0, "new");
        ReportDuplicate(1, existing);

        Assert.Equal([added.ID, existing.ID], _api.AddEntryCallList.SelectMany(x => x.Request.MediaFileIDList));
        UploadBatchResult result = Assert.Single(_completedList);
        Assert.Equal((1, 1, 1), (result.SucceededCount, result.DuplicateCount, result.ExistingAddedCount));
        Assert.Empty(result.FailureList);
    }

    [Fact]
    public async Task AFileTheLibraryAlreadyHolds_SentToAPlaylistThatAlreadyHasItAndTakesNoDuplicates_IsLeftOutWithAReason()
    {
        ResponseMediaFile existing = Media("held");
        Guid playlistID = await AddPlaylist(false, existing.ID);
        await _service.Upload([File("held.mp3")], playlistID);

        ReportDuplicate(0, existing);

        Assert.Empty(_api.AddEntryCallList);
        UploadBatchResult result = Assert.Single(_completedList);
        Assert.Equal((0, 0, 0), (result.SucceededCount, result.DuplicateCount, result.ExistingAddedCount));
        Assert.Equal(PresentationTexts.UploadMessageAlreadyInPlaylist, Assert.Single(result.FailureList).Message);
    }

    [Fact]
    public async Task AFileTheLibraryAlreadyHolds_SentToAPlaylistThatTakesDuplicates_IsAddedEvenIfTheListHasIt()
    {
        ResponseMediaFile existing = Media("held");
        Guid playlistID = await AddPlaylist(true, existing.ID);
        await _service.Upload([File("held.mp3")], playlistID);

        ReportDuplicate(0, existing);

        Assert.Equal([existing.ID], _api.AddEntryCallList.SelectMany(x => x.Request.MediaFileIDList));
        Assert.Equal(1, Assert.Single(_completedList).ExistingAddedCount);
    }

    [Fact]
    public async Task AnUploadToTheLibrary_AddsNothingToAnyPlaylist()
    {
        await _service.Upload([File("a.mp3")]);

        Report(0, "a");

        Assert.Empty(_api.AddEntryCallList);
    }

    [Fact]
    public async Task AFileThatIsStoredButCannotBeAddedToThePlaylist_IsAFailureThatSaysItIsInTheLibrary()
    {
        _api.AddEntryFailure = new ApiException(HttpStatusCode.NotFound, "Not found.");
        await _service.Upload([File("a.mp3")], Guid.NewGuid());

        Report(0, "A song");

        UploadBatchResult result = Assert.Single(_completedList);
        Assert.Equal(0, result.SucceededCount);
        UploadFailure failure = Assert.Single(result.FailureList);
        Assert.Equal(("A song", PresentationTexts.UploadMessageNotAddedToPlaylist), (failure.Name, failure.Message));
    }

    [Fact]
    public async Task AFileThatIsRejected_IsNotAddedToThePlaylist()
    {
        await _service.Upload([File("a.mp3")], Guid.NewGuid());

        _sync.RaiseUploadResult(Failed(_api.UploadCallList[0].JobID, "A song", UploadFailureReasonEnum.LibraryFull));

        Assert.Empty(_api.AddEntryCallList);
        Assert.Single(Assert.Single(_completedList).FailureList);
    }

    #endregion
}
