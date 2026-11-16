using System.Net;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FrontendSyncFlowTest
{
    private const string Password = "a-long-enough-password";
    private const string LibraryPath = "/Media/Library";
    private const string RefreshPath = "/Auth/Refresh";

    private static async Task<(ApiAccount Account, string Email)> CreateAccount(MediaApiClient api)
    {
        string email = $"{Guid.CreateVersion7()}@example.com";
        return (await api.SignUp(email), email);
    }

    private static async Task<ResponseMediaFile> UploadMedia(MediaApiClient api, ApiAccount account, string title)
    {
        HttpResponseMessage response = await api.Upload(account.Token, MediaApiClient.Mp3(), title: title);
        response.EnsureSuccessStatusCode();

        return (await api.WaitForMediaFileList(account.Token, 1)).Single(x => x.Title == title);
    }

    private static async Task<bool> StoreHasTitle(FrontendClient frontend, string title)
    {
        return (await frontend.Store.GetMediaFileList()).Any(x => x.Title == title);
    }

    [Fact]
    public async Task SignIn_KeepsTheRefreshTokenInTheStorage_AndTheAccessTokenInMemoryOnly()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (_, string email) = await CreateAccount(api);
        await using FrontendClient frontend = FrontendClient.Create(factory);

        await frontend.Session.SignIn(email, Password);

        Assert.True(frontend.Session.IsSignedIn);
        Assert.False(string.IsNullOrEmpty(frontend.SessionStorage.RefreshToken));
        Assert.NotEqual(await frontend.Session.GetAccessToken(), frontend.SessionStorage.RefreshToken);
    }

    [Fact]
    public async Task AnUpload_ArrivesAsAPush_AndFillsTheStoreWithoutASync_AndWritesNoETag()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        await using FrontendClient frontend = FrontendClient.Create(factory);
        await frontend.Session.SignIn(email, Password);
        await frontend.Hub.Start();
        Assert.True(frontend.Hub.IsConnected);

        await UploadMedia(api, account, "Pushed");

        await FrontendClient.WaitFor(() => StoreHasTitle(frontend, "Pushed"), "the upload push");
        Assert.Null(await frontend.Store.GetLibraryETag());
        Assert.DoesNotContain(frontend.Network.RequestList, x => x.Path == LibraryPath);
    }

    [Fact]
    public async Task SyncLibrary_FillsTheStore_AndASecondSyncIsA304()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        await UploadMedia(api, account, "One");
        await using FrontendClient frontend = FrontendClient.Create(factory);
        await frontend.Session.SignIn(email, Password);

        await frontend.Sync.SyncLibrary();
        string? eTag = await frontend.Store.GetLibraryETag();
        await frontend.Sync.SyncLibrary();

        Assert.Equal(["One"], (await frontend.Store.GetMediaFileList()).Select(x => x.Title).ToList());
        Assert.False(string.IsNullOrEmpty(eTag));
        List<(HttpMethod Method, string Path, string? IfNoneMatch, HttpStatusCode? Status)> requestList = frontend.Network.RequestList.Where(x => x.Path == LibraryPath).ToList();
        Assert.Equal(2, requestList.Count);
        Assert.Null(requestList[0].IfNoneMatch);
        Assert.Equal(HttpStatusCode.OK, requestList[0].Status);
        Assert.Equal(eTag, requestList[1].IfNoneMatch);
        Assert.Equal(HttpStatusCode.NotModified, requestList[1].Status);
    }

    [Fact]
    public async Task AnEdit_ArrivesAsAMediaUpdatedPush_WithoutAnETagChange()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        ResponseMediaFile media = await UploadMedia(api, account, "Before");
        await using FrontendClient frontend = FrontendClient.Create(factory);
        await frontend.Session.SignIn(email, Password);
        await frontend.Sync.SyncLibrary();
        string? eTag = await frontend.Store.GetLibraryETag();
        await frontend.Hub.Start();

        HttpResponseMessage response = await api.Patch(account.Token, media.ID, """{"title":"After"}""");
        response.EnsureSuccessStatusCode();

        await FrontendClient.WaitFor(() => StoreHasTitle(frontend, "After"), "the edit push");
        Assert.Single(await frontend.Store.GetMediaFileList());
        Assert.Equal(eTag, await frontend.Store.GetLibraryETag());
    }

    [Fact]
    public async Task ADelete_ArrivesAsMediaDeleted_AndTheHoldingPlaylistIsUpdated()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        ResponseMediaFile media = await UploadMedia(api, account, "Doomed");
        await using FrontendClient frontend = FrontendClient.Create(factory);
        await frontend.Session.SignIn(email, Password);
        ResponsePlaylist playlist = await frontend.Api.CreatePlaylist(new() { Name = "Mix" });
        await frontend.Api.AddPlaylistEntryList(playlist.ID, new() { MediaFileIDList = [media.ID] });
        await frontend.Sync.SyncLibrary();
        await frontend.Sync.SyncPlaylistList();
        Assert.Equal(1, (await frontend.Store.GetPlaylistList()).Single().EntryCount);
        await frontend.Hub.Start();

        HttpResponseMessage response = await api.Delete(account.Token, media.ID);
        response.EnsureSuccessStatusCode();

        await FrontendClient.WaitFor(async () => (await frontend.Store.GetMediaFileList()).Count == 0, "the delete push");
        await FrontendClient.WaitFor(async () => (await frontend.Store.GetPlaylistList()).Single().EntryCount == 0, "the playlist update push");
    }

    [Fact]
    public async Task APlayedCall_ArrivesAsPlaylistUpdated_WithTheNewLastPlayedTime_AndTheSameModifiedTime()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        await using FrontendClient frontend = FrontendClient.Create(factory);
        await frontend.Session.SignIn(email, Password);
        ResponsePlaylist playlist = await frontend.Api.CreatePlaylist(new() { Name = "Mix" });
        await frontend.Sync.SyncPlaylistList();
        ResponsePlaylist before = (await frontend.Store.GetPlaylistList()).Single();
        Assert.Null(before.TimeLastPlayed);
        await frontend.Hub.Start();

        HttpResponseMessage response = await api.SendJson(account.Token, HttpMethod.Post, $"/Playlist/{playlist.ID}/Played");
        response.EnsureSuccessStatusCode();

        await FrontendClient.WaitFor(async () => (await frontend.Store.GetPlaylistList()).Single().TimeLastPlayed is not null, "the played push");
        ResponsePlaylist after = (await frontend.Store.GetPlaylistList()).Single();
        Assert.Equal(before.TimeModified, after.TimeModified);
    }

    [Fact]
    public async Task AfterTheConnectionDropsAndReturns_TheListsAreInSyncAgain()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        await using FrontendClient frontend = FrontendClient.Create(factory);
        await frontend.Session.SignIn(email, Password);
        await frontend.Hub.Start();
        Assert.True(frontend.Hub.IsConnected);

        frontend.Network.Drop();
        await UploadMedia(api, account, "While offline");
        frontend.Network.Restore();

        await FrontendClient.WaitFor(() => StoreHasTitle(frontend, "While offline"), "the resync after reconnecting", 40);
        Assert.Contains(frontend.Network.RequestList, x => x.Path == LibraryPath);
    }

    [Fact]
    public async Task SigningOut_ClosesTheHub_SoNoMorePushesReachTheDevice()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        await using FrontendClient frontend = FrontendClient.Create(factory);
        await frontend.Session.SignIn(email, Password);
        await frontend.Hub.Start();
        Assert.True(frontend.Hub.IsConnected);

        await frontend.Session.SignOut();

        await FrontendClient.WaitFor(() => Task.FromResult(!frontend.Hub.IsConnected), "the hub to close after the sign-out");
        await UploadMedia(api, account, "After sign-out");
        await Task.Delay(500, TestContext.Current.CancellationToken);
        Assert.Empty(await frontend.Store.GetMediaFileList());
    }

    [Fact]
    public async Task AnExpiredAccessToken_IsRefreshedBeforeTheNextCall()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        await UploadMedia(api, account, "One");
        await using FrontendClient frontend = FrontendClient.Create(factory);
        await frontend.Session.SignIn(email, Password);
        string? tokenBefore = frontend.SessionStorage.RefreshToken;

        frontend.Time.Now = frontend.Time.Now.AddMinutes(20);
        await frontend.Sync.SyncLibrary();

        Assert.Contains(frontend.Network.RequestList, x => x.Path == RefreshPath);
        Assert.NotEqual(tokenBefore, frontend.SessionStorage.RefreshToken);
        Assert.Equal(["One"], (await frontend.Store.GetMediaFileList()).Select(x => x.Title).ToList());
    }

    [Fact]
    public async Task ARestartWithAStoredRefreshToken_SignsInSilently_SyncsAndConnectsTheHub()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        await UploadMedia(api, account, "One");
        await using FrontendClient first = FrontendClient.Create(factory);
        await first.Session.SignIn(email, Password);
        string? tokenBefore = first.SessionStorage.RefreshToken;

        await using FrontendClient restarted = FrontendClient.Create(factory, first.SessionStorage);
        await restarted.Startup.Start();

        Assert.True(restarted.Session.IsSignedIn);
        Assert.NotEqual(tokenBefore, restarted.SessionStorage.RefreshToken);
        Assert.Equal(["One"], (await restarted.Store.GetMediaFileList()).Select(x => x.Title).ToList());
        Assert.True(restarted.Hub.IsConnected);
    }

    [Fact]
    public async Task ARejectedRefreshToken_SignsOutCleanly_AndKeepsTheLocalLists()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        await UploadMedia(api, account, "Kept");
        await using FrontendClient first = FrontendClient.Create(factory);
        await first.Session.SignIn(email, Password);
        await first.Sync.SyncLibrary();

        await using FrontendClient restarted = FrontendClient.Create(factory, new() { RefreshToken = "no-longer-valid" }, first.Folder);
        await restarted.Startup.Start();

        Assert.False(restarted.Session.IsSignedIn);
        Assert.Null(restarted.SessionStorage.RefreshToken);
        Assert.False(restarted.Hub.IsConnected);
        Assert.Equal(["Kept"], (await restarted.Store.GetMediaFileList()).Select(x => x.Title).ToList());
    }

    [Fact]
    public async Task AFreshStoreFile_AfterTheOldOneWasDeleted_IsRefilledByTheNextSync()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        await UploadMedia(api, account, "One");
        await using FrontendClient first = FrontendClient.Create(factory);
        await first.Session.SignIn(email, Password);
        await first.Sync.SyncLibrary();
        Assert.False(string.IsNullOrEmpty(await first.Store.GetLibraryETag()));

        await using FrontendClient cleared = FrontendClient.Create(factory, first.SessionStorage);
        await cleared.Startup.Start();

        Assert.Equal(["One"], (await cleared.Store.GetMediaFileList()).Select(x => x.Title).ToList());
        Assert.Null(cleared.Network.RequestList.First(x => x.Path == LibraryPath).IfNoneMatch);
    }
}
