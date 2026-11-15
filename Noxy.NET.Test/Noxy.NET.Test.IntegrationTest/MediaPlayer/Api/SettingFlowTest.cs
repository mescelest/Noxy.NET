using System.Net;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.Test.MediaPlayer.Fixtures;
using Noxy.NET.Test.MediaPlayer.Fixtures.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class SettingFlowTest
{
    private static async Task<List<ResponseSetting>> GetList(MediaApiClient api, ApiAccount user)
    {
        HttpResponseMessage response = await api.GetPath(user.Token, "/Settings");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadJson<List<ResponseSetting>>();
    }

    private static async Task<HttpResponseMessage> Patch(MediaApiClient api, ApiAccount user, string key, string json)
    {
        return await api.SendJson(user.Token, HttpMethod.Patch, $"/Settings/{key}", json);
    }

    private static JsonElement Value(ResponseSetting setting)
    {
        return JsonSerializer.SerializeToElement(setting.Value);
    }

    [Fact]
    public async Task Get_ForANewUser_ReturnsTheFourDefaultsInOrderWithTheirTypes_AStrongETag_AndNothingIsSaved()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        HttpResponseMessage response = await api.GetPath(user.Token, "/Settings");
        string body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        List<ResponseSetting> list = JsonSerializer.Deserialize<List<ResponseSetting>>(body, JsonSerializerOptions.Web) ?? [];

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Matches("^\"[0-9a-f]{64}\"$", response.GetETag());
        Assert.Equal([SettingKey.Shuffle, SettingKey.Repeat, SettingKey.Volume, SettingKey.MergeOnRename], list.Select(x => x.Key).ToList());
        Assert.Equal([SettingValueType.Enum, SettingValueType.Enum, SettingValueType.Integer, SettingValueType.Boolean], list.Select(x => x.ValueType).ToList());
        Assert.Equal(["Off", "No", "100", "False"], list.Select(x => Value(x).ToString()).ToList());
        Assert.Contains("\"valueType\":\"Enum\"", body);
        Assert.All(list, x => Assert.Equal(DateTime.UnixEpoch, x.TimeModified));
    }

    [Fact]
    public async Task Patch_EachSetting_SavesAndReadsBack_AndTheOthersStayDefault()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        string defaultETag = (await api.GetPath(user.Token, "/Settings")).GetETag();

        HttpResponseMessage volume = await Patch(api, user, SettingKey.Volume, """{"value":40}""");
        HttpResponseMessage repeat = await Patch(api, user, SettingKey.Repeat, """{"value":"Forever"}""");
        HttpResponseMessage shuffle = await Patch(api, user, SettingKey.Shuffle, """{"value":"SpreadArtists"}""");
        HttpResponseMessage merge = await Patch(api, user, SettingKey.MergeOnRename, """{"value":true}""");
        HttpResponseMessage listResponse = await api.GetPath(user.Token, "/Settings");
        List<ResponseSetting> list = await listResponse.ReadJson<List<ResponseSetting>>();

        Assert.All([volume, repeat, shuffle, merge], x => Assert.Equal(HttpStatusCode.OK, x.StatusCode));
        Assert.Equal(40, Value(await volume.ReadJson<ResponseSetting>()).GetInt32());
        Assert.Equal("SpreadArtists", Value(list.Single(x => x.Key == SettingKey.Shuffle)).GetString());
        Assert.Equal("Forever", Value(list.Single(x => x.Key == SettingKey.Repeat)).GetString());
        Assert.Equal(40, Value(list.Single(x => x.Key == SettingKey.Volume)).GetInt32());
        Assert.True(Value(list.Single(x => x.Key == SettingKey.MergeOnRename)).GetBoolean());
        Assert.All(list, x => Assert.True(x.TimeModified > DateTime.UnixEpoch));
        Assert.NotEqual(defaultETag, listResponse.GetETag());
    }

    [Fact]
    public async Task Patch_OneSetting_LeavesTheOthersUntouched_AndChangesTheETag()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await Patch(api, user, SettingKey.Repeat, """{"value":"Once"}""");
        string before = (await api.GetPath(user.Token, "/Settings")).GetETag();

        await Patch(api, user, SettingKey.Volume, """{"value":10}""");

        List<ResponseSetting> list = await GetList(api, user);
        Assert.Equal("Once", Value(list.Single(x => x.Key == SettingKey.Repeat)).GetString());
        Assert.Equal("Off", Value(list.Single(x => x.Key == SettingKey.Shuffle)).GetString());
        Assert.NotEqual(before, (await api.GetPath(user.Token, "/Settings")).GetETag());
    }

    [Fact]
    public async Task Get_AMatchingIfNoneMatch_Gets304WithAnEmptyBody()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await Patch(api, user, SettingKey.Volume, """{"value":10}""");
        string eTag = (await api.GetPath(user.Token, "/Settings")).GetETag();
        using HttpRequestMessage conditional = new(HttpMethod.Get, "/Settings");
        conditional.Headers.Authorization = new("Bearer", user.Token);
        conditional.Headers.TryAddWithoutValidation("If-None-Match", eTag);

        HttpResponseMessage response = await factory.CreateClient().SendAsync(conditional, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
        Assert.Equal(eTag, response.GetETag());
    }

    [Theory]
    [InlineData(SettingKey.Volume, """{"value":"loud"}""")]
    [InlineData(SettingKey.Volume, """{"value":101}""")]
    [InlineData(SettingKey.Volume, """{"value":-1}""")]
    [InlineData(SettingKey.Volume, """{"value":3.5}""")]
    [InlineData(SettingKey.Volume, """{"value":null}""")]
    [InlineData(SettingKey.Repeat, """{"value":"Sometimes"}""")]
    [InlineData(SettingKey.Repeat, """{"value":"forever"}""")]
    [InlineData(SettingKey.Repeat, """{"value":1}""")]
    [InlineData(SettingKey.Shuffle, """{"value":true}""")]
    [InlineData(SettingKey.MergeOnRename, """{"value":"true"}""")]
    [InlineData(SettingKey.MergeOnRename, """{"value":1}""")]
    [InlineData(SettingKey.Volume, "{}")]
    [InlineData(SettingKey.Volume, "[]")]
    [InlineData(SettingKey.Volume, "40")]
    [InlineData(SettingKey.Volume, """{"value":""")]
    public async Task Patch_ABadValueOrBody_Is400_AndNothingChanges(string key, string json)
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        string before = (await api.GetPath(user.Token, "/Settings")).GetETag();

        HttpResponseMessage response = await Patch(api, user, key, json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(before, (await api.GetPath(user.Token, "/Settings")).GetETag());
    }

    [Theory]
    [InlineData("volume")]
    [InlineData("VOLUME")]
    [InlineData("Nope")]
    public async Task Patch_AnUnknownOrWrongCaseKey_Is404(string key)
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        Assert.Equal(HttpStatusCode.NotFound, (await Patch(api, user, key, """{"value":40}""")).StatusCode);
    }

    [Fact]
    public async Task Patch_TheSameValueTwice_PushesOnce_AndTheDefaultForAUserWithNoRowPushesNothing()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using LibraryHubListener hub = await LibraryHubListener.Connect(factory, user.Token);

        await Patch(api, user, SettingKey.Volume, """{"value":100}""");
        await Patch(api, user, SettingKey.Volume, """{"value":40}""");
        await Patch(api, user, SettingKey.Volume, """{"value":40}""");

        ResponseSetting pushed = await hub.NextSettingUpdated();
        Assert.Equal(40, Value(pushed).GetInt32());
        Assert.Equal(1, await hub.CountReceivedAfter(TimeSpan.FromMilliseconds(500)));
    }

    [Fact]
    public async Task Patch_PushesSettingUpdatedToTheOwnerOnly()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("b@example.com");
        await using LibraryHubListener ownerHub = await LibraryHubListener.Connect(factory, user.Token);
        await using LibraryHubListener otherHub = await LibraryHubListener.Connect(factory, other.Token);

        await Patch(api, user, SettingKey.Repeat, """{"value":"Forever"}""");

        ResponseSetting pushed = await ownerHub.NextSettingUpdated();
        Assert.Equal((SettingKey.Repeat, "Forever"), (pushed.Key, Value(pushed).GetString()));
        Assert.True(await otherHub.ReceivesNothingWithin(TimeSpan.FromMilliseconds(500)));
    }

    [Fact]
    public async Task TheSettingsOfAUser_AreNeverVisibleOrChangeableByAnother_AndNeedAToken()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("b@example.com");
        await Patch(api, user, SettingKey.Volume, """{"value":10}""");

        List<ResponseSetting> theirs = await GetList(api, other);

        Assert.Equal(100, Value(theirs.Single(x => x.Key == SettingKey.Volume)).GetInt32());
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.GetPath(null, "/Settings")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.SendJson(null, HttpMethod.Patch, $"/Settings/{SettingKey.Volume}", """{"value":1}""")).StatusCode);
        Assert.Equal(10, Value((await GetList(api, user)).Single(x => x.Key == SettingKey.Volume)).GetInt32());
    }

    [Fact]
    public async Task ACollidingRename_Is409WhileMergeOnRenameIsUnsaved_AndMergesAfterItIsSet()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseArtist first = await (await api.SendJson(user.Token, HttpMethod.Post, "/Artist", """{"name":"A"}""")).ReadJson<ResponseArtist>();
        ResponseArtist second = await (await api.SendJson(user.Token, HttpMethod.Post, "/Artist", """{"name":"B"}""")).ReadJson<ResponseArtist>();

        HttpResponseMessage conflict = await api.SendJson(user.Token, HttpMethod.Patch, $"/Artist/{second.ID}", """{"name":"A"}""");
        await Patch(api, user, SettingKey.MergeOnRename, """{"value":true}""");
        HttpResponseMessage merged = await api.SendJson(user.Token, HttpMethod.Patch, $"/Artist/{second.ID}", """{"name":"A"}""");

        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.Equal(HttpStatusCode.OK, merged.StatusCode);
        Assert.Equal(first.ID, (await merged.ReadJson<ResponseArtist>()).ID);
        Assert.Equal(first.ID, Assert.Single(await (await api.GetPath(user.Token, "/Artist")).ReadJson<List<ResponseArtist>>()).ID);
    }
}
