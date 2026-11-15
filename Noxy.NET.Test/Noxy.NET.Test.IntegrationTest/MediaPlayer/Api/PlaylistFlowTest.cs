using System.IO.Compression;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.Test.MediaPlayer.Fixtures;
using Noxy.NET.Test.MediaPlayer.Fixtures.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class PlaylistFlowTest
{
    private static async Task<List<Guid>> UploadMediaFileList(MediaApiClient api, ApiAccount user, int count)
    {
        for (int index = 1; index <= count; index++)
        {
            HttpResponseMessage response = await api.Upload(user.Token, MediaApiClient.Mp3(300 + index), $"Media {index}.mp3", $"Media {index}");
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            await api.WaitForMediaFileList(user.Token, index);
        }

        List<ResponseMediaFile> list = await api.GetMediaFileList(user.Token);
        return [.. list.OrderBy(x => x.Title).Select(x => x.ID)];
    }

    private static async Task<T> Read<T>(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await Deserialize<T>(response);
    }

    private static async Task<T> ReadCreated<T>(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await Deserialize<T>(response);
    }

    private static async Task<T> Deserialize<T>(HttpResponseMessage response)
    {
        return await response.ReadJson<T>();
    }

    private static async Task<ResponsePlaylist> CreatePlaylist(MediaApiClient api, ApiAccount user, string name = "Mix", bool allowDuplicates = true)
    {
        string json = JsonSerializer.Serialize(new { name, allowDuplicates });
        return await ReadCreated<ResponsePlaylist>(await api.CreatePlaylist(user.Token, json));
    }

    private static string MediaFileBody(IEnumerable<Guid> mediaFileIDList, int? position = null)
    {
        return JsonSerializer.Serialize(new { mediaFileIDList, position });
    }

    private static string EntryBody(IEnumerable<Guid> entryIDList)
    {
        return JsonSerializer.Serialize(new { entryIDList });
    }

    private static async Task<ResponsePlaylistDetail> AddMediaFileList(MediaApiClient api, ApiAccount user, Guid playlistID, IEnumerable<Guid> mediaFileIDList, int? position = null)
    {
        return await Read<ResponsePlaylistDetail>(await api.AddPlaylistEntryList(user.Token, playlistID, MediaFileBody(mediaFileIDList, position)));
    }

    private static async Task<ResponsePlaylistDetail> GetDetail(MediaApiClient api, ApiAccount user, Guid playlistID)
    {
        return await Read<ResponsePlaylistDetail>(await api.GetPlaylist(user.Token, playlistID));
    }

    private static async Task<string> LibraryETag(MediaApiClient api, ApiAccount user)
    {
        HttpResponseMessage response = await api.GetPlaylistLibrary(user.Token);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return response.GetETag();
    }

    private static ApiFactory Factory(string key, string value)
    {
        return new(new() { [ConfigurationPath.Combine(ConfigPlaylist.SectionName, key)] = value });
    }

    #region -- Library --

    [Fact]
    public async Task OldPlaylistsRoute_IsNotFound()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        HttpResponseMessage response = await api.GetPath(user.Token, "/Playlists");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Library_Returns200WithAnETagAndNoEntries_AndAMatchingRequestGets304()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponsePlaylist created = await CreatePlaylist(api, user, "Road trip");
        await AddMediaFileList(api, user, created.ID, await UploadMediaFileList(api, user, 2));

        HttpResponseMessage first = await api.GetPlaylistLibrary(user.Token);
        string eTag = first.GetETag();
        string body = await first.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        HttpResponseMessage second = await api.GetPlaylistLibrary(user.Token, eTag);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.DoesNotContain("entryList", body, StringComparison.OrdinalIgnoreCase);
        ResponsePlaylist listed = Assert.Single(JsonSerializer.Deserialize<List<ResponsePlaylist>>(body, JsonSerializerOptions.Web) ?? throw new InvalidOperationException("The body is not a playlist list."));
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
        List<Guid> mediaFileList = await UploadMediaFileList(api, user, 3);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        List<string> eTagList = [await LibraryETag(api, user)];

        async Task<HttpResponseMessage> Step(Func<Task<HttpResponseMessage>> change)
        {
            HttpResponseMessage response = await change();
            Assert.True(response.IsSuccessStatusCode, $"The change failed with {(int)response.StatusCode}.");
            eTagList.Add(await LibraryETag(api, user));
            return response;
        }

        await Step(() => api.PatchPlaylist(user.Token, playlist.ID, """{"name":"Renamed"}"""));
        await Step(() => api.PatchPlaylist(user.Token, playlist.ID, """{"allowDuplicates":false}"""));
        HttpResponseMessage added = await Step(() => api.AddPlaylistEntryList(user.Token, playlist.ID, MediaFileBody(mediaFileList)));
        ResponsePlaylistDetail detail = await added.ReadJson<ResponsePlaylistDetail>();
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
        List<Guid> mediaFileList = await UploadMediaFileList(api, user, 1);
        ResponsePlaylist playlist = await CreatePlaylist(api, user, allowDuplicates: false);
        ResponsePlaylistDetail detail = await AddMediaFileList(api, user, playlist.ID, mediaFileList);
        string before = await LibraryETag(api, user);

        await api.MovePlaylistEntry(user.Token, playlist.ID, detail.EntryList[0].ID, """{"position":0}""");
        await api.PatchPlaylist(user.Token, playlist.ID, """{"name":"Mix","allowDuplicates":false}""");
        await api.PatchPlaylist(user.Token, playlist.ID, "{}");
        await AddMediaFileList(api, user, playlist.ID, mediaFileList);

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

        Assert.Empty(await first.ReadJson<List<ResponsePlaylist>>());
        Assert.Equal(first.GetETag(), second.GetETag());
    }

    [Fact]
    public async Task Library_IsGzippedWhenAsked()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await CreatePlaylist(api, user);

        HttpResponseMessage response = await api.GetPlaylistLibrary(user.Token, acceptEncoding: "gzip");

        Assert.Contains("gzip", response.Content.Headers.ContentEncoding);
        await using GZipStream gzip = new(await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken), CompressionMode.Decompress);
        List<ResponsePlaylist> list = await JsonSerializer.DeserializeAsync<List<ResponsePlaylist>>(gzip, JsonSerializerOptions.Web, TestContext.Current.CancellationToken) ?? throw new InvalidOperationException("The body is not a playlist list.");
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
        Assert.Single(await (await api.GetPlaylistLibrary(user.Token)).ReadJson<List<ResponsePlaylist>>());
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
    public async Task Detail_EntriesCarryTheMediaFilesTitleAndArtist()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> mediaFileList = await UploadMediaFileList(api, user, 1);
        Assert.Equal(HttpStatusCode.OK, (await api.Patch(user.Token, mediaFileList[0], """{"artist":"Someone"}""")).StatusCode);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        await AddMediaFileList(api, user, playlist.ID, mediaFileList);

        ResponsePlaylistEntry entry = Assert.Single((await GetDetail(api, user, playlist.ID)).EntryList);

        Assert.Equal(mediaFileList[0], entry.MediaFile.ID);
        Assert.Equal("Media 1", entry.MediaFile.Title);
        Assert.Equal("Someone", entry.MediaFile.Artist);
    }

    [Fact]
    public async Task Detail_AfterPatchingAMediaFileTitle_AnOldIfNoneMatchGetsTheNewTitle()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> mediaFileList = await UploadMediaFileList(api, user, 1);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        await AddMediaFileList(api, user, playlist.ID, mediaFileList);
        string eTag = (await api.GetPlaylist(user.Token, playlist.ID)).GetETag();
        Assert.Equal(HttpStatusCode.OK, (await api.Patch(user.Token, mediaFileList[0], """{"title":"Renamed"}""")).StatusCode);

        HttpResponseMessage response = await api.GetPlaylist(user.Token, playlist.ID, eTag);

        ResponsePlaylistDetail detail = await Read<ResponsePlaylistDetail>(response);
        Assert.Equal("Renamed", detail.EntryList.Single().MediaFile.Title);
        Assert.NotEqual(eTag, response.GetETag());
    }

    [Fact]
    public async Task Get_ReturnsEntriesByPosition_And304OnAMatchingETag()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> mediaFileList = await UploadMediaFileList(api, user, 3);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        await AddMediaFileList(api, user, playlist.ID, mediaFileList);

        HttpResponseMessage first = await api.GetPlaylist(user.Token, playlist.ID);
        ResponsePlaylistDetail detail = await Read<ResponsePlaylistDetail>(first);
        HttpResponseMessage second = await api.GetPlaylist(user.Token, playlist.ID, first.GetETag());

        Assert.Equal(mediaFileList, detail.EntryList.Select(x => x.MediaFileID));
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

        ResponsePlaylist created = await ReadCreated<ResponsePlaylist>(response);
        Assert.Equal("Road trip", created.Name);
        Assert.True(created.AllowDuplicates);
        Assert.Equal(0, created.EntryCount);
        Assert.Equal(DateTimeKind.Utc, created.TimeModified.Kind);
        Assert.EndsWith($"/Playlist/{created.ID}", response.GetLocation().AbsolutePath);
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
        List<Guid> mediaFileList = await UploadMediaFileList(api, user, 1);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        await AddMediaFileList(api, user, playlist.ID, [mediaFileList[0], mediaFileList[0]]);

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
        List<Guid> mediaFileList = await UploadMediaFileList(api, user, 1);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        ResponsePlaylistDetail detail = await AddMediaFileList(api, user, playlist.ID, [mediaFileList[0], mediaFileList[0]]);

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
    public async Task Delete_Returns204_AndTheMediaFilesRemain()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("other@example.com");
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        await AddMediaFileList(api, user, playlist.ID, await UploadMediaFileList(api, user, 2));

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
        List<Guid> mediaFileList = await UploadMediaFileList(api, user, 3);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);

        await AddMediaFileList(api, user, playlist.ID, mediaFileList.Take(2));
        ResponsePlaylistDetail detail = await AddMediaFileList(api, user, playlist.ID, mediaFileList.Skip(2));

        Assert.Equal(mediaFileList, detail.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([0, 1, 2], detail.EntryList.Select(x => x.Position));
        Assert.Equal(3, detail.EntryCount);
    }

    [Fact]
    public async Task AddEntries_AtAPosition_ShiftsTheOthers()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> mediaFileList = await UploadMediaFileList(api, user, 4);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        await AddMediaFileList(api, user, playlist.ID, [mediaFileList[0], mediaFileList[1], mediaFileList[2]]);

        ResponsePlaylistDetail detail = await AddMediaFileList(api, user, playlist.ID, [mediaFileList[3]], 1);

        Assert.Equal([mediaFileList[0], mediaFileList[3], mediaFileList[1], mediaFileList[2]], detail.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([0, 1, 2, 3], detail.EntryList.Select(x => x.Position));
        Assert.Equal(detail.EntryList.Select(x => x.MediaFileID), (await GetDetail(api, user, playlist.ID)).EntryList.Select(x => x.MediaFileID));
    }

    [Fact]
    public async Task AddEntries_DuplicatesOff_SkipsRepeats_AndDuplicatesOnKeepsThem()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> mediaFileList = await UploadMediaFileList(api, user, 2);
        ResponsePlaylist strict = await CreatePlaylist(api, user, "Strict", allowDuplicates: false);
        ResponsePlaylist loose = await CreatePlaylist(api, user, "Loose");

        ResponsePlaylistDetail strictDetail = await AddMediaFileList(api, user, strict.ID, [mediaFileList[0], mediaFileList[0], mediaFileList[1]]);
        ResponsePlaylistDetail again = await AddMediaFileList(api, user, strict.ID, [mediaFileList[1]]);
        ResponsePlaylistDetail looseDetail = await AddMediaFileList(api, user, loose.ID, [mediaFileList[0], mediaFileList[0]]);

        Assert.Equal([mediaFileList[0], mediaFileList[1]], strictDetail.EntryList.Select(x => x.MediaFileID));
        Assert.Equal(2, again.EntryList.Count);
        Assert.Equal([mediaFileList[0], mediaFileList[0]], looseDetail.EntryList.Select(x => x.MediaFileID));
    }

    [Fact]
    public async Task AddEntries_UnknownOrOtherUsersMediaFile_Returns404_AndAddsNothing()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("other@example.com");
        List<Guid> mine = await UploadMediaFileList(api, user, 1);
        List<Guid> theirs = await UploadMediaFileList(api, other, 1);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);

        Assert.Equal(HttpStatusCode.NotFound, (await api.AddPlaylistEntryList(user.Token, playlist.ID, MediaFileBody([mine[0], theirs[0]]))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.AddPlaylistEntryList(user.Token, playlist.ID, MediaFileBody([Guid.CreateVersion7()]))).StatusCode);

        Assert.Empty((await GetDetail(api, user, playlist.ID)).EntryList);
    }

    [Fact]
    public async Task AddEntries_ToAnotherUsersPlaylist_Returns404()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("other@example.com");
        List<Guid> theirs = await UploadMediaFileList(api, other, 1);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);

        Assert.Equal(HttpStatusCode.NotFound, (await api.AddPlaylistEntryList(other.Token, playlist.ID, MediaFileBody(theirs))).StatusCode);
    }

    [Theory]
    [InlineData("""{"mediaFileIDList":[]}""")]
    [InlineData("""{}""")]
    [InlineData("""{"mediaFileIDList":["not-a-guid"]}""")]
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
        List<Guid> mediaFileList = await UploadMediaFileList(api, user, 1);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);

        Assert.Equal(HttpStatusCode.BadRequest, (await api.AddPlaylistEntryList(user.Token, playlist.ID, MediaFileBody(mediaFileList, -1))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await api.AddPlaylistEntryList(user.Token, playlist.ID, MediaFileBody(mediaFileList, 5))).StatusCode);
        Assert.Empty((await GetDetail(api, user, playlist.ID)).EntryList);
    }

    [Fact]
    public async Task AddEntries_PastTheEntryCap_Returns409()
    {
        await using ApiFactory factory = Factory(nameof(ConfigPlaylist.MaxEntriesPerPlaylist), "2");
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> mediaFileList = await UploadMediaFileList(api, user, 3);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        await AddMediaFileList(api, user, playlist.ID, mediaFileList.Take(2));

        Assert.Equal(HttpStatusCode.Conflict, (await api.AddPlaylistEntryList(user.Token, playlist.ID, MediaFileBody([mediaFileList[2]]))).StatusCode);
        Assert.Equal(2, (await GetDetail(api, user, playlist.ID)).EntryList.Count);
    }

    [Fact]
    public async Task RemoveEntries_RemovesAndRenumbers()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> mediaFileList = await UploadMediaFileList(api, user, 4);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        ResponsePlaylistDetail detail = await AddMediaFileList(api, user, playlist.ID, mediaFileList);

        ResponsePlaylistDetail after = await Read<ResponsePlaylistDetail>(await api.RemovePlaylistEntryList(user.Token, playlist.ID, EntryBody([detail.EntryList[0].ID, detail.EntryList[2].ID])));

        Assert.Equal([mediaFileList[1], mediaFileList[3]], after.EntryList.Select(x => x.MediaFileID));
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
        ResponsePlaylistDetail detail = await AddMediaFileList(api, user, playlist.ID, await UploadMediaFileList(api, user, 2));

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
        List<Guid> mediaFileList = await UploadMediaFileList(api, user, 4);
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        ResponsePlaylistDetail detail = await AddMediaFileList(api, user, playlist.ID, mediaFileList);

        ResponsePlaylistDetail down = await Read<ResponsePlaylistDetail>(await api.MovePlaylistEntry(user.Token, playlist.ID, detail.EntryList[0].ID, """{"position":2}"""));
        ResponsePlaylistDetail up = await Read<ResponsePlaylistDetail>(await api.MovePlaylistEntry(user.Token, playlist.ID, detail.EntryList[3].ID, """{"position":0}"""));

        Assert.Equal([mediaFileList[1], mediaFileList[2], mediaFileList[0], mediaFileList[3]], down.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([mediaFileList[3], mediaFileList[1], mediaFileList[2], mediaFileList[0]], up.EntryList.Select(x => x.MediaFileID));
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
        ResponsePlaylistDetail detail = await AddMediaFileList(api, user, playlist.ID, await UploadMediaFileList(api, user, 3));

        Assert.Equal(HttpStatusCode.BadRequest, (await api.MovePlaylistEntry(user.Token, playlist.ID, detail.EntryList[0].ID, json)).StatusCode);
    }

    [Fact]
    public async Task MoveEntry_UnknownEntryOrPlaylist_Returns404()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        ResponsePlaylistDetail detail = await AddMediaFileList(api, user, playlist.ID, await UploadMediaFileList(api, user, 2));

        Assert.Equal(HttpStatusCode.NotFound, (await api.MovePlaylistEntry(user.Token, playlist.ID, Guid.CreateVersion7(), """{"position":0}""")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.MovePlaylistEntry(user.Token, Guid.CreateVersion7(), detail.EntryList[0].ID, """{"position":0}""")).StatusCode);
    }

    #endregion -- Entries --

    #region -- Deleting a mediaFile --

    [Fact]
    public async Task DeleteMediaFile_RemovesItFromPlaylists_AndPushesTheChange()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<Guid> mediaFileList = await UploadMediaFileList(api, user, 3);
        ResponsePlaylist one = await CreatePlaylist(api, user, "One");
        ResponsePlaylist two = await CreatePlaylist(api, user, "Two");
        await AddMediaFileList(api, user, one.ID, [mediaFileList[0], mediaFileList[1], mediaFileList[0], mediaFileList[2]]);
        await AddMediaFileList(api, user, two.ID, [mediaFileList[1]]);
        string before = await LibraryETag(api, user);
        await using LibraryHubListener phone = await LibraryHubListener.Connect(factory, user.Token);

        Assert.Equal(HttpStatusCode.NoContent, (await api.Delete(user.Token, mediaFileList[0])).StatusCode);

        ResponsePlaylistDetail afterOne = await GetDetail(api, user, one.ID);
        Assert.Equal([mediaFileList[1], mediaFileList[2]], afterOne.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([0, 1], afterOne.EntryList.Select(x => x.Position));
        Assert.Single((await GetDetail(api, user, two.ID)).EntryList);
        Assert.NotEqual(before, await LibraryETag(api, user));

        ResponsePlaylist pushed = await phone.NextPlaylistUpdated();
        Assert.Equal(one.ID, pushed.ID);
        Assert.Equal(2, pushed.EntryCount);
        Assert.Equal(mediaFileList[0], await phone.NextMediaDeleted());
    }

    #endregion -- Deleting a mediaFile --

    #region -- Push --

    [Fact]
    public async Task Push_APlaylistUpdate_ReachesAConnectionExactlyOnce()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using LibraryHubListener phone = await LibraryHubListener.Connect(factory, user.Token);

        await CreatePlaylist(api, user);

        await phone.NextPlaylistUpdated();
        Assert.Equal(1, await phone.CountReceivedAfter(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public async Task Push_EveryChange_SendsPlaylistUpdatedToEveryConnectionOfTheUser_AndNotToAnotherUser()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("other@example.com");
        List<Guid> mediaFileList = await UploadMediaFileList(api, user, 2);
        await using LibraryHubListener phone = await LibraryHubListener.Connect(factory, user.Token);
        await using LibraryHubListener computer = await LibraryHubListener.Connect(factory, user.Token, tokenInQueryString: true);
        await using LibraryHubListener stranger = await LibraryHubListener.Connect(factory, other.Token);

        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        Assert.Equal(playlist.ID, (await phone.NextPlaylistUpdated()).ID);
        Assert.Equal(playlist.ID, (await computer.NextPlaylistUpdated()).ID);

        ResponsePlaylistDetail detail = await AddMediaFileList(api, user, playlist.ID, mediaFileList);
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
        List<Guid> mediaFileList = await UploadMediaFileList(api, user, 1);
        ResponsePlaylist playlist = await CreatePlaylist(api, user, allowDuplicates: false);
        ResponsePlaylistDetail detail = await AddMediaFileList(api, user, playlist.ID, mediaFileList);
        await using LibraryHubListener hub = await LibraryHubListener.Connect(factory, user.Token);

        await api.PatchPlaylist(user.Token, playlist.ID, """{"name":"Mix"}""");
        await api.MovePlaylistEntry(user.Token, playlist.ID, detail.EntryList[0].ID, """{"position":0}""");
        await api.AddPlaylistEntryList(user.Token, playlist.ID, MediaFileBody(mediaFileList));

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

    #region -- Played --

    private static string PlayedPath(Guid id)
    {
        return $"/Playlist/{id}/Played";
    }

    [Fact]
    public async Task Played_Is204_MovesTheLibraryETagButNeitherTheDetailETagNorTheModifiedTime_AndPushesTheStamp()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        HttpResponseMessage detailBefore = await api.GetPlaylist(user.Token, playlist.ID);
        string libraryBefore = await LibraryETag(api, user);
        await using LibraryHubListener phone = await LibraryHubListener.Connect(factory, user.Token);

        HttpResponseMessage played = await api.SendJson(user.Token, HttpMethod.Post, PlayedPath(playlist.ID));

        Assert.Equal(HttpStatusCode.NoContent, played.StatusCode);
        Assert.NotEqual(libraryBefore, await LibraryETag(api, user));
        Assert.Equal(HttpStatusCode.NotModified, (await api.GetPlaylist(user.Token, playlist.ID, detailBefore.GetETag())).StatusCode);

        ResponsePlaylist pushed = await phone.NextPlaylistUpdated();
        Assert.Equal(playlist.ID, pushed.ID);
        Assert.NotNull(pushed.TimeLastPlayed);
        Assert.Equal(playlist.TimeModified, pushed.TimeModified);

        HttpResponseMessage libraryResponse = await api.GetPlaylistLibrary(user.Token);
        ResponsePlaylist listed = (await Read<List<ResponsePlaylist>>(libraryResponse)).Single();
        Assert.Equal(pushed.TimeLastPlayed, listed.TimeLastPlayed);
        Assert.Equal(playlist.TimeModified, listed.TimeModified);
    }

    [Fact]
    public async Task Played_ABurstInsideTheWindow_WritesAndPushesOnce()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        await using LibraryHubListener phone = await LibraryHubListener.Connect(factory, user.Token);

        for (int index = 0; index < 3; index++)
        {
            Assert.Equal(HttpStatusCode.NoContent, (await api.SendJson(user.Token, HttpMethod.Post, PlayedPath(playlist.ID))).StatusCode);
        }

        await phone.NextPlaylistUpdated();
        Assert.Equal(1, await phone.CountReceivedAfter(TimeSpan.FromMilliseconds(700)));
    }

    [Fact]
    public async Task Played_WithAZeroWindow_UpdatesEveryTime()
    {
        await using ApiFactory factory = new(new() { [ConfigurationPath.Combine(ConfigPlayback.SectionName, nameof(ConfigPlayback.PlayedWindowSeconds))] = "0" });
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        await using LibraryHubListener phone = await LibraryHubListener.Connect(factory, user.Token);

        await api.SendJson(user.Token, HttpMethod.Post, PlayedPath(playlist.ID));
        await Task.Delay(20, TestContext.Current.CancellationToken);
        await api.SendJson(user.Token, HttpMethod.Post, PlayedPath(playlist.ID));

        ResponsePlaylist first = await phone.NextPlaylistUpdated();
        ResponsePlaylist second = await phone.NextPlaylistUpdated();
        Assert.True(second.TimeLastPlayed > first.TimeLastPlayed);
    }

    [Fact]
    public async Task Played_AnotherUsersAnUnknownOrAMalformedPlaylist_Is404_AndWithoutATokenIs401()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("b@example.com");
        ResponsePlaylist playlist = await CreatePlaylist(api, user);
        await using LibraryHubListener stranger = await LibraryHubListener.Connect(factory, other.Token);

        Assert.Equal(HttpStatusCode.NotFound, (await api.SendJson(other.Token, HttpMethod.Post, PlayedPath(playlist.ID))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.SendJson(user.Token, HttpMethod.Post, PlayedPath(Guid.CreateVersion7()))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.SendJson(user.Token, HttpMethod.Post, "/Playlist/not-a-guid/Played")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.SendJson(null, HttpMethod.Post, PlayedPath(playlist.ID))).StatusCode);

        Assert.Null((await Read<List<ResponsePlaylist>>(await api.GetPlaylistLibrary(user.Token))).Single().TimeLastPlayed);
        Assert.True(await stranger.ReceivesNothingWithin(TimeSpan.FromMilliseconds(500)));
    }

    #endregion -- Played --
}
