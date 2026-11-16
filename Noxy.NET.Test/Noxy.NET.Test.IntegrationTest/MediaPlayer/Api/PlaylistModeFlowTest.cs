using System.Net;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.Test.MediaPlayer.Fixtures;
using Noxy.NET.Test.MediaPlayer.Fixtures.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class PlaylistModeFlowTest
{
    private static async Task<ResponsePlaylist> Create(MediaApiClient api, ApiAccount user)
    {
        HttpResponseMessage response = await api.CreatePlaylist(user.Token, JsonSerializer.Serialize(new { name = "Mix" }));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.ReadJson<ResponsePlaylist>();
    }

    private static async Task<ResponsePlaylist> InList(MediaApiClient api, ApiAccount user, Guid id)
    {
        List<ResponsePlaylist> list = await (await api.GetPlaylistLibrary(user.Token)).ReadJson<List<ResponsePlaylist>>();
        return list.Single(x => x.ID == id);
    }

    [Fact]
    public async Task Patch_SetsAMode_AnswersWithIt_PushesIt_AndShowsInTheListAndTheDetail()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponsePlaylist playlist = await Create(api, user);
        await using LibraryHubListener hub = await LibraryHubListener.Connect(factory, user.Token);

        HttpResponseMessage response = await api.PatchPlaylist(user.Token, playlist.ID, """{"shuffle":"Random"}""");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"shuffle\":\"Random\"", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(ShuffleModeEnum.Random, (await hub.NextPlaylistUpdated()).Shuffle);
        Assert.Equal(ShuffleModeEnum.Random, (await InList(api, user, playlist.ID)).Shuffle);
        Assert.Null((await InList(api, user, playlist.ID)).Repeat);
        ResponsePlaylistDetail detail = await (await api.GetPlaylist(user.Token, playlist.ID)).ReadJson<ResponsePlaylistDetail>();
        Assert.Equal(ShuffleModeEnum.Random, detail.Shuffle);
    }

    [Fact]
    public async Task Patch_WithNull_ClearsTheMode()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponsePlaylist playlist = await Create(api, user);
        await api.PatchPlaylist(user.Token, playlist.ID, """{"repeat":"Forever"}""");

        HttpResponseMessage response = await api.PatchPlaylist(user.Token, playlist.ID, """{"repeat":null}""");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null((await response.ReadJson<ResponsePlaylist>()).Repeat);
        Assert.Null((await InList(api, user, playlist.ID)).Repeat);
    }

    [Fact]
    public async Task Patch_WithAFieldLeftOut_LeavesTheModeAlone()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponsePlaylist playlist = await Create(api, user);
        await api.PatchPlaylist(user.Token, playlist.ID, """{"shuffle":"SpreadArtists"}""");

        await api.PatchPlaylist(user.Token, playlist.ID, """{"name":"Renamed"}""");

        ResponsePlaylist stored = await InList(api, user, playlist.ID);
        Assert.Equal("Renamed", stored.Name);
        Assert.Equal(ShuffleModeEnum.SpreadArtists, stored.Shuffle);
    }

    [Fact]
    public async Task Patch_TheSameModeTwice_PushesOnce()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponsePlaylist playlist = await Create(api, user);
        await using LibraryHubListener hub = await LibraryHubListener.Connect(factory, user.Token);

        await api.PatchPlaylist(user.Token, playlist.ID, """{"shuffle":"Random"}""");
        await api.PatchPlaylist(user.Token, playlist.ID, """{"shuffle":"Random"}""");

        await hub.NextPlaylistUpdated();
        Assert.Equal(1, await hub.CountReceivedAfter(TimeSpan.FromMilliseconds(500)));
    }

    [Theory]
    [InlineData("""{"shuffle":"Sometimes"}""")]
    [InlineData("""{"repeat":"Never"}""")]
    [InlineData("""{"shuffle":99}""")]
    [InlineData("""{"shuffle":1}""")]
    [InlineData("""{"repeat":"1"}""")]
    public async Task Patch_ANameThatIsNotAMode_Is400_AndChangesNothing(string json)
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponsePlaylist playlist = await Create(api, user);

        HttpResponseMessage response = await api.PatchPlaylist(user.Token, playlist.ID, json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ResponsePlaylist stored = await InList(api, user, playlist.ID);
        Assert.Null(stored.Shuffle);
        Assert.Null(stored.Repeat);
    }
}
