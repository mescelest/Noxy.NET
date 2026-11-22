using System.Net;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class DownloadServiceTest
{
    private const string ETag = "\"file-v1\"";

    private readonly FakeApiClient _clientApi = new();
    private readonly FakeFileStore _storeFile = new();
    private readonly FakeMetadataStore _storeMetadata = new();
    private readonly FakeDelay _delay = new();
    private readonly FakeLogger<DownloadService> _logger = new();
    private readonly DownloadService _serviceDownload;

    public DownloadServiceTest()
    {
        _serviceDownload = new(_clientApi, _storeFile, new HashService(), _storeMetadata, _delay, _logger);
    }

    private static byte[] Bytes(int length, byte seed = 1)
    {
        byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++) bytes[i] = (byte)(i * 13 + seed);

        return bytes;
    }

    private static string Hash(byte[] bytes)
    {
        return Convert.ToHexStringLower(SHA256.HashData(bytes));
    }

    private async Task<ResponseMediaFile> AddMedia(byte[] bytes, string contentType = "audio/mpeg")
    {
        ResponseMediaFile media = new()
        {
            ID = Guid.CreateVersion7(),
            Title = "Media",
            ContentType = contentType,
            SizeBytes = bytes.Length,
            ContentHash = Hash(bytes),
            TimeCreated = DateTime.UtcNow,
            TimeModified = DateTime.UtcNow
        };
        List<ResponseMediaFile> list = await _storeMetadata.GetMediaFileList();
        await _storeMetadata.ReplaceMediaFileList([.. list, media]);

        return media;
    }

    private static Task<ApiFileResponse> Whole(byte[] bytes, string eTag = ETag)
    {
        return Task.FromResult(new ApiFileResponse(HttpStatusCode.OK, new MemoryStream(bytes), eTag, bytes.Length));
    }

    private static Task<ApiFileResponse> Rest(byte[] bytes, long from, string eTag = ETag)
    {
        return Task.FromResult(new ApiFileResponse(HttpStatusCode.PartialContent, new MemoryStream(bytes[(int)from..]), eTag, bytes.Length));
    }

    private static Task<ApiFileResponse> Interrupted(byte[] head, Exception failure, string eTag = ETag)
    {
        return Task.FromResult(new ApiFileResponse(HttpStatusCode.OK, new FakeInterruptedStream(head, failure), eTag, null));
    }

    private async Task<FileState?> State(Guid id)
    {
        return (await _storeMetadata.GetFileStateList()).SingleOrDefault(x => x.MediaFileID == id);
    }

    [Fact]
    public async Task EnsureOnDevice_DownloadsVerifiesAndStores_ThenStateIsOnDeviceWithTheVerifiedHash()
    {
        byte[] bytes = Bytes(10_000);
        ResponseMediaFile media = await AddMedia(bytes, "audio/flac");
        _clientApi.FileHandler = (_, _, _) => Whole(bytes);

        bool result = await _serviceDownload.EnsureOnDevice(media.ID);

        Assert.True(result);
        Assert.Equal(new FileState(media.ID, FileSyncStateEnum.OnDevice, media.ContentHash, null), await State(media.ID));
        string? path = await _storeFile.GetPath(media.ID);
        Assert.NotNull(path);
        Assert.EndsWith(".flac", path);
        Assert.Equal(bytes, _storeFile.ReadPath(path));
        Assert.Empty(await _storeFile.GetPartialIDList());
        Assert.Equal([(media.ID, null, null)], _clientApi.FileRequestList);
    }

    [Fact]
    public async Task EnsureOnDevice_WhenAlreadyOnDeviceWithTheSameHash_DoesNotDownload()
    {
        byte[] bytes = Bytes(500);
        ResponseMediaFile media = await AddMedia(bytes);
        _storeFile.Hold(media.ID, bytes);
        await _storeMetadata.SaveFileState(new(media.ID, FileSyncStateEnum.OnDevice, media.ContentHash, null));

        bool result = await _serviceDownload.EnsureOnDevice(media.ID);

        Assert.True(result);
        Assert.Empty(_clientApi.FileRequestList);
    }

    [Fact]
    public async Task EnsureOnDevice_WhenOnDeviceWithADifferentHash_DownloadsAgain()
    {
        byte[] oldBytes = Bytes(500);
        byte[] newBytes = Bytes(600, 7);
        ResponseMediaFile media = await AddMedia(newBytes);
        _storeFile.Hold(media.ID, oldBytes);
        await _storeMetadata.SaveFileState(new(media.ID, FileSyncStateEnum.OnDevice, Hash(oldBytes), null));
        _clientApi.FileHandler = (_, _, _) => Whole(newBytes);

        bool result = await _serviceDownload.EnsureOnDevice(media.ID);

        Assert.True(result);
        Assert.Single(_clientApi.FileRequestList);
        Assert.Equal(media.ContentHash, (await State(media.ID))?.VerifiedHash);
        Assert.Equal(newBytes, _storeFile.ReadPath(await _storeFile.GetPath(media.ID) ?? string.Empty));
    }

    [Fact]
    public async Task EnsureOnDevice_WhenTheStateSaysOnDeviceButTheFileIsGone_DownloadsAgain()
    {
        byte[] bytes = Bytes(500);
        ResponseMediaFile media = await AddMedia(bytes);
        await _storeMetadata.SaveFileState(new(media.ID, FileSyncStateEnum.OnDevice, media.ContentHash, null));
        _clientApi.FileHandler = (_, _, _) => Whole(bytes);

        bool result = await _serviceDownload.EnsureOnDevice(media.ID);

        Assert.True(result);
        Assert.Single(_clientApi.FileRequestList);
        Assert.NotNull(await _storeFile.GetPath(media.ID));
    }

    [Fact]
    public async Task EnsureOnDevice_WhenTheHashDoesNotMatch_StoresNothing_DiscardsThePartial_AndEndsFailedAfterMaxAttempts()
    {
        ResponseMediaFile media = await AddMedia(Bytes(1000));
        _clientApi.FileHandler = (_, _, _) => Whole(Bytes(1000, 99));

        bool result = await _serviceDownload.EnsureOnDevice(media.ID);

        Assert.False(result);
        Assert.Equal(PresentationConstants.MaxDownloadAttempts, _clientApi.FileRequestList.Count);
        Assert.Empty(await _storeFile.GetHeldIDList());
        Assert.Empty(await _storeFile.GetPartialIDList());
        FileState? state = await State(media.ID);
        Assert.Equal(FileSyncStateEnum.Failed, state?.State);
        Assert.False(string.IsNullOrWhiteSpace(state?.Error));
        Assert.Null(state.VerifiedHash);
    }

    [Fact]
    public async Task EnsureOnDevice_WhenTheHashMatchesOnTheSecondTry_EndsOnDevice()
    {
        byte[] bytes = Bytes(1000);
        ResponseMediaFile media = await AddMedia(bytes);
        int calls = 0;
        _clientApi.FileHandler = (_, _, _) => ++calls == 1 ? Whole(Bytes(1000, 99)) : Whole(bytes);

        bool result = await _serviceDownload.EnsureOnDevice(media.ID);

        Assert.True(result);
        Assert.Equal(2, _clientApi.FileRequestList.Count);
        Assert.Equal(FileSyncStateEnum.OnDevice, (await State(media.ID))?.State);
    }

    [Fact]
    public async Task EnsureOnDevice_RetriesATransientFailure_WithGrowingDelays()
    {
        byte[] bytes = Bytes(1000);
        ResponseMediaFile media = await AddMedia(bytes);
        int calls = 0;
        _clientApi.FileHandler = (_, _, _) => ++calls < 3 ? throw new HttpRequestException("The network is down.") : Whole(bytes);

        bool result = await _serviceDownload.EnsureOnDevice(media.ID);

        Assert.True(result);
        Assert.Equal([TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4)], _delay.DelayList);
    }

    [Fact]
    public async Task EnsureOnDevice_WhenEveryTryFails_EndsFailed_WithNoDelayAfterTheLastTry()
    {
        ResponseMediaFile media = await AddMedia(Bytes(1000));
        _clientApi.FileHandler = (_, _, _) => throw new HttpRequestException("The network is down.");

        bool result = await _serviceDownload.EnsureOnDevice(media.ID);

        Assert.False(result);
        Assert.Equal(PresentationConstants.MaxDownloadAttempts, _clientApi.FileRequestList.Count);
        Assert.Equal([TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4)], _delay.DelayList);
        Assert.Equal(FileSyncStateEnum.Failed, (await State(media.ID))?.State);
        Assert.Empty(await _storeFile.GetPartialIDList());
    }

    [Fact]
    public async Task EnsureOnDevice_WhenTheServerDoesNotHaveTheMediaFile_EndsFailedAtOnce_WithoutRetrying()
    {
        ResponseMediaFile media = await AddMedia(Bytes(1000));
        _clientApi.FileHandler = (_, _, _) => throw new ApiException(HttpStatusCode.NotFound, "The media file was not found.");

        bool result = await _serviceDownload.EnsureOnDevice(media.ID);

        Assert.False(result);
        Assert.Single(_clientApi.FileRequestList);
        Assert.Empty(_delay.DelayList);
        Assert.Equal(FileSyncStateEnum.Failed, (await State(media.ID))?.State);
    }

    [Fact]
    public async Task EnsureOnDevice_AServerError_IsRetried()
    {
        byte[] bytes = Bytes(1000);
        ResponseMediaFile media = await AddMedia(bytes);
        int calls = 0;
        _clientApi.FileHandler = (_, _, _) => ++calls == 1 ? throw new ApiException(HttpStatusCode.InternalServerError, "Broken.") : Whole(bytes);

        bool result = await _serviceDownload.EnsureOnDevice(media.ID);

        Assert.True(result);
        Assert.Equal(2, _clientApi.FileRequestList.Count);
    }

    [Fact]
    public async Task EnsureOnDevice_AnInterruptedDownload_ResumesFromThePartialWithRangeAndIfRange()
    {
        byte[] bytes = Bytes(10_000);
        ResponseMediaFile media = await AddMedia(bytes);
        int calls = 0;
        _clientApi.FileHandler = (_, from, _) => ++calls == 1 ? Interrupted(bytes[..4000], new HttpRequestException("The connection was cut.")) : Rest(bytes, from ?? 0);

        bool result = await _serviceDownload.EnsureOnDevice(media.ID);

        Assert.True(result);
        Assert.Equal([(media.ID, null, null), (media.ID, 4000L, ETag)], _clientApi.FileRequestList);
        Assert.Equal(bytes, _storeFile.ReadPath(await _storeFile.GetPath(media.ID) ?? string.Empty));
    }

    [Fact]
    public async Task EnsureOnDevice_WhenARangedRequestReturns200_DiscardsThePartialBytes()
    {
        byte[] newBytes = Bytes(8000, 42);
        ResponseMediaFile media = await AddMedia(newBytes);
        int calls = 0;
        _clientApi.FileHandler = (_, _, _) => ++calls == 1 ? Interrupted(Bytes(3000), new HttpRequestException("The connection was cut.")) : Whole(newBytes, "\"file-v2\"");

        bool result = await _serviceDownload.EnsureOnDevice(media.ID);

        Assert.True(result);
        Assert.Equal(3000, _clientApi.FileRequestList[1].RangeStart);
        Assert.Equal(newBytes, _storeFile.ReadPath(await _storeFile.GetPath(media.ID) ?? string.Empty));
    }

    [Fact]
    public async Task EnsureOnDevice_ConcurrentCallsForTheSameMediaFile_DownloadOnce()
    {
        byte[] bytes = Bytes(1000);
        ResponseMediaFile media = await AddMedia(bytes);
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _clientApi.FileHandler = async (_, _, _) =>
        {
            await gate.Task;
            return await Whole(bytes);
        };

        List<Task<bool>> callList = [.. Enumerable.Range(0, 5).Select(_ => _serviceDownload.EnsureOnDevice(media.ID))];
        await Task.Delay(50, TestContext.Current.CancellationToken);
        gate.SetResult();
        bool[] resultList = await Task.WhenAll(callList);

        Assert.All(resultList, x => Assert.True(x));
        Assert.Single(_clientApi.FileRequestList);
    }

    [Fact]
    public async Task AtMostThreeDownloadsRunAtOnce()
    {
        List<ResponseMediaFile> mediaList = [];
        Dictionary<Guid, byte[]> bytesMap = [];
        for (int i = 0; i < 10; i++)
        {
            byte[] bytes = Bytes(500, (byte)(i + 1));
            ResponseMediaFile media = await AddMedia(bytes);
            mediaList.Add(media);
            bytesMap[media.ID] = bytes;
        }

        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _clientApi.FileHandler = async (id, _, _) =>
        {
            await gate.Task;
            return await Whole(bytesMap[id]);
        };

        List<Task<bool>> callList = [.. mediaList.Select(x => _serviceDownload.EnsureOnDevice(x.ID))];
        await Task.Delay(150, TestContext.Current.CancellationToken);

        Assert.Equal(PresentationConstants.MaxParallelDownloads, _clientApi.FileRequestList.Count);

        gate.SetResult();
        bool[] resultList = await Task.WhenAll(callList);

        Assert.All(resultList, x => Assert.True(x));
        Assert.Equal(10, _clientApi.FileRequestList.Count);
    }

    [Fact]
    public async Task Remove_OfAHeldFile_DeletesTheFileAndItsState_AndRaisesChanged()
    {
        byte[] bytes = Bytes(1000);
        ResponseMediaFile media = await AddMedia(bytes);
        _clientApi.FileHandler = (_, _, _) => Whole(bytes);
        Assert.True(await _serviceDownload.EnsureOnDevice(media.ID));
        int changed = 0;
        _serviceDownload.Changed += () => changed++;

        await _serviceDownload.Remove(media.ID);

        Assert.Empty(await _storeFile.GetHeldIDList());
        Assert.Null(await State(media.ID));
        Assert.True(changed >= 1);
    }

    [Fact]
    public async Task Remove_OfARunningDownload_StopsItFirst_AndLeavesNothingBehind()
    {
        byte[] bytes = Bytes(10_000);
        ResponseMediaFile media = await AddMedia(bytes);
        _clientApi.FileHandler = (_, _, _) => Task.FromResult(new ApiFileResponse(HttpStatusCode.OK, new FakeInterruptedStream(bytes[..2000], null, true), ETag, bytes.Length));
        Task<bool> download = _serviceDownload.EnsureOnDevice(media.ID);
        await WaitUntil(async () => await _storeFile.GetPartialLength(media.ID) > 0);

        await _serviceDownload.Remove(media.ID);

        Assert.False(await download);
        Assert.Empty(await _storeFile.GetPartialIDList());
        Assert.Empty(await _storeFile.GetHeldIDList());
        Assert.Null(await State(media.ID));
    }

    [Fact]
    public async Task Remove_OfAMediaFileThatIsNotOnTheDevice_IsHarmless()
    {
        await _serviceDownload.Remove(Guid.NewGuid());
    }

    [Fact]
    public async Task Cancel_StopsARunningDownload_DiscardsThePartial_AndLeavesNotDownloaded()
    {
        byte[] bytes = Bytes(10_000);
        ResponseMediaFile media = await AddMedia(bytes);
        _clientApi.FileHandler = (_, _, _) => Task.FromResult(new ApiFileResponse(HttpStatusCode.OK, new FakeInterruptedStream(bytes[..2000], null, true), ETag, bytes.Length));
        Task<bool> download = _serviceDownload.EnsureOnDevice(media.ID);
        await WaitUntil(async () => await _storeFile.GetPartialLength(media.ID) > 0);

        await _serviceDownload.Cancel(media.ID);

        Assert.False(await download);
        Assert.Empty(await _storeFile.GetPartialIDList());
        Assert.Empty(await _storeFile.GetHeldIDList());
        Assert.Null(await State(media.ID));
    }

    [Fact]
    public async Task Cancel_ADownloadStillWaitingForAFreePlace_NeverStarts()
    {
        List<ResponseMediaFile> mediaList = [];
        for (int i = 0; i < 4; i++) mediaList.Add(await AddMedia(Bytes(500, (byte)(i + 1))));

        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _clientApi.FileHandler = async (_, _, _) =>
        {
            await gate.Task;
            return await Whole(Bytes(500));
        };
        List<Task<bool>> callList = [.. mediaList.Select(x => _serviceDownload.EnsureOnDevice(x.ID))];
        await WaitUntil(() => Task.FromResult(_clientApi.FileRequestList.Count == PresentationConstants.MaxParallelDownloads));
        ResponseMediaFile waiting = mediaList.Single(x => _clientApi.FileRequestList.All(r => r.ID != x.ID));

        await _serviceDownload.Cancel(waiting.ID);
        gate.SetResult();
        await Task.WhenAll(callList);

        Assert.DoesNotContain(_clientApi.FileRequestList, x => x.ID == waiting.ID);
        Assert.Null(await State(waiting.ID));
    }

    [Fact]
    public async Task EnsureOnDevice_WhenTheDiskIsFull_EndsFailedWithAClearError_AndDiscardsThePartial_NotACrash()
    {
        byte[] bytes = Bytes(10_000);
        ResponseMediaFile media = await AddMedia(bytes);
        _storeFile.FullAfterPartialBytes = 3000;
        _clientApi.FileHandler = (_, _, _) => Whole(bytes);

        bool result = await _serviceDownload.EnsureOnDevice(media.ID);

        Assert.False(result);
        Assert.Single(_clientApi.FileRequestList);
        Assert.Empty(_delay.DelayList);
        FileState? state = await State(media.ID);
        Assert.Equal(FileSyncStateEnum.Failed, state?.State);
        Assert.Contains("no room", state?.Error);
        Assert.Empty(await _storeFile.GetPartialIDList());
        Assert.Empty(await _storeFile.GetHeldIDList());
    }

    [Fact]
    public async Task EnsureOnDevice_ForAnUnknownMediaFile_ReturnsFalse_AndRequestsNothing()
    {
        bool result = await _serviceDownload.EnsureOnDevice(Guid.CreateVersion7());

        Assert.False(result);
        Assert.Empty(_clientApi.FileRequestList);
        Assert.Empty(await _storeMetadata.GetFileStateList());
    }

    [Fact]
    public async Task TheStateSequence_IsDownloadingThenOnDevice_OrDownloadingThenFailed()
    {
        byte[] bytes = Bytes(1000);
        ResponseMediaFile good = await AddMedia(bytes);
        ResponseMediaFile bad = await AddMedia(Bytes(1000, 5));
        _clientApi.FileHandler = (id, _, _) => id == good.ID ? Whole(bytes) : throw new ApiException(HttpStatusCode.NotFound, "Gone.");

        await _serviceDownload.EnsureOnDevice(good.ID);
        await _serviceDownload.EnsureOnDevice(bad.ID);

        Assert.Equal([FileSyncStateEnum.Downloading, FileSyncStateEnum.OnDevice], _storeMetadata.FileStateHistoryList.Where(x => x.MediaFileID == good.ID).Select(x => x.State));
        Assert.Equal([FileSyncStateEnum.Downloading, FileSyncStateEnum.Failed], _storeMetadata.FileStateHistoryList.Where(x => x.MediaFileID == bad.ID).Select(x => x.State));
    }

    [Fact]
    public async Task StateChanges_RaiseChanged()
    {
        byte[] bytes = Bytes(1000);
        ResponseMediaFile media = await AddMedia(bytes);
        _clientApi.FileHandler = (_, _, _) => Whole(bytes);
        int changed = 0;
        _serviceDownload.Changed += () => changed++;

        await _serviceDownload.EnsureOnDevice(media.ID);

        Assert.Equal(2, changed);
    }

    [Fact]
    public async Task Enqueue_StartsTheDownloadsAndReturns_ThenEveryMediaFileEndsOnDevice()
    {
        List<ResponseMediaFile> mediaList = [];
        Dictionary<Guid, byte[]> bytesMap = [];
        for (int i = 0; i < 5; i++)
        {
            byte[] bytes = Bytes(500, (byte)(i + 1));
            ResponseMediaFile media = await AddMedia(bytes);
            mediaList.Add(media);
            bytesMap[media.ID] = bytes;
        }

        _clientApi.FileHandler = (id, _, _) => Whole(bytesMap[id]);

        await _serviceDownload.Enqueue([.. mediaList.Select(x => x.ID)]);

        await WaitUntil(async () => (await _storeMetadata.GetFileStateList()).Count(x => x.State == FileSyncStateEnum.OnDevice) == 5);
    }

    [Fact]
    public async Task AFailedDownload_IsLogged_WithoutAnyTokenInTheMessage()
    {
        ResponseMediaFile media = await AddMedia(Bytes(500));
        _clientApi.FileHandler = (_, _, _) => throw new HttpRequestException("The network is down.");

        await _serviceDownload.EnsureOnDevice(media.ID);

        Assert.Contains(_logger.EntryList, x => x.Level == LogLevel.Warning);
        Assert.DoesNotContain(_logger.EntryList, x => x.Message.Contains("Bearer", StringComparison.OrdinalIgnoreCase));
    }

    #region -- Bulk --

    // Media files in the library with distinct bytes; the fake API client serves each one's bytes once the gate opens.
    private async Task<(List<ResponseMediaFile> MediaList, TaskCompletionSource Gate)> AddMediaAndServe(int count)
    {
        List<ResponseMediaFile> mediaList = [];
        Dictionary<Guid, byte[]> bytesMap = [];
        for (int i = 0; i < count; i++)
        {
            byte[] bytes = Bytes(500 + i, (byte)(i + 1));
            ResponseMediaFile media = await AddMedia(bytes);
            mediaList.Add(media);
            bytesMap[media.ID] = bytes;
        }

        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _clientApi.FileHandler = async (id, _, _) =>
        {
            await gate.Task;
            return await Whole(bytesMap[id]);
        };

        return (mediaList, gate);
    }

    private async Task WaitForTheBulkToEnd()
    {
        await WaitUntil(() =>
        {
            DownloadProgress progress = _serviceDownload.GetProgress();
            return Task.FromResult(progress.Finished + progress.Failed >= progress.Total || progress.StoppedReason is not null);
        });
    }

    [Fact]
    public async Task QueueMissing_QueuesOnlyMediaFilesNotOnDevice()
    {
        (List<ResponseMediaFile> mediaList, TaskCompletionSource gate) = await AddMediaAndServe(3);
        _storeFile.Hold(mediaList[0].ID, Bytes(500));
        await _storeMetadata.SaveFileState(new(mediaList[0].ID, FileSyncStateEnum.OnDevice, mediaList[0].ContentHash, null));
        gate.SetResult();

        await _serviceDownload.QueueMissing([.. mediaList.Select(x => x.ID)]);
        await WaitForTheBulkToEnd();

        Assert.Equal(2, _serviceDownload.GetProgress().Total);
        Assert.DoesNotContain(_clientApi.FileRequestList, x => x.ID == mediaList[0].ID);
        Assert.Equal(2, _clientApi.FileRequestList.Count);
    }

    [Fact]
    public async Task QueueMissing_ShowsProgress_TotalFinishedFailedAndBytesDone()
    {
        (List<ResponseMediaFile> mediaList, TaskCompletionSource gate) = await AddMediaAndServe(5);
        Func<Guid, long?, string?, Task<ApiFileResponse>> serve = _clientApi.FileHandler;
        _clientApi.FileHandler = (id, from, ifRange) => id == mediaList[4].ID ? throw new ApiException(HttpStatusCode.NotFound, "Gone.") : serve(id, from, ifRange);

        await _serviceDownload.QueueMissing([.. mediaList.Select(x => x.ID)]);

        Assert.Equal(new DownloadProgress(5, 0, 0, 0, null), _serviceDownload.GetProgress());

        gate.SetResult();
        await WaitForTheBulkToEnd();

        long expectedBytes = mediaList.Take(4).Sum(x => x.SizeBytes);
        Assert.Equal(new DownloadProgress(5, 4, 1, expectedBytes, null), _serviceDownload.GetProgress());
    }

    [Fact]
    public async Task QueueMissing_WhenFreeSpaceIsLowerThanTheNextFile_StopsAndReportsWhy_AndTheRestStayNotDownloaded()
    {
        (List<ResponseMediaFile> mediaList, TaskCompletionSource gate) = await AddMediaAndServe(6);
        await _serviceDownload.QueueMissing([.. mediaList.Select(x => x.ID)]);
        await WaitUntil(() => Task.FromResult(_clientApi.FileRequestList.Count == PresentationConstants.MaxParallelDownloads));

        _storeFile.FreeBytes = PresentationConstants.StorageReserveBytes;
        gate.SetResult();
        await WaitUntil(() => Task.FromResult(_serviceDownload.GetProgress().StoppedReason is not null));

        DownloadProgress progress = _serviceDownload.GetProgress();
        Assert.Equal(DownloadStopReasonEnum.StorageLow, progress.StoppedReason);
        Assert.Equal(6, progress.Total);
        Assert.Equal(PresentationConstants.MaxParallelDownloads, _clientApi.FileRequestList.Count);
        Assert.Equal(PresentationConstants.MaxParallelDownloads, (await _storeMetadata.GetFileStateList()).Count(x => x.State == FileSyncStateEnum.OnDevice));
        Assert.Equal(3, mediaList.Count(x => _clientApi.FileRequestList.All(r => r.ID != x.ID)));
    }

    [Fact]
    public async Task QueueMissing_WhenThereIsNoRoomForTheFirstFile_StartsNothing()
    {
        (List<ResponseMediaFile> mediaList, TaskCompletionSource gate) = await AddMediaAndServe(3);
        gate.SetResult();
        _storeFile.FreeBytes = PresentationConstants.StorageReserveBytes + mediaList.Min(x => x.SizeBytes) - 1;

        await _serviceDownload.QueueMissing([.. mediaList.Select(x => x.ID)]);
        await WaitUntil(() => Task.FromResult(_serviceDownload.GetProgress().StoppedReason is not null));

        Assert.Empty(_clientApi.FileRequestList);
        Assert.Empty(await _storeMetadata.GetFileStateList());
    }

    [Fact]
    public async Task QueueMissing_CalledAgain_DoesNotQueueTheSameMediaFilesTwice()
    {
        (List<ResponseMediaFile> mediaList, TaskCompletionSource gate) = await AddMediaAndServe(5);
        List<Guid> idList = [.. mediaList.Select(x => x.ID)];

        await _serviceDownload.QueueMissing(idList);
        await _serviceDownload.QueueMissing(idList);
        await _serviceDownload.QueueMissing(idList[..2]);
        gate.SetResult();
        await WaitForTheBulkToEnd();

        Assert.Equal(5, _serviceDownload.GetProgress().Total);
        Assert.Equal(5, _clientApi.FileRequestList.Count);
        Assert.Equal(5, _clientApi.FileRequestList.Select(x => x.ID).Distinct().Count());
    }

    [Fact]
    public async Task QueueMissing_ForAnEmptyList_DoesNothing()
    {
        await _serviceDownload.QueueMissing([]);

        Assert.Equal(new DownloadProgress(0, 0, 0, 0, null), _serviceDownload.GetProgress());
        Assert.Empty(_clientApi.FileRequestList);
    }

    [Fact]
    public async Task QueueMissing_AMediaFileAddedToTheLibraryLater_IsNotQueuedByARunningBulk()
    {
        (List<ResponseMediaFile> mediaList, TaskCompletionSource gate) = await AddMediaAndServe(2);
        await _serviceDownload.QueueMissing([.. mediaList.Select(x => x.ID)]);

        ResponseMediaFile later = await AddMedia(Bytes(300, 77));
        gate.SetResult();
        await WaitForTheBulkToEnd();

        Assert.Equal(2, _serviceDownload.GetProgress().Total);
        Assert.DoesNotContain(_clientApi.FileRequestList, x => x.ID == later.ID);
    }

    [Fact]
    public async Task QueueMissing_AfterAnEarlierBulkEnded_StartsAFreshProgress()
    {
        (List<ResponseMediaFile> mediaList, TaskCompletionSource gate) = await AddMediaAndServe(2);
        gate.SetResult();
        await _serviceDownload.QueueMissing([mediaList[0].ID]);
        await WaitForTheBulkToEnd();
        Assert.Equal(1, _serviceDownload.GetProgress().Finished);

        await _serviceDownload.QueueMissing([mediaList[1].ID]);
        await WaitForTheBulkToEnd();

        Assert.Equal(new DownloadProgress(1, 1, 0, mediaList[1].SizeBytes, null), _serviceDownload.GetProgress());
    }

    [Fact]
    public async Task Cancel_AMediaFileWaitingInTheBulkQueue_LeavesIt_AndTheTotalShrinks()
    {
        (List<ResponseMediaFile> mediaList, TaskCompletionSource gate) = await AddMediaAndServe(5);
        await _serviceDownload.QueueMissing([.. mediaList.Select(x => x.ID)]);
        await WaitUntil(() => Task.FromResult(_clientApi.FileRequestList.Count == PresentationConstants.MaxParallelDownloads));
        ResponseMediaFile waiting = mediaList.First(x => _clientApi.FileRequestList.All(r => r.ID != x.ID));

        await _serviceDownload.Cancel(waiting.ID);
        gate.SetResult();
        await WaitForTheBulkToEnd();

        Assert.Equal(4, _serviceDownload.GetProgress().Total);
        Assert.DoesNotContain(_clientApi.FileRequestList, x => x.ID == waiting.ID);
    }

    [Fact]
    public async Task NothingStartsFromReadingProgressOrState_OnlyAnExplicitCallDoes()
    {
        (List<ResponseMediaFile> mediaList, TaskCompletionSource gate) = await AddMediaAndServe(3);
        gate.SetResult();

        _serviceDownload.GetProgress();
        await _storeMetadata.GetFileStateList();
        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.Empty(_clientApi.FileRequestList);

        await _serviceDownload.Enqueue([.. mediaList.Select(x => x.ID)]);
        await WaitUntil(async () => (await _storeMetadata.GetFileStateList()).Count(x => x.State == FileSyncStateEnum.OnDevice) == 3);
    }

    [Fact]
    public async Task TheBulk_RaisesChangedAsItProgresses()
    {
        (List<ResponseMediaFile> mediaList, TaskCompletionSource gate) = await AddMediaAndServe(3);
        gate.SetResult();
        int changed = 0;
        _serviceDownload.Changed += () => Interlocked.Increment(ref changed);

        await _serviceDownload.QueueMissing([.. mediaList.Select(x => x.ID)]);
        await WaitForTheBulkToEnd();

        Assert.True(changed >= 3);
    }

    #endregion -- Bulk --

    private static async Task WaitUntil(Func<Task<bool>> condition)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(10);
        while (!await condition())
        {
            Assert.True(DateTime.UtcNow < deadline, "Timed out waiting for the condition.");
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }
    }
}
