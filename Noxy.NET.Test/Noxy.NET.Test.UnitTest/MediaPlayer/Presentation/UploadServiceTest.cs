using System.Net;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class UploadServiceTest
{
    private readonly FakeApiClient _api = new();
    private readonly FakeSyncService _sync = new();
    private readonly UploadService _service;
    private readonly List<UploadBatchResult> _completedList = [];

    public UploadServiceTest()
    {
        _service = new(_api, _sync);
        _service.BatchCompleted += _completedList.Add;
    }

    private static UploadFile File(string name)
    {
        return new(name, "audio/mpeg", () => new MemoryStream());
    }

    private static ResponseMediaFile Media(string title)
    {
        return new() { ID = Guid.CreateVersion7(), Title = title, ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = "h", TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow };
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
        Assert.All(result.FailureList, x => Assert.Equal(Texts.ActionMessageOffline, x.Message));
    }

    // The texts come from the resource file, which an attribute cannot hold.
    public static TheoryData<UploadFailureReasonEnum?, string> ReasonCaseList => new()
    {
        { UploadFailureReasonEnum.LibraryFull, Texts.UploadMessageLibraryFull },
        { UploadFailureReasonEnum.FileMissing, Texts.UploadMessageFileMissing },
        { UploadFailureReasonEnum.Unknown, Texts.UploadMessageUnknown },
        { null, Texts.UploadMessageUnknown }
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
        Assert.Equal(("A song", Texts.UploadMessageNotAddedToPlaylist), (failure.Name, failure.Message));
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
