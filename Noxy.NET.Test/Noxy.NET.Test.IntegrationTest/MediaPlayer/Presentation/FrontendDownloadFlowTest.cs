using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FrontendDownloadFlowTest
{
    private const string Password = "a-long-enough-password";
    private const string FileSuffix = "/File";

    private static async Task<(ApiAccount Account, string Email)> CreateAccount(MediaApiClient api)
    {
        string email = $"{Guid.CreateVersion7()}@example.com";
        return (await api.SignUp(email), email);
    }

    // Uploads one media file with its own bytes and waits until the server has it. Returns what the server holds for it.
    private static async Task<ResponseMediaFile> Upload(MediaApiClient api, ApiAccount account, string title, byte[] bytes)
    {
        int before = (await api.GetMediaFileList(account.Token)).Count;
        HttpResponseMessage response = await api.Upload(account.Token, bytes, title: title);
        response.EnsureSuccessStatusCode();

        return (await api.WaitForMediaFileList(account.Token, before + 1)).Single(x => x.Title == title);
    }

    // A signed-in, started frontend with the library synced.
    private static async Task<FrontendClient> SignedIn(ApiFactory factory, string email)
    {
        FrontendClient frontend = FrontendClient.Create(factory);
        await frontend.Session.SignIn(email, Password);
        await frontend.Startup.Start();

        return frontend;
    }

    private static int FileRequestCount(FrontendClient frontend, Guid id)
    {
        return frontend.Network.RequestList.Count(x => x.Path == $"/Media/{id}{FileSuffix}");
    }

    private static async Task<FileState?> State(FrontendClient frontend, Guid id)
    {
        return (await frontend.Store.GetFileStateList()).SingleOrDefault(x => x.MediaFileID == id);
    }

    [Fact]
    public async Task EnsureOnDevice_DownloadsTheFile_StateIsOnDeviceWithTheServersHash_AndTheBytesAreTheUpload()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        byte[] bytes = MediaApiClient.Mp3(5000);
        ResponseMediaFile media = await Upload(api, account, "One", bytes);
        await using FrontendClient frontend = await SignedIn(factory, email);

        bool onDevice = await frontend.Download.EnsureOnDevice(media.ID);

        Assert.True(onDevice);
        FileState? state = await State(frontend, media.ID);
        Assert.Equal(FileSyncStateEnum.OnDevice, state?.State);
        Assert.Null(state?.Error);
        string? path = await frontend.FileStore.GetPath(media.ID);
        Assert.NotNull(path);
        byte[] stored = await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken);
        Assert.Equal(bytes, stored);
    }

    [Fact]
    public async Task ASecondEnsureOnDevice_MakesNoRequest()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        ResponseMediaFile media = await Upload(api, account, "One", MediaApiClient.Mp3(3000));
        await using FrontendClient frontend = await SignedIn(factory, email);
        await frontend.Download.EnsureOnDevice(media.ID);
        Assert.Equal(1, FileRequestCount(frontend, media.ID));

        bool again = await frontend.Download.EnsureOnDevice(media.ID);

        Assert.True(again);
        Assert.Equal(1, FileRequestCount(frontend, media.ID));
    }

    [Fact]
    public async Task Play_LoadsTheStoredPath_AndMakesNoRequestForAFileThatIsOnTheDevice()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        ResponseMediaFile media = await Upload(api, account, "One", MediaApiClient.Mp3(3000));
        await using FrontendClient frontend = await SignedIn(factory, email);
        await frontend.Download.EnsureOnDevice(media.ID);
        int requestsBefore = frontend.Network.RequestList.Count(x => x.Path.EndsWith(FileSuffix, StringComparison.Ordinal));

        await frontend.Player.StartLibrary(media.ID, LibrarySortEnum.Title, false, string.Empty);

        Assert.Equal(await frontend.FileStore.GetPath(media.ID), frontend.Audio.LoadedPath);
        Assert.True(frontend.Audio.IsPlaying);
        Assert.Equal(requestsBefore, frontend.Network.RequestList.Count(x => x.Path.EndsWith(FileSuffix, StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Play_OfAMissingFile_DownloadsItVerifiesItAndPlaysIt()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        ResponseMediaFile media = await Upload(api, account, "One", MediaApiClient.Mp3(4000));
        await using FrontendClient frontend = await SignedIn(factory, email);

        await frontend.Player.StartLibrary(media.ID, LibrarySortEnum.Title, false, string.Empty);

        Assert.Equal(FileSyncStateEnum.OnDevice, (await State(frontend, media.ID))?.State);
        Assert.Equal(await frontend.FileStore.GetPath(media.ID), frontend.Audio.LoadedPath);
        Assert.True(frontend.Audio.IsPlaying);
    }

    [Fact]
    public async Task EditingTheMediaFile_ThroughTheApi_DoesNotDownloadItAgain()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        ResponseMediaFile media = await Upload(api, account, "Before", MediaApiClient.Mp3(3000));
        await using FrontendClient frontend = await SignedIn(factory, email);
        await frontend.Download.EnsureOnDevice(media.ID);

        HttpResponseMessage response = await api.Patch(account.Token, media.ID, """{"title":"After"}""");
        response.EnsureSuccessStatusCode();
        await FrontendClient.WaitFor(async () => (await frontend.Store.GetMediaFile(media.ID))?.Title == "After", "the edit push");

        Assert.Equal(1, FileRequestCount(frontend, media.ID));
        Assert.Equal(FileSyncStateEnum.OnDevice, (await State(frontend, media.ID))?.State);
        Assert.NotNull(await frontend.FileStore.GetPath(media.ID));
    }

    [Fact]
    public async Task DeletingTheMediaFile_ThroughTheApi_RemovesItsFileAndState()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        ResponseMediaFile media = await Upload(api, account, "Doomed", MediaApiClient.Mp3(3000));
        await using FrontendClient frontend = await SignedIn(factory, email);
        await frontend.Download.EnsureOnDevice(media.ID);
        Assert.NotNull(await frontend.FileStore.GetPath(media.ID));

        HttpResponseMessage response = await api.Delete(account.Token, media.ID);
        response.EnsureSuccessStatusCode();

        await FrontendClient.WaitFor(async () => await frontend.FileStore.GetPath(media.ID) is null, "the file to be removed");
        Assert.Null(await State(frontend, media.ID));
        Assert.Null(await frontend.Store.GetMediaFile(media.ID));
    }

    [Fact]
    public async Task ADownloadCutOffHalfWay_ResumesFromThePartialFile_AndEndsWithAVerifiedFile()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        byte[] bytes = MediaApiClient.Mp3(20_000);
        ResponseMediaFile media = await Upload(api, account, "Long", bytes);
        await using FrontendClient frontend = await SignedIn(factory, email);
        frontend.Network.CutNextFileResponseAfter(8000);

        bool onDevice = await frontend.Download.EnsureOnDevice(media.ID);

        Assert.True(onDevice);
        Assert.Equal(2, frontend.Network.FileRangeList.Count);
        Assert.Null(frontend.Network.FileRangeList[0]);
        Assert.Equal("bytes=8000-", frontend.Network.FileRangeList[1]);
        string? path = await frontend.FileStore.GetPath(media.ID);
        Assert.NotNull(path);
        Assert.Equal(bytes, await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken));
        Assert.Equal(FileSyncStateEnum.OnDevice, (await State(frontend, media.ID))?.State);
        Assert.Empty(await frontend.FileStore.GetPartialIDList());
    }

    [Fact]
    public async Task AfterTheFilesAreDeletedFromTheDisk_RepairMarksNotDownloaded_AndANewEnsureOnDeviceDownloadsAgain()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        ResponseMediaFile media = await Upload(api, account, "One", MediaApiClient.Mp3(3000));
        await using FrontendClient frontend = await SignedIn(factory, email);
        await frontend.Download.EnsureOnDevice(media.ID);
        string path = await frontend.FileStore.GetPath(media.ID) ?? throw new InvalidOperationException("The file was not stored.");
        File.Delete(path);

        await frontend.Reconciler.Repair();

        Assert.Null(await State(frontend, media.ID));
        Assert.Null(await frontend.FileStore.GetPath(media.ID));

        bool onDevice = await frontend.Download.EnsureOnDevice(media.ID);

        Assert.True(onDevice);
        Assert.Equal(2, FileRequestCount(frontend, media.ID));
        Assert.NotNull(await frontend.FileStore.GetPath(media.ID));
    }

    [Fact]
    public async Task APartialFileLeftByAnEarlierRun_IsDiscardedAtStartUp_NeverResumed()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        ResponseMediaFile media = await Upload(api, account, "One", MediaApiClient.Mp3(3000));
        await using FrontendClient first = await SignedIn(factory, email);
        await first.FileStore.AppendPartial(media.ID, new MemoryStream([1, 2, 3]));

        await using FrontendClient restarted = FrontendClient.Create(factory, first.SessionStorage, first.Folder);
        await restarted.Startup.Start();

        Assert.Empty(await restarted.FileStore.GetPartialIDList());
        Assert.True(await restarted.Download.EnsureOnDevice(media.ID));
        Assert.Null(restarted.Network.FileRangeList.Single());
    }

    [Fact]
    public async Task ThePlayerQueue_PlaysTwoMediaFilesInOrder_AndPrefetchesTheThird()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        ResponseMediaFile first = await Upload(api, account, "First", MediaApiClient.Mp3(3000));
        ResponseMediaFile second = await Upload(api, account, "Second", MediaApiClient.Mp3(3100));
        // Never named again: the library queue after "Second" holds it, and the test waits for it to be prefetched.
        await Upload(api, account, "Third", MediaApiClient.Mp3(3200));
        await using FrontendClient frontend = await SignedIn(factory, email);

        await frontend.Player.StartLibrary(first.ID, LibrarySortEnum.Title, false, string.Empty);

        Assert.Equal(await frontend.FileStore.GetPath(first.ID), frontend.Audio.LoadedPath);
        await FrontendClient.WaitFor(async () => (await frontend.Store.GetFileStateList()).Count(x => x.State == FileSyncStateEnum.OnDevice) == 3, "the next media files to be prefetched");

        int requestsBefore = frontend.Network.RequestList.Count(x => x.Path.EndsWith(FileSuffix, StringComparison.Ordinal));
        frontend.Audio.RaiseEnded();
        string? secondPath = await frontend.FileStore.GetPath(second.ID);
        await FrontendClient.WaitFor(() => Task.FromResult(frontend.Player.Current?.MediaFileID == second.ID && frontend.Audio.LoadedPath == secondPath && frontend.Audio.IsPlaying), "the second media file to play");

        Assert.Equal(await frontend.FileStore.GetPath(second.ID), frontend.Audio.LoadedPath);
        Assert.Equal(requestsBefore, frontend.Network.RequestList.Count(x => x.Path.EndsWith(FileSuffix, StringComparison.Ordinal)));
    }

    [Fact]
    public async Task ABulkDownload_FetchesEveryMissingMediaFile_AndReportsTheProgress()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        List<ResponseMediaFile> mediaList = [];
        for (int i = 0; i < 4; i++) mediaList.Add(await Upload(api, account, $"Media {i}", MediaApiClient.Mp3(2000 + i * 100)));
        await using FrontendClient frontend = await SignedIn(factory, email);
        await frontend.Download.EnsureOnDevice(mediaList[0].ID);

        await frontend.Download.QueueMissing([.. mediaList.Select(x => x.ID)]);
        await FrontendClient.WaitFor(() =>
        {
            DownloadProgress progress = frontend.Download.GetProgress();
            return Task.FromResult(progress.Finished + progress.Failed >= progress.Total && progress.Total > 0);
        }, "the bulk download to end");

        DownloadProgress done = frontend.Download.GetProgress();
        Assert.Equal(3, done.Total);
        Assert.Equal(3, done.Finished);
        Assert.Equal(0, done.Failed);
        Assert.Equal(mediaList.Skip(1).Sum(x => x.SizeBytes), done.BytesDone);
        Assert.Equal(4, (await frontend.FileStore.GetHeldIDList()).Count);
        Assert.Equal(1, FileRequestCount(frontend, mediaList[0].ID));
    }
}
