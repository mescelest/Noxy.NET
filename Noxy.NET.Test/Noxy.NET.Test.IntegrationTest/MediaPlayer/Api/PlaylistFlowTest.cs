using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class PlaylistFlowTest
{
    private static async Task<List<Guid>> UploadSongList(MediaApiClient api, ApiAccount user, int count)
    {
        for (int index = 1; index <= count; index++)
        {
            HttpResponseMessage response = await api.Upload(user.Token, MediaApiClient.Mp3(300 + index), $"Song {index}.mp3", $"Song {index}");
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            await api.WaitForMediaFileList(user.Token, index);
        }

        List<ResponseMediaFile> list = await api.GetMediaFileList(user.Token);
        return [.. list.OrderBy(x => x.Title).Select(x => x.ID)];
    }

    private static async Task<T> Read<T>(HttpResponseMessage response, HttpStatusCode expected = HttpStatusCode.OK)
    {
        Assert.Equal(expected, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<T>(TestContext.Current.CancellationToken))!;
    }

    private static async Task<ResponsePlaylist> CreatePlaylist(MediaApiClient api, ApiAccount user, string name = "Mix", bool allowDuplicates = true)
    {
        string json = JsonSerializer.Serialize(new { name, allowDuplicates });
        return await Read<ResponsePlaylist>(await api.CreatePlaylist(user.Token, json), HttpStatusCode.Created);
    }

    private static string SongBody(IEnumerable<Guid> songIDList, int? position = null)
    {
        return JsonSerializer.Serialize(new { songIDList, position });
    }

    private static string EntryBody(IEnumerable<Guid> entryIDList)
    {
        return JsonSerializer.Serialize(new { entryIDList });
    }

    private static async Task<ResponsePlaylistDetail> AddSongList(MediaApiClient api, ApiAccount user, Guid playlistID, IEnumerable<Guid> songIDList, int? position = null)
    {
        return await Read<ResponsePlaylistDetail>(await api.AddPlaylistEntryList(user.Token, playlistID, SongBody(songIDList, position)));
    }

    private static async Task<ResponsePlaylistDetail> GetDetail(MediaApiClient api, ApiAccount user, Guid playlistID)
    {
        return await Read<ResponsePlaylistDetail>(await api.GetPlaylist(user.Token, playlistID));
    }

    private static async Task<string> LibraryETag(MediaApiClient api, ApiAccount user)
    {
        HttpResponseMessage response = await api.GetPlaylistLibrary(user.Token);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return response.Headers.ETag!.Tag;
    }

    private static ApiFactory Factory(string key, string value)
    {
        return new(new() { [ConfigurationPath.Combine(ConfigPlaylist.SectionName, key)] = value });
    }

    #region -- Library --

    [Fact]
    public async Task Library_Returns200WithAnETagAndNoEntries_AndAMatchingRequestGets304()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponsePlaylist created = await CreatePlaylist(api, user, "Road trip");
        await AddSongList(api, user, created.ID, await UploadSongList(api, user, 2));

        HttpResponseMessage first = await api.GetPlaylistLibrary(user.Token);
        string eTag = first.Headers.ETag!.Tag;
        string body = await first.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        HttpResponseMessage second = await api.GetPlaylistLibrary(user.Token, eTag);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.DoesNotContain("entryList", body, StringComparison.OrdinalIgnoreCase);
        ResponsePlaylist listed = Assert.Single(JsonSerializer.Deserialize<List<ResponsePlaylist>>(body, JsonSerializerOptions.Web)!);
        Assert.Equal("Road trip", listed.Name);
        Assert.Equal(2, listed.EntryCount);
        Assert.True(first.Headers.CacheControl?.Private);
        Assert.True(first.Headers.CacheControl?.NoCache);
        Assert.Equal(HttpStatusCode.NotModified, second.StatusCode);
        Assert.Empty(await second.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
        Assert.Equal(eTag, second.Headers.ETag?.Tag);
    }

    [Fact]
    public async Task Library_ETagChangesAfterEveryKindOfChange()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> songList = await UploadSongList(api, user, 3);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        List<string> eTagList = [await LibraryETag(api, user)];

        async Task Step(Func<Task<HttpResponseMessage>> change)
        {
            HttpResponseMessage response = await change();
            Assert.True(response.IsSuccessStatusCode, $"The change failed with {(int)response.StatusCode}.");
            eTagList.Add(await LibraryETag(api, user));
        }

        ResponsePlaylistDetail detail = null!;
        await Step(() => api.PatchPlaylist(user.Token, playlist.ID, """{"name":"Renamed"}"""));
        await Step(() => api.PatchPlaylist(user.Token, playlist.ID, """{"allowDuplicates":false}"""));
        await Step(async () =>
        {
            HttpResponseMessage response = await api.AddPlaylistEntryList(user.Token, playlist.ID, SongBody(songList));
            detail = (await response.Content.ReadFromJsonAsync<ResponsePlaylistDetail>(TestContext.Current.CancellationToken))!;
            return response;
        });
        await Step(() => api.MovePlaylistEntry(user.Token, playlist.ID, detail.EntryList[0].ID, """{"position":2}"""));
        await Step(() => api.RemovePlaylistEntryList(user.Token, playlist.ID, EntryBody([detail.EntryList[1].ID])));
        await Step(() => api.CreatePlaylist(user.Token, """{"name":"Second"}"""));
        await Step(() => api.DeletePlaylist(user.Token, playlist.ID));

        for (int index = 1; index < eTagList.Count; index++)
        {
            Assert.NotEqual(eTagList[index - 1], eTagList[index]);
        }
    }

    [Fact]
    public async Task Library_NoOpChangesKeepTheETag()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> songList = await UploadSongList(api, user, 1);
        ResponsePlaylist playlist = await CreatePlaylist(api, user, "Mix", allowDuplicates: false);
        ResponsePlaylistDetail detail = await AddSongList(api, user, playlist.ID, songList);
        string before = await LibraryETag(api, user);

        await api.MovePlaylistEntry(user.Token, playlist.ID, detail.EntryList[0].ID, """{"position":0}""");
        await api.PatchPlaylist(user.Token, playlist.ID, """{"name":"Mix","allowDuplicates":false}""");
        await api.PatchPlaylist(user.Token, playlist.ID, "{}");
        await AddSongList(api, user, playlist.ID, songList);

        Assert.Equal(before, await LibraryETag(api, user));
    }

    [Fact]
    public async Task Library_EmptyList_ReturnsAnEmptyListAndAStableETag()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        HttpResponseMessage first = await api.GetPlaylistLibrary(user.Token);
        HttpResponseMessage second = await api.GetPlaylistLibrary(user.Token);

        Assert.Empty((await first.Content.ReadFromJsonAsync<List<ResponsePlaylist>>(TestContext.Current.CancellationToken))!);
        Assert.Equal(first.Headers.ETag!.Tag, second.Headers.ETag!.Tag);
    }

    [Fact]
    public async Task Library_IsGzippedWhenAsked()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await CreatePlaylist(api, user, "Mix");

        HttpResponseMessage response = await api.GetPlaylistLibrary(user.Token, acceptEncoding: "gzip");

        Assert.Contains("gzip", response.Content.Headers.ContentEncoding);
        await using GZipStream gzip = new(await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken), CompressionMode.Decompress);
        List<ResponsePlaylist> list = (await JsonSerializer.DeserializeAsync<List<ResponsePlaylist>>(gzip, JsonSerializerOptions.Web, TestContext.Current.CancellationToken))!;
        Assert.Equal("Mix", Assert.Single(list).Name);
    }

    [Fact]
    public async Task Library_IsPerUser()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("other@example.com");
        await CreatePlaylist(api, user);
        string before = await LibraryETag(api, user);

        await CreatePlaylist(api, other, "Theirs");

        Assert.Equal(before, await LibraryETag(api, user));
        Assert.Single((await (await api.GetPlaylistLibrary(user.Token)).Content.ReadFromJsonAsync<List<ResponsePlaylist>>(TestContext.Current.CancellationToken))!);
    }

    [Fact]
    public async Task Library_WithoutToken_Returns401()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);

        Assert.Equal(HttpStatusCode.Unauthorized, (await api.GetPlaylistLibrary(null)).StatusCode);
    }

    #endregion -- Library --

    #region -- Detail --

    [Fact]
    public async Task Get_ReturnsEntriesByPosition_And304OnAMatchingETag()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> songList = await UploadSongList(api, user, 3);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        await AddSongList(api, user, playlist.ID, songList);

        HttpResponseMessage first = await api.GetPlaylist(user.Token, playlist.ID);
        ResponsePlaylistDetail detail = await Read<ResponsePlaylistDetail>(first);
        HttpResponseMessage second = await api.GetPlaylist(user.Token, playlist.ID, first.Headers.ETag!.Tag);

        Assert.Equal(songList, detail.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([0, 1, 2], detail.EntryList.Select(x => x.Position));
        Assert.Equal(3, detail.EntryCount);
        Assert.Equal(HttpStatusCode.NotModified, second.StatusCode);
        Assert.Empty(await second.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Get_AnotherUsersPlaylist_Returns404()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("other@example.com");
        ResponsePlaylist playlist = await CreatePlaylist(api, user);

        Assert.Equal(HttpStatusCode.NotFound, (await api.GetPlaylist(other.Token, playlist.ID)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.GetPlaylist(user.Token, Guid.CreateVersion7())).StatusCode);
    }

    #endregion -- Detail --

    #region -- Create, patch and delete --

    [Fact]
    public async Task Create_Returns201WithThePlaylist()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        HttpResponseMessage response = await api.CreatePlaylist(user.Token, """{"name":"  Road trip  "}""");

        ResponsePlaylist created = await Read<ResponsePlaylist>(response, HttpStatusCode.Created);
        Assert.Equal("Road trip", created.Name);
        Assert.True(created.AllowDuplicates);
        Assert.Equal(0, created.EntryCount);
        Assert.Equal(DateTimeKind.Utc, created.TimeModified.Kind);
        Assert.EndsWith($"/Playlists/{created.ID}", response.Headers.Location!.AbsolutePath);
    }

    [Theory]
    [InlineData("""{"name":""}""")]
    [InlineData("""{"name":"   "}""")]
    [InlineData("""{}""")]
    [InlineData("""{"name":null}""")]
    [InlineData("""{"name":5}""")]
    [InlineData("""[]""")]
    public async Task Create_InvalidBodies_Return400(string json)
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        Assert.Equal(HttpStatusCode.BadRequest, (await api.CreatePlaylist(user.Token, json)).StatusCode);
    }

    [Fact]
    public async Task Create_ANameOf255Works_And256Returns400()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        Assert.Equal(HttpStatusCode.Created, (await api.CreatePlaylist(user.Token, JsonSerializer.Serialize(new { name = new string('a', 255) }))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await api.CreatePlaylist(user.Token, JsonSerializer.Serialize(new { name = new string('a', 256) }))).StatusCode);
    }

    [Fact]
    public async Task Create_PastTheLimit_Returns409()
    {
        await using ApiFactory factory = Factory(nameof(ConfigPlaylist.MaxPlaylistsPerUser), "2");
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await CreatePlaylist(api, user, "One");
        await CreatePlaylist(api, user, "Two");

        Assert.Equal(HttpStatusCode.Conflict, (await api.CreatePlaylist(user.Token, """{"name":"Three"}""")).StatusCode);
    }

    [Fact]
    public async Task Patch_RenamesAndTogglesTheFlag()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponsePlaylist playlist = await CreatePlaylist(api, user, "Before");

        ResponsePlaylist renamed = await Read<ResponsePlaylist>(await api.PatchPlaylist(user.Token, playlist.ID, """{"name":"After"}"""));
        ResponsePlaylist flagged = await Read<ResponsePlaylist>(await api.PatchPlaylist(user.Token, playlist.ID, """{"allowDuplicates":false}"""));

        Assert.Equal("After", renamed.Name);
        Assert.True(renamed.AllowDuplicates);
        Assert.Equal("After", flagged.Name);
        Assert.False(flagged.AllowDuplicates);
        Assert.True(flagged.TimeModified > playlist.TimeModified);
    }

    [Fact]
    public async Task Patch_DuplicatesOffWithDuplicates_Returns409_AndChangesNothing()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> songList = await UploadSongList(api, user, 1);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        await AddSongList(api, user, playlist.ID, [songList[0], songList[0]]);

        HttpResponseMessage response = await api.PatchPlaylist(user.Token, playlist.ID, """{"allowDuplicates":false,"name":"Renamed"}""");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ResponsePlaylistDetail detail = await GetDetail(api, user, playlist.ID);
        Assert.True(detail.AllowDuplicates);
        Assert.Equal("Mix", detail.Name);
    }

    [Fact]
    public async Task Patch_RemovingTheDuplicatesFirst_LetsTheFlagTurnOff()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> songList = await UploadSongList(api, user, 1);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        ResponsePlaylistDetail detail = await AddSongList(api, user, playlist.ID, [songList[0], songList[0]]);

        await Read<ResponsePlaylistDetail>(await api.RemovePlaylistEntryList(user.Token, playlist.ID, EntryBody([detail.EntryList[1].ID])));
        ResponsePlaylist flagged = await Read<ResponsePlaylist>(await api.PatchPlaylist(user.Token, playlist.ID, """{"allowDuplicates":false}"""));

        Assert.False(flagged.AllowDuplicates);
    }

    [Theory]
    [InlineData("""{"name":null}""")]
    [InlineData("""{"name":" "}""")]
    [InlineData("""{"allowDuplicates":null}""")]
    [InlineData("""{"allowDuplicates":"yes"}""")]
    [InlineData("""[]""")]
    public async Task Patch_WrongTypesOrNotAnObject_Return400(string json)
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponsePlaylist playlist = await CreatePlaylist(api, user);

        Assert.Equal(HttpStatusCode.BadRequest, (await api.PatchPlaylist(user.Token, playlist.ID, json)).StatusCode);
    }

    [Fact]
    public async Task Patch_AnotherUsersPlaylist_Returns404()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("other@example.com");
        ResponsePlaylist playlist = await CreatePlaylist(api, user);

        Assert.Equal(HttpStatusCode.NotFound, (await api.PatchPlaylist(other.Token, playlist.ID, """{"name":"Stolen"}""")).StatusCode);
    }

    [Fact]
    public async Task Delete_Returns204_AndTheSongsRemain()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("other@example.com");
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        await AddSongList(api, user, playlist.ID, await UploadSongList(api, user, 2));

        Assert.Equal(HttpStatusCode.NotFound, (await api.DeletePlaylist(other.Token, playlist.ID)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await api.DeletePlaylist(user.Token, playlist.ID)).StatusCode);

        Assert.Equal(HttpStatusCode.NotFound, (await api.GetPlaylist(user.Token, playlist.ID)).StatusCode);
        Assert.Equal(2, (await api.GetMediaFileList(user.Token)).Count);
    }

    #endregion -- Create, patch and delete --

    #region -- Entries --

    [Fact]
    public async Task AddEntries_AppendsAndReturnsThePlaylistWithPositions()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> songList = await UploadSongList(api, user, 3);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);

        await AddSongList(api, user, playlist.ID, songList.Take(2));
        ResponsePlaylistDetail detail = await AddSongList(api, user, playlist.ID, songList.Skip(2));

        Assert.Equal(songList, detail.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([0, 1, 2], detail.EntryList.Select(x => x.Position));
        Assert.Equal(3, detail.EntryCount);
    }

    [Fact]
    public async Task AddEntries_AtAPosition_ShiftsTheOthers()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> songList = await UploadSongList(api, user, 4);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        await AddSongList(api, user, playlist.ID, [songList[0], songList[1], songList[2]]);

        ResponsePlaylistDetail detail = await AddSongList(api, user, playlist.ID, [songList[3]], 1);

        Assert.Equal([songList[0], songList[3], songList[1], songList[2]], detail.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([0, 1, 2, 3], detail.EntryList.Select(x => x.Position));
        Assert.Equal(detail.EntryList.Select(x => x.MediaFileID), (await GetDetail(api, user, playlist.ID)).EntryList.Select(x => x.MediaFileID));
    }

    [Fact]
    public async Task AddEntries_DuplicatesOff_SkipsRepeats_AndDuplicatesOnKeepsThem()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> songList = await UploadSongList(api, user, 2);
        ResponsePlaylist strict = await CreatePlaylist(api, user, "Strict", allowDuplicates: false);
        ResponsePlaylist loose = await CreatePlaylist(api, user, "Loose");

        ResponsePlaylistDetail strictDetail = await AddSongList(api, user, strict.ID, [songList[0], songList[0], songList[1]]);
        ResponsePlaylistDetail again = await AddSongList(api, user, strict.ID, [songList[1]]);
        ResponsePlaylistDetail looseDetail = await AddSongList(api, user, loose.ID, [songList[0], songList[0]]);

        Assert.Equal([songList[0], songList[1]], strictDetail.EntryList.Select(x => x.MediaFileID));
        Assert.Equal(2, again.EntryList.Count);
        Assert.Equal([songList[0], songList[0]], looseDetail.EntryList.Select(x => x.MediaFileID));
    }

    [Fact]
    public async Task AddEntries_UnknownOrOtherUsersSong_Returns404_AndAddsNothing()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("other@example.com");
        List<Guid> mine = await UploadSongList(api, user, 1);
        List<Guid> theirs = await UploadSongList(api, other, 1);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);

        Assert.Equal(HttpStatusCode.NotFound, (await api.AddPlaylistEntryList(user.Token, playlist.ID, SongBody([mine[0], theirs[0]]))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.AddPlaylistEntryList(user.Token, playlist.ID, SongBody([Guid.CreateVersion7()]))).StatusCode);

        Assert.Empty((await GetDetail(api, user, playlist.ID)).EntryList);
    }

    [Fact]
    public async Task AddEntries_ToAnotherUsersPlaylist_Returns404()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("other@example.com");
        List<Guid> theirs = await UploadSongList(api, other, 1);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);

        Assert.Equal(HttpStatusCode.NotFound, (await api.AddPlaylistEntryList(other.Token, playlist.ID, SongBody(theirs))).StatusCode);
    }

    [Theory]
    [InlineData("""{"songIDList":[]}""")]
    [InlineData("""{}""")]
    [InlineData("""{"songIDList":["not-a-guid"]}""")]
    public async Task AddEntries_BadBodies_Return400(string json)
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponsePlaylist playlist = await CreatePlaylist(api, user);

        Assert.Equal(HttpStatusCode.BadRequest, (await api.AddPlaylistEntryList(user.Token, playlist.ID, json)).StatusCode);
    }

    [Fact]
    public async Task AddEntries_ABadPosition_Returns400()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> songList = await UploadSongList(api, user, 1);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);

        Assert.Equal(HttpStatusCode.BadRequest, (await api.AddPlaylistEntryList(user.Token, playlist.ID, SongBody(songList, -1))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await api.AddPlaylistEntryList(user.Token, playlist.ID, SongBody(songList, 5))).StatusCode);
        Assert.Empty((await GetDetail(api, user, playlist.ID)).EntryList);
    }

    [Fact]
    public async Task AddEntries_PastTheEntryCap_Returns409()
    {
        await using ApiFactory factory = Factory(nameof(ConfigPlaylist.MaxEntriesPerPlaylist), "2");
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> songList = await UploadSongList(api, user, 3);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        await AddSongList(api, user, playlist.ID, songList.Take(2));

        Assert.Equal(HttpStatusCode.Conflict, (await api.AddPlaylistEntryList(user.Token, playlist.ID, SongBody([songList[2]]))).StatusCode);
        Assert.Equal(2, (await GetDetail(api, user, playlist.ID)).EntryList.Count);
    }

    [Fact]
    public async Task RemoveEntries_RemovesAndRenumbers()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> songList = await UploadSongList(api, user, 4);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        ResponsePlaylistDetail detail = await AddSongList(api, user, playlist.ID, songList);

        ResponsePlaylistDetail after = await Read<ResponsePlaylistDetail>(await api.RemovePlaylistEntryList(user.Token, playlist.ID, EntryBody([detail.EntryList[0].ID, detail.EntryList[2].ID])));

        Assert.Equal([songList[1], songList[3]], after.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([0, 1], after.EntryList.Select(x => x.Position));
        Assert.Equal([0, 1], (await GetDetail(api, user, playlist.ID)).EntryList.Select(x => x.Position));
    }

    [Fact]
    public async Task RemoveEntries_UnknownEntry_Returns404_AndRemovesNothing()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        ResponsePlaylistDetail detail = await AddSongList(api, user, playlist.ID, await UploadSongList(api, user, 2));

        Assert.Equal(HttpStatusCode.NotFound, (await api.RemovePlaylistEntryList(user.Token, playlist.ID, EntryBody([detail.EntryList[0].ID, Guid.CreateVersion7()]))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await api.RemovePlaylistEntryList(user.Token, playlist.ID, EntryBody([]))).StatusCode);

        Assert.Equal(2, (await GetDetail(api, user, playlist.ID)).EntryList.Count);
    }

    [Fact]
    public async Task MoveEntry_ShiftsTheOthers()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> songList = await UploadSongList(api, user, 4);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        ResponsePlaylistDetail detail = await AddSongList(api, user, playlist.ID, songList);

        ResponsePlaylistDetail down = await Read<ResponsePlaylistDetail>(await api.MovePlaylistEntry(user.Token, playlist.ID, detail.EntryList[0].ID, """{"position":2}"""));
        ResponsePlaylistDetail up = await Read<ResponsePlaylistDetail>(await api.MovePlaylistEntry(user.Token, playlist.ID, detail.EntryList[3].ID, """{"position":0}"""));

        Assert.Equal([songList[1], songList[2], songList[0], songList[3]], down.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([songList[3], songList[1], songList[2], songList[0]], up.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([0, 1, 2, 3], up.EntryList.Select(x => x.Position));
        Assert.Equal(up.EntryList.Select(x => x.MediaFileID), (await GetDetail(api, user, playlist.ID)).EntryList.Select(x => x.MediaFileID));
    }

    [Theory]
    [InlineData("""{"position":-1}""")]
    [InlineData("""{"position":3}""")]
    [InlineData("""{"position":"first"}""")]
    public async Task MoveEntry_OutOfRangeOrWrongType_Returns400(string json)
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        ResponsePlaylistDetail detail = await AddSongList(api, user, playlist.ID, await UploadSongList(api, user, 3));

        Assert.Equal(HttpStatusCode.BadRequest, (await api.MovePlaylistEntry(user.Token, playlist.ID, detail.EntryList[0].ID, json)).StatusCode);
    }

    [Fact]
    public async Task MoveEntry_UnknownEntryOrPlaylist_Returns404()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        ResponsePlaylistDetail detail = await AddSongList(api, user, playlist.ID, await UploadSongList(api, user, 2));

        Assert.Equal(HttpStatusCode.NotFound, (await api.MovePlaylistEntry(user.Token, playlist.ID, Guid.CreateVersion7(), """{"position":0}""")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.MovePlaylistEntry(user.Token, Guid.CreateVersion7(), detail.EntryList[0].ID, """{"position":0}""")).StatusCode);
    }

    #endregion -- Entries --

    #region -- Deleting a song --

    [Fact]
    public async Task DeleteSong_RemovesItFromPlaylists_AndPushesTheChange()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> songList = await UploadSongList(api, user, 3);
        ResponsePlaylist one = await CreatePlaylist(api, user, "One");
        ResponsePlaylist two = await CreatePlaylist(api, user, "Two");
        await AddSongList(api, user, one.ID, [songList[0], songList[1], songList[0], songList[2]]);
        await AddSongList(api, user, two.ID, [songList[1]]);
        string before = await LibraryETag(api, user);
        await using LibraryHubListener phone = await LibraryHubListener.Connect(factory, user.Token);

        Assert.Equal(HttpStatusCode.NoContent, (await api.Delete(user.Token, songList[0])).StatusCode);

        ResponsePlaylistDetail afterOne = await GetDetail(api, user, one.ID);
        Assert.Equal([songList[1], songList[2]], afterOne.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([0, 1], afterOne.EntryList.Select(x => x.Position));
        Assert.Single((await GetDetail(api, user, two.ID)).EntryList);
        Assert.NotEqual(before, await LibraryETag(api, user));

        ResponsePlaylist pushed = await phone.NextPlaylistUpdated();
        Assert.Equal(one.ID, pushed.ID);
        Assert.Equal(2, pushed.EntryCount);
        Assert.Equal(songList[0], await phone.NextMediaDeleted());
    }

    #endregion -- Deleting a song --

    #region -- Push --

    [Fact]
    public async Task Push_EveryChange_SendsPlaylistUpdatedToEveryConnectionOfTheUser_AndNotToAnotherUser()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("other@example.com");
        List<Guid> songList = await UploadSongList(api, user, 2);
        await using LibraryHubListener phone = await LibraryHubListener.Connect(factory, user.Token);
        await using LibraryHubListener computer = await LibraryHubListener.Connect(factory, user.Token, tokenInQueryString: true);
        await using LibraryHubListener stranger = await LibraryHubListener.Connect(factory, other.Token);

        ResponsePlaylist playlist = await CreatePlaylist(api, user, "Mix");
        Assert.Equal(playlist.ID, (await phone.NextPlaylistUpdated()).ID);
        Assert.Equal(playlist.ID, (await computer.NextPlaylistUpdated()).ID);

        ResponsePlaylistDetail detail = await AddSongList(api, user, playlist.ID, songList);
        ResponsePlaylist added = await phone.NextPlaylistUpdated();
        Assert.Equal(2, added.EntryCount);
        Assert.Equal(2, (await computer.NextPlaylistUpdated()).EntryCount);
        Assert.Equal(detail.TimeModified, added.TimeModified);

        await api.MovePlaylistEntry(user.Token, playlist.ID, detail.EntryList[0].ID, """{"position":1}""");
        Assert.Equal(playlist.ID, (await phone.NextPlaylistUpdated()).ID);
        Assert.Equal(playlist.ID, (await computer.NextPlaylistUpdated()).ID);

        await api.RemovePlaylistEntryList(user.Token, playlist.ID, EntryBody([detail.EntryList[0].ID]));
        Assert.Equal(1, (await phone.NextPlaylistUpdated()).EntryCount);
        Assert.Equal(1, (await computer.NextPlaylistUpdated()).EntryCount);

        await api.PatchPlaylist(user.Token, playlist.ID, """{"name":"Renamed"}""");
        Assert.Equal("Renamed", (await phone.NextPlaylistUpdated()).Name);
        Assert.Equal("Renamed", (await computer.NextPlaylistUpdated()).Name);

        Assert.True(await stranger.ReceivesNothingWithin(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public async Task Push_NoOpChange_PushesNothing()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> songList = await UploadSongList(api, user, 1);
        ResponsePlaylist playlist = await CreatePlaylist(api, user, "Mix", allowDuplicates: false);
        ResponsePlaylistDetail detail = await AddSongList(api, user, playlist.ID, songList);
        await using LibraryHubListener hub = await LibraryHubListener.Connect(factory, user.Token);

        await api.PatchPlaylist(user.Token, playlist.ID, """{"name":"Mix"}""");
        await api.MovePlaylistEntry(user.Token, playlist.ID, detail.EntryList[0].ID, """{"position":0}""");
        await api.AddPlaylistEntryList(user.Token, playlist.ID, SongBody(songList));

        Assert.True(await hub.ReceivesNothingWithin(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public async Task Push_Delete_SendsPlaylistDeletedWithTheId()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("other@example.com");
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        await using LibraryHubListener phone = await LibraryHubListener.Connect(factory, user.Token);
        await using LibraryHubListener computer = await LibraryHubListener.Connect(factory, user.Token);
        await using LibraryHubListener stranger = await LibraryHubListener.Connect(factory, other.Token);

        await api.DeletePlaylist(user.Token, playlist.ID);

        Assert.Equal(playlist.ID, await phone.NextPlaylistDeleted());
        Assert.Equal(playlist.ID, await computer.NextPlaylistDeleted());
        Assert.True(await stranger.ReceivesNothingWithin(TimeSpan.FromSeconds(1)));
    }

    #endregion -- Push --
}
