using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.Test.MediaPlayer.Fixtures;
using Noxy.NET.Test.MediaPlayer.Fixtures.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FrontendMetadataSyncFlowTest
{
    private const string Password = "a-long-enough-password";
    private const string SettingsPath = "/Settings";

    private static async Task<(ApiAccount Account, string Email)> CreateAccount(MediaApiClient api)
    {
        string email = $"{Guid.CreateVersion7()}@example.com";
        return (await api.SignUp(email), email);
    }

    private static async Task<FrontendClient> SignedIn(ApiFactory factory, string email)
    {
        FrontendClient frontend = FrontendClient.Create(factory);
        await frontend.Session.SignIn(email, Password);
        return frontend;
    }

    private static async Task<ResponseArtist> CreateArtist(MediaApiClient api, ApiAccount account, string name)
    {
        HttpResponseMessage response = await api.SendJson(account.Token, HttpMethod.Post, "/Artist", JsonSerializer.Serialize(new { name }));
        response.EnsureSuccessStatusCode();

        return await response.ReadJson<ResponseArtist>();
    }

    private static async Task<bool> KeepQueued(FrontendClient frontend)
    {
        return (await frontend.SettingService.GetSettingList()).FirstOrDefault(x => x.Key == SettingKey.KeepQueuedItems)?.GetBoolean() ?? false;
    }

    private static async Task<bool> HasArtist(FrontendClient frontend, string name)
    {
        return (await frontend.Store.Artist.GetList()).Any(x => x.Name == name);
    }

    [Fact]
    public async Task ASettingChangedThroughTheApi_ArrivesAsAPush_WithNoETagChange_AndTheSettingFollows()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        await using FrontendClient frontend = await SignedIn(factory, email);
        await frontend.SettingService.SyncSettingList();
        string? eTag = await frontend.Store.Setting.GetETag();
        await frontend.Hub.Start();
        Assert.True(frontend.Hub.IsConnected);
        Assert.False(await KeepQueued(frontend));

        HttpResponseMessage response = await api.SendJson(account.Token, HttpMethod.Patch, $"{SettingsPath}/{SettingKey.KeepQueuedItems}", """{"value":true}""");
        response.EnsureSuccessStatusCode();

        await FrontendClient.WaitFor(() => KeepQueued(frontend), "the setting push");
        Assert.Equal(eTag, await frontend.Store.Setting.GetETag());
    }

    [Fact]
    public async Task KeepQueuedItems_WrittenThroughTheSettingService_IsAccepted_AndReadBackByASecondClient()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (_, string email) = await CreateAccount(api);
        await using FrontendClient writer = await SignedIn(factory, email);
        await using FrontendClient reader = await SignedIn(factory, email);

        await writer.SettingService.Update(SettingKey.KeepQueuedItems, JsonSerializer.SerializeToElement(true));
        await reader.SettingService.SyncSettingList();

        ResponseSetting stored = (await writer.SettingService.GetSettingList()).Single(x => x.Key == SettingKey.KeepQueuedItems);
        ResponseSetting read = (await reader.SettingService.GetSettingList()).Single(x => x.Key == SettingKey.KeepQueuedItems);
        Assert.True(stored.GetBoolean());
        Assert.True(read.GetBoolean());
    }

    [Fact]
    public async Task AnArtist_CreatedRenamedAndDeletedThroughTheApi_Arrives_AndTheStoreFollows()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        await using FrontendClient frontend = await SignedIn(factory, email);
        await frontend.MetadataSync.SyncArtistList();
        await frontend.Hub.Start();

        ResponseArtist artist = await CreateArtist(api, account, "Artist One");
        await FrontendClient.WaitFor(() => HasArtist(frontend, "Artist One"), "the artist push");

        (await api.SendJson(account.Token, HttpMethod.Patch, $"/Artist/{artist.ID}", """{"name":"Renamed"}""")).EnsureSuccessStatusCode();
        await FrontendClient.WaitFor(() => HasArtist(frontend, "Renamed"), "the rename push");
        Assert.False(await HasArtist(frontend, "Artist One"));

        (await api.SendJson(account.Token, HttpMethod.Delete, $"/Artist/{artist.ID}")).EnsureSuccessStatusCode();
        await FrontendClient.WaitFor(async () => (await frontend.Store.Artist.GetList()).Count == 0, "the delete push");
    }

    [Fact]
    public async Task AGenre_CreatedThroughTheApi_Arrives_AndTheStoreFollows()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        await using FrontendClient frontend = await SignedIn(factory, email);
        await frontend.MetadataSync.SyncGenreList();
        await frontend.Hub.Start();

        (await api.SendJson(account.Token, HttpMethod.Post, "/Genre", """{"name":"Jazz"}""")).EnsureSuccessStatusCode();

        await FrontendClient.WaitFor(async () => (await frontend.Store.Genre.GetList()).Any(x => x.Name == "Jazz"), "the genre push");
    }

    [Fact]
    public async Task AStartAfterTheConnectionWasDown_BringsTheSettingsAndTheListsLevel()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        await using FrontendClient frontend = await SignedIn(factory, email);
        await frontend.Startup.Start();
        await frontend.Hub.Stop();

        (await api.SendJson(account.Token, HttpMethod.Patch, $"{SettingsPath}/{SettingKey.KeepQueuedItems}", """{"value":true}""")).EnsureSuccessStatusCode();
        await CreateArtist(api, account, "While Away");
        (await api.SendJson(account.Token, HttpMethod.Post, "/Genre", """{"name":"Missed"}""")).EnsureSuccessStatusCode();
        await frontend.Startup.Start();

        Assert.True(await KeepQueued(frontend));
        Assert.True(await HasArtist(frontend, "While Away"));
        Assert.Contains(await frontend.Store.Genre.GetList(), x => x.Name == "Missed");
    }

    [Fact]
    public async Task AClearedStoreWithTheETagsStillThere_IsRefilledByTheNextSync()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        await CreateArtist(api, account, "Kept");
        await using FrontendClient frontend = await SignedIn(factory, email);
        await frontend.SettingService.SyncSettingList();
        await frontend.MetadataSync.SyncArtistList();
        Assert.NotNull(await frontend.Store.Setting.GetETag());
        Assert.NotNull(await frontend.Store.Artist.GetETag());

        await frontend.Store.Setting.ReplaceList([]);
        await frontend.Store.Artist.ReplaceList([]);
        await frontend.SettingService.SyncSettingList();
        await frontend.MetadataSync.SyncArtistList();

        Assert.NotEmpty(await frontend.Store.Setting.GetList());
        Assert.True(await HasArtist(frontend, "Kept"));
        Assert.All(frontend.Network.RequestList.Where(x => x.Path is SettingsPath or "/Artist").Skip(2), x => Assert.Null(x.IfNoneMatch));
    }

    [Fact]
    public async Task ASecondSyncWithNothingChanged_IsA304_ForSettingsAndEveryList()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await CreateAccount(api);
        await CreateArtist(api, account, "Artist");
        await using FrontendClient frontend = await SignedIn(factory, email);

        await frontend.SettingService.SyncSettingList();
        await frontend.MetadataSync.SyncArtistList();
        await frontend.MetadataSync.SyncAlbumList();
        await frontend.MetadataSync.SyncGenreList();
        string? settingETag = await frontend.Store.Setting.GetETag();
        await frontend.SettingService.SyncSettingList();
        await frontend.MetadataSync.SyncArtistList();

        Assert.Equal(settingETag, await frontend.Store.Setting.GetETag());
        Assert.Equal(2, frontend.Network.RequestList.Count(x => x.Path == SettingsPath));
        Assert.Equal(settingETag, frontend.Network.RequestList.Last(x => x.Path == SettingsPath).IfNoneMatch);
    }
}
