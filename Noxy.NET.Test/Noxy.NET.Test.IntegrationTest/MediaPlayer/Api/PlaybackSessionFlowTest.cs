using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Domain.Responses.Playback;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.Test.MediaPlayer.Fixtures;
using Noxy.NET.Test.MediaPlayer.Fixtures.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class PlaybackSessionFlowTest
{
    private const string Path = "/PlaybackSession";

    private static string Body(IEnumerable<Guid> queue, int? index = null, double position = 0, Guid? playlistID = null)
    {
        return JsonSerializer.Serialize(new { queueMediaFileIDList = queue, currentIndex = index, positionSeconds = position, playlistID });
    }

    private static async Task<HttpResponseMessage> Put(MediaApiClient api, ApiAccount user, string body)
    {
        return await api.SendJson(user.Token, HttpMethod.Put, Path, body);
    }

    private static async Task<ResponsePlaybackSession> Save(MediaApiClient api, ApiAccount user, string body)
    {
        HttpResponseMessage response = await Put(api, user, body);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadJson<ResponsePlaybackSession>();
    }

    private static async Task<HttpResponseMessage> Get(MediaApiClient api, ApiAccount user, string? ifNoneMatch = null)
    {
        return await api.GetPlaybackSession(user.Token, ifNoneMatch);
    }

    [Fact]
    public async Task Get_WithNoSession_Is204()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        Assert.Equal(HttpStatusCode.NoContent, (await Get(api, user)).StatusCode);
    }

    [Fact]
    public async Task Put_SavesTheSession_AndGetReturnsItWithAnETag_AndAMatchingRequestGets304()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> queue = await api.UploadMediaFileList(user, 3);
        ResponsePlaylist playlist = await (await api.CreatePlaylist(user.Token, JsonSerializer.Serialize(new { name = "Mix" }))).ReadJson<ResponsePlaylist>();

        ResponsePlaybackSession saved = await Save(api, user, Body(queue, 1, 42.5, playlist.ID));

        Assert.Equal(queue, saved.QueueMediaFileIDList);
        Assert.Equal(1, saved.CurrentIndex);
        Assert.Equal(42.5, saved.PositionSeconds);
        Assert.Equal(playlist.ID, saved.PlaylistID);

        HttpResponseMessage response = await Get(api, user);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string eTag = response.GetETag();
        ResponsePlaybackSession read = await response.ReadJson<ResponsePlaybackSession>();
        Assert.Equal(saved.ID, read.ID);
        Assert.Equal(queue, read.QueueMediaFileIDList);

        Assert.Equal(HttpStatusCode.NotModified, (await Get(api, user, eTag)).StatusCode);

        await Save(api, user, Body(queue, 2, 43));

        HttpResponseMessage changed = await Get(api, user, eTag);
        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
        Assert.NotEqual(eTag, changed.GetETag());
    }

    [Fact]
    public async Task Put_AgainFromAnotherDevice_ReplacesIt_AndTheLastWriteWins()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> queue = await api.UploadMediaFileList(user, 2);

        ResponsePlaybackSession phone = await Save(api, user, Body(queue, 0, 10));
        ResponsePlaybackSession computer = await Save(api, user, Body([queue[1]], 0, 99));

        Assert.Equal(phone.ID, computer.ID);
        ResponsePlaybackSession read = await (await Get(api, user)).ReadJson<ResponsePlaybackSession>();
        Assert.Equal([queue[1]], read.QueueMediaFileIDList);
        Assert.Equal(99, read.PositionSeconds);
    }

    [Fact]
    public async Task Put_InvalidSaves_AreRejected_AndNothingIsStored()
    {
        await using ApiFactory factory = new(new() { [ConfigurationPath.Combine(ConfigPlayback.SectionName, nameof(ConfigPlayback.MaxQueueLength))] = "2" });
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("b@example.com");
        List<Guid> queue = await api.UploadMediaFileList(user, 3);
        List<Guid> foreign = await api.UploadMediaFileList(other, 1);

        Assert.Equal(HttpStatusCode.BadRequest, (await Put(api, user, Body(queue.Take(2), 2))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Put(api, user, Body(queue.Take(2), -1))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Put(api, user, Body([], 0))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Put(api, user, Body(queue.Take(2), 0, -1))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Put(api, user, Body(queue, 0))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Put(api, user, Body([queue[0], foreign[0]], 0))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Put(api, user, Body([Guid.CreateVersion7()], 0))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Put(api, user, Body([queue[0]], 0, 0, Guid.CreateVersion7()))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Put(api, user, "{not json")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Put(api, user, "{\"queueMediaFileIDList\":null}")).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await Get(api, user)).StatusCode);
    }

    [Fact]
    public async Task Put_PushesTheSessionToEveryDeviceOfTheUserOnce_AndNothingForAnIdenticalSave_OrToAnotherUser()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("b@example.com");
        List<Guid> queue = await api.UploadMediaFileList(user, 2);
        await using LibraryHubListener phone = await LibraryHubListener.Connect(factory, user.Token);
        await using LibraryHubListener computer = await LibraryHubListener.Connect(factory, user.Token, tokenInQueryString: true);
        await using LibraryHubListener stranger = await LibraryHubListener.Connect(factory, other.Token);

        ResponsePlaybackSession saved = await Save(api, user, Body(queue, 1, 5));
        await Save(api, user, Body(queue, 1, 5));

        ResponsePlaybackSession onPhone = await phone.NextPlaybackSessionUpdated();
        ResponsePlaybackSession onComputer = await computer.NextPlaybackSessionUpdated();
        Assert.Equal(saved.ID, onPhone.ID);
        Assert.Equal(saved.TimeModified, onComputer.TimeModified);
        Assert.Equal(1, await phone.CountReceivedAfter(TimeSpan.FromMilliseconds(500)));
        Assert.True(await stranger.ReceivesNothingWithin(TimeSpan.FromMilliseconds(300)));
    }

    [Fact]
    public async Task Delete_RemovesTheSession_Is204_PushesCleared_AndASecondDeleteIsA204WithNoPush()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> queue = await api.UploadMediaFileList(user, 1);
        await Save(api, user, Body(queue, 0));
        await using LibraryHubListener phone = await LibraryHubListener.Connect(factory, user.Token);

        Assert.Equal(HttpStatusCode.NoContent, (await api.SendJson(user.Token, HttpMethod.Delete, Path)).StatusCode);
        await phone.NextPlaybackSessionCleared();
        Assert.Equal(HttpStatusCode.NoContent, (await Get(api, user)).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await api.SendJson(user.Token, HttpMethod.Delete, Path)).StatusCode);
        Assert.Equal(1, await phone.CountReceivedAfter(TimeSpan.FromMilliseconds(500)));
    }

    [Fact]
    public async Task DeletingAMediaFileInTheQueue_RemovesItFromTheSession_MovesTheETag_AndPushesTheSession()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> queue = await api.UploadMediaFileList(user, 3);
        await Save(api, user, Body([queue[0], queue[1], queue[2], queue[1]], 2, 20));
        string before = (await Get(api, user)).GetETag();
        await using LibraryHubListener phone = await LibraryHubListener.Connect(factory, user.Token);

        Assert.Equal(HttpStatusCode.NoContent, (await api.Delete(user.Token, queue[1])).StatusCode);

        Assert.Equal(queue[1], await phone.NextMediaDeleted());
        ResponsePlaybackSession pushed = await phone.NextPlaybackSessionUpdated();
        Assert.Equal([queue[0], queue[2]], pushed.QueueMediaFileIDList);
        Assert.Equal(1, pushed.CurrentIndex);
        Assert.Equal(20, pushed.PositionSeconds);

        HttpResponseMessage response = await Get(api, user, before);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEqual(before, response.GetETag());
        ResponsePlaybackSession read = await response.ReadJson<ResponsePlaybackSession>();
        Assert.Equal(pushed.TimeModified, read.TimeModified);
        Assert.Equal([queue[0], queue[2]], read.QueueMediaFileIDList);
    }

    [Fact]
    public async Task DeletingTheMediaFileThatIsPlaying_ClearsTheIndexAndPosition_AndDeletingTheLastOneClearsTheSession()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> queue = await api.UploadMediaFileList(user, 2);
        await Save(api, user, Body(queue, 0, 55));
        await using LibraryHubListener phone = await LibraryHubListener.Connect(factory, user.Token);

        await api.Delete(user.Token, queue[0]);

        await phone.NextMediaDeleted();
        ResponsePlaybackSession pushed = await phone.NextPlaybackSessionUpdated();
        Assert.Equal([queue[1]], pushed.QueueMediaFileIDList);
        Assert.Null(pushed.CurrentIndex);
        Assert.Equal(0, pushed.PositionSeconds);

        await api.Delete(user.Token, queue[1]);

        await phone.NextMediaDeleted();
        await phone.NextPlaybackSessionCleared();
        Assert.Equal(HttpStatusCode.NoContent, (await Get(api, user)).StatusCode);
    }

    [Fact]
    public async Task DeletingAMediaFileOfTheSameUserThatIsNotInTheQueue_ChangesNeitherTheSessionNorItsETag()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> library = await api.UploadMediaFileList(user, 2);
        await Save(api, user, Body([library[0]], 0));
        string before = (await Get(api, user)).GetETag();

        await api.Delete(user.Token, library[1]);

        Assert.Equal(HttpStatusCode.NotModified, (await Get(api, user, before)).StatusCode);
    }

    [Fact]
    public async Task EveryAction_WithoutAToken_Is401()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);

        Assert.Equal(HttpStatusCode.Unauthorized, (await api.SendJson(null, HttpMethod.Get, Path)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.SendJson(null, HttpMethod.Put, Path, Body([]))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.SendJson(null, HttpMethod.Delete, Path)).StatusCode);
    }

    [Fact]
    public async Task AnotherUsersSession_IsInvisible_AndUntouchedByDelete()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("b@example.com");
        List<Guid> queue = await api.UploadMediaFileList(user, 1);
        await Save(api, user, Body(queue, 0, 3));

        Assert.Equal(HttpStatusCode.NoContent, (await Get(api, other)).StatusCode);
        await api.SendJson(other.Token, HttpMethod.Delete, Path);

        Assert.Equal(HttpStatusCode.OK, (await Get(api, user)).StatusCode);
    }
}
