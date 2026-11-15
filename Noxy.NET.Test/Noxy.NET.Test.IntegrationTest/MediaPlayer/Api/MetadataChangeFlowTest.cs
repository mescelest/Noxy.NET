using System.Net;
using System.Text.Json;
using Noxy.NET.MediaPlayer.API;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.Test.MediaPlayer.Fixtures;
using Noxy.NET.Test.MediaPlayer.Fixtures.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class MetadataChangeFlowTest
{
    private static async Task<ResponseMediaFile> UploadMediaFile(MediaApiClient api, ApiAccount user, string title, int alreadyThere = 0)
    {
        HttpResponseMessage response = await api.Upload(user.Token, MediaApiClient.Mp3(300 + alreadyThere), $"{title}.mp3", title);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        return (await api.WaitForMediaFileList(user.Token, alreadyThere + 1)).Single(x => x.Title == title);
    }

    private static async Task<T> Send<T>(MediaApiClient api, ApiAccount user, HttpMethod method, string path, string? json = null, HttpStatusCode expected = HttpStatusCode.OK)
    {
        HttpResponseMessage response = await api.SendJson(user.Token, method, path, json);
        Assert.Equal(expected, response.StatusCode);
        return await response.ReadJson<T>();
    }

    private static string Json(object body)
    {
        return JsonSerializer.Serialize(body);
    }

    #region -- Artist --

    [Fact]
    public async Task Artist_CreateRenameAndDelete_WorkAndTheListFollows()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        ResponseArtist created = await Send<ResponseArtist>(api, user, HttpMethod.Post, "/Artist", """{"name":"  Radiohead "}""", HttpStatusCode.Created);
        ResponseArtist renamed = await Send<ResponseArtist>(api, user, HttpMethod.Patch, $"/Artist/{created.ID}", """{"name":"Thom Yorke"}""");
        List<ResponseArtist> list = await Send<List<ResponseArtist>>(api, user, HttpMethod.Get, "/Artist");
        HttpResponseMessage deleted = await api.SendJson(user.Token, HttpMethod.Delete, $"/Artist/{created.ID}");

        Assert.Equal("Radiohead", created.Name);
        Assert.Equal("Thom Yorke", renamed.Name);
        Assert.Equal(created.ID, Assert.Single(list).ID);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal("[]", await (await api.GetPath(user.Token, "/Artist")).Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Artist_ANameThatIsTaken_Is409NamingTheExistingArtist()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseArtist first = await Send<ResponseArtist>(api, user, HttpMethod.Post, "/Artist", """{"name":"A"}""", HttpStatusCode.Created);
        ResponseArtist second = await Send<ResponseArtist>(api, user, HttpMethod.Post, "/Artist", """{"name":"B"}""", HttpStatusCode.Created);

        HttpResponseMessage duplicate = await api.SendJson(user.Token, HttpMethod.Post, "/Artist", """{"name":" a "}""");
        HttpResponseMessage collision = await api.SendJson(user.Token, HttpMethod.Patch, $"/Artist/{second.ID}", """{"name":"A"}""");

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal(first.ID, (await duplicate.ReadJson<JsonElement>()).GetProperty(ApiExceptionHandler.ExistingIDKey).GetGuid());
        Assert.Equal(HttpStatusCode.Conflict, collision.StatusCode);
        Assert.Equal(first.ID, (await collision.ReadJson<JsonElement>()).GetProperty(ApiExceptionHandler.ExistingIDKey).GetGuid());
    }

    [Fact]
    public async Task Artist_InvalidNames_Return400_AndAnotherUsersArtistReturns404()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("b@example.com");
        ResponseArtist theirs = await Send<ResponseArtist>(api, other, HttpMethod.Post, "/Artist", """{"name":"Theirs"}""", HttpStatusCode.Created);

        Assert.Equal(HttpStatusCode.BadRequest, (await api.SendJson(user.Token, HttpMethod.Post, "/Artist", """{"name":"  "}""")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await api.SendJson(user.Token, HttpMethod.Post, "/Artist", Json(new { name = new string('a', 256) }))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.SendJson(user.Token, HttpMethod.Patch, $"/Artist/{theirs.ID}", """{"name":"Mine"}""")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.SendJson(user.Token, HttpMethod.Delete, $"/Artist/{theirs.ID}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.SendJson(user.Token, HttpMethod.Delete, $"/Artist/{Guid.CreateVersion7()}")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.SendJson(null, HttpMethod.Post, "/Artist", """{"name":"x"}""")).StatusCode);
    }

    [Fact]
    public async Task Artist_TheSameNameForTwoUsers_IsAllowed()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("b@example.com");

        await Send<ResponseArtist>(api, user, HttpMethod.Post, "/Artist", """{"name":"Same"}""", HttpStatusCode.Created);
        await Send<ResponseArtist>(api, other, HttpMethod.Post, "/Artist", """{"name":"Same"}""", HttpStatusCode.Created);
    }

    [Fact]
    public async Task Artist_Rename_ChangesTheTextAndTheModifiedTimeOfItsFiles_AndTheLibraryETag()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile file = await UploadMediaFile(api, user, "One");
        ResponseMediaFile linked = await Send<ResponseMediaFile>(api, user, HttpMethod.Patch, $"/Media/{file.ID}", """{"artist":"Old","album":"Record","albumArtist":"Old"}""");
        string eTagBefore = (await api.GetLibrary(user.Token)).GetETag();

        await Send<ResponseArtist>(api, user, HttpMethod.Patch, $"/Artist/{linked.ArtistIDList[0]}", """{"name":"New"}""");

        ResponseMediaFile after = Assert.Single(await api.GetMediaFileList(user.Token));
        Assert.Equal("New", after.Artist);
        Assert.Equal("New", after.AlbumArtist);
        Assert.True(after.TimeModified > linked.TimeModified);
        Assert.NotEqual(eTagBefore, (await api.GetLibrary(user.Token)).GetETag());
    }

    [Fact]
    public async Task Artist_Delete_KeepsTheFilesAndUnlinksThem_AndAnAlbumArtistRemovalKeepsTheAlbum()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile file = await UploadMediaFile(api, user, "One");
        ResponseMediaFile linked = await Send<ResponseMediaFile>(api, user, HttpMethod.Patch, $"/Media/{file.ID}", """{"artist":"Gone","album":"Record","albumArtist":"Gone"}""");

        HttpResponseMessage deleted = await api.SendJson(user.Token, HttpMethod.Delete, $"/Artist/{linked.ArtistIDList[0]}");

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        ResponseMediaFile after = Assert.Single(await api.GetMediaFileList(user.Token));
        Assert.Empty(after.ArtistIDList);
        Assert.Null(after.Artist);
        Assert.Equal(linked.AlbumID, after.AlbumID);
        Assert.Empty(after.AlbumArtistIDList);
        ResponseAlbum album = Assert.Single(await Send<List<ResponseAlbum>>(api, user, HttpMethod.Get, "/Album"));
        Assert.Empty(album.ArtistIDList);
        Assert.Equal(1, album.MediaFileCount);
    }

    [Fact]
    public async Task Artist_RenameOntoAnotherName_MergesWhenTheSettingIsOn()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile file = await UploadMediaFile(api, user, "One");
        ResponseMediaFile linked = await Send<ResponseMediaFile>(api, user, HttpMethod.Patch, $"/Media/{file.ID}", """{"artist":"A"}""");
        ResponseArtist other = await Send<ResponseArtist>(api, user, HttpMethod.Post, "/Artist", """{"name":"B"}""", HttpStatusCode.Created);
        await Send<ResponseSetting>(api, user, HttpMethod.Patch, $"/Settings/{SettingKey.MergeOnRename}", """{"value":true}""");

        ResponseArtist merged = await Send<ResponseArtist>(api, user, HttpMethod.Patch, $"/Artist/{linked.ArtistIDList[0]}", """{"name":"b"}""");

        Assert.Equal(other.ID, merged.ID);
        Assert.Equal("B", merged.Name);
        Assert.Equal(1, merged.MediaFileCount);
        Assert.Equal(other.ID, Assert.Single(await Send<List<ResponseArtist>>(api, user, HttpMethod.Get, "/Artist")).ID);
        Assert.Equal([other.ID], Assert.Single(await api.GetMediaFileList(user.Token)).ArtistIDList);
    }

    #endregion -- Artist --

    #region -- Genre --

    [Fact]
    public async Task Genre_CreateRenameAndDelete_Work()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile file = await UploadMediaFile(api, user, "One");
        ResponseMediaFile linked = await Send<ResponseMediaFile>(api, user, HttpMethod.Patch, $"/Media/{file.ID}", """{"genre":"Rock; Pop"}""");

        ResponseGenre created = await Send<ResponseGenre>(api, user, HttpMethod.Post, "/Genre", """{"name":"Jazz"}""", HttpStatusCode.Created);
        ResponseGenre renamed = await Send<ResponseGenre>(api, user, HttpMethod.Patch, $"/Genre/{linked.GenreIDList[0]}", """{"name":"Hard Rock"}""");
        HttpResponseMessage deleted = await api.SendJson(user.Token, HttpMethod.Delete, $"/Genre/{linked.GenreIDList[1]}");

        Assert.Equal("Jazz", created.Name);
        Assert.Equal("Hard Rock", renamed.Name);
        Assert.Equal(1, renamed.MediaFileCount);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal("Hard Rock", Assert.Single(await api.GetMediaFileList(user.Token)).Genre);
        Assert.Equal(2, (await Send<List<ResponseGenre>>(api, user, HttpMethod.Get, "/Genre")).Count);
        Assert.Equal(HttpStatusCode.Conflict, (await api.SendJson(user.Token, HttpMethod.Post, "/Genre", """{"name":" jazz"}""")).StatusCode);
    }

    #endregion -- Genre --

    #region -- Album --

    [Fact]
    public async Task Album_CreateUpdateAndDelete_Work()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseArtist artist = await Send<ResponseArtist>(api, user, HttpMethod.Post, "/Artist", """{"name":"Queen"}""", HttpStatusCode.Created);
        ResponseMediaFile file = await UploadMediaFile(api, user, "One");

        ResponseAlbum created = await Send<ResponseAlbum>(api, user, HttpMethod.Post, "/Album", Json(new { title = "Greatest Hits", artistIDList = new[] { artist.ID } }), HttpStatusCode.Created);
        await Send<ResponseMediaFile>(api, user, HttpMethod.Patch, $"/Media/{file.ID}", Json(new { albumID = created.ID }));
        ResponseAlbum renamed = await Send<ResponseAlbum>(api, user, HttpMethod.Patch, $"/Album/{created.ID}", """{"title":"Live"}""");
        ResponseMediaFile after = Assert.Single(await api.GetMediaFileList(user.Token));
        HttpResponseMessage deleted = await api.SendJson(user.Token, HttpMethod.Delete, $"/Album/{created.ID}");

        Assert.Equal([artist.ID], created.ArtistIDList);
        Assert.Equal("Live", renamed.Title);
        Assert.Equal(1, renamed.MediaFileCount);
        Assert.Equal("Live", after.Album);
        Assert.Equal("Queen", after.AlbumArtist);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        ResponseMediaFile final = Assert.Single(await api.GetMediaFileList(user.Token));
        Assert.Null(final.AlbumID);
        Assert.Null(final.Album);
        Assert.Equal("[]", await (await api.GetPath(user.Token, "/Album")).Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Album_TheSameTitleAndArtists_Is409_AndAnUnknownArtistIs404()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseAlbum first = await Send<ResponseAlbum>(api, user, HttpMethod.Post, "/Album", """{"title":"Record"}""", HttpStatusCode.Created);

        HttpResponseMessage duplicate = await api.SendJson(user.Token, HttpMethod.Post, "/Album", """{"title":" RECORD "}""");
        HttpResponseMessage unknownArtist = await api.SendJson(user.Token, HttpMethod.Post, "/Album", Json(new { title = "Other", artistIDList = new[] { Guid.CreateVersion7() } }));

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal(first.ID, (await duplicate.ReadJson<JsonElement>()).GetProperty(ApiExceptionHandler.ExistingIDKey).GetGuid());
        Assert.Equal(HttpStatusCode.NotFound, unknownArtist.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await api.SendJson(user.Token, HttpMethod.Post, "/Album", """{"title":""}""")).StatusCode);
    }

    #endregion -- Album --
}
