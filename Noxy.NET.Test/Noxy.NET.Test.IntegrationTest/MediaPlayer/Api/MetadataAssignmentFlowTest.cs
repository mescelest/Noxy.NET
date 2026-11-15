using System.Net;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fixtures;
using Noxy.NET.Test.MediaPlayer.Fixtures.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class MetadataAssignmentFlowTest
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

    [Fact]
    public async Task Artist_AssignAndRemove_ChangeTheFilesAndTheCount()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile first = await UploadMediaFile(api, user, "One");
        ResponseMediaFile second = await UploadMediaFile(api, user, "Two", 1);
        ResponseArtist artist = await Send<ResponseArtist>(api, user, HttpMethod.Post, "/Artist", """{"name":"A"}""", HttpStatusCode.Created);

        ResponseArtist assigned = await Send<ResponseArtist>(api, user, HttpMethod.Post, $"/Artist/{artist.ID}/MediaFileList", Json(new { mediaFileIDList = new[] { first.ID, second.ID } }));
        ResponseArtist again = await Send<ResponseArtist>(api, user, HttpMethod.Post, $"/Artist/{artist.ID}/MediaFileList", Json(new { mediaFileIDList = new[] { first.ID } }));
        List<ResponseMediaFile> afterAssign = await api.GetMediaFileList(user.Token);
        ResponseArtist removed = await Send<ResponseArtist>(api, user, HttpMethod.Post, $"/Artist/{artist.ID}/MediaFileList/Remove", Json(new { mediaFileIDList = new[] { first.ID } }));
        List<ResponseMediaFile> afterRemove = await api.GetMediaFileList(user.Token);

        Assert.Equal(2, assigned.MediaFileCount);
        Assert.Equal(2, again.MediaFileCount);
        Assert.All(afterAssign, x => Assert.Equal([artist.ID], x.ArtistIDList));
        Assert.Equal(1, removed.MediaFileCount);
        Assert.Empty(afterRemove.Single(x => x.ID == first.ID).ArtistIDList);
        Assert.Equal([artist.ID], afterRemove.Single(x => x.ID == second.ID).ArtistIDList);
    }

    [Fact]
    public async Task Artist_Merge_MovesTheFilesAndTheAlbumArtist_AndRemovesTheSource()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile first = await UploadMediaFile(api, user, "One");
        ResponseMediaFile second = await UploadMediaFile(api, user, "Two", 1);
        ResponseMediaFile sourceFile = await Send<ResponseMediaFile>(api, user, HttpMethod.Patch, $"/Media/{first.ID}", """{"artist":"Source","album":"Record","albumArtist":"Source"}""");
        ResponseMediaFile targetFile = await Send<ResponseMediaFile>(api, user, HttpMethod.Patch, $"/Media/{second.ID}", """{"artist":"Target","album":"Record","albumArtist":"Target"}""");

        ResponseArtist merged = await Send<ResponseArtist>(api, user, HttpMethod.Post, $"/Artist/{sourceFile.ArtistIDList[0]}/Merge", Json(new { targetID = targetFile.ArtistIDList[0] }));

        Assert.Equal(targetFile.ArtistIDList[0], merged.ID);
        Assert.Equal(2, merged.MediaFileCount);
        Assert.Equal(targetFile.ArtistIDList[0], Assert.Single(await Send<List<ResponseArtist>>(api, user, HttpMethod.Get, "/Artist")).ID);
        List<ResponseMediaFile> files = await api.GetMediaFileList(user.Token);
        Assert.All(files, x => Assert.Equal("Target", x.Artist));
        Assert.All(files, x => Assert.Equal("Target", x.AlbumArtist));
        ResponseAlbum album = Assert.Single(await Send<List<ResponseAlbum>>(api, user, HttpMethod.Get, "/Album"));
        Assert.Equal(2, album.MediaFileCount);
        Assert.Equal([targetFile.ArtistIDList[0]], album.ArtistIDList);
        Assert.All(files, x => Assert.Equal(album.ID, x.AlbumID));
    }

    [Fact]
    public async Task Artist_Split_ReplacesItOnTheFilesAndInTheAlbumArtistList()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile file = await UploadMediaFile(api, user, "One");
        ResponseMediaFile linked = await Send<ResponseMediaFile>(api, user, HttpMethod.Patch, $"/Media/{file.ID}", """{"artist":"Simon & Garfunkel","album":"Record","albumArtist":"Simon & Garfunkel"}""");
        ResponseArtist existing = await Send<ResponseArtist>(api, user, HttpMethod.Post, "/Artist", """{"name":"Garfunkel"}""", HttpStatusCode.Created);

        List<ResponseArtist> result = await Send<List<ResponseArtist>>(api, user, HttpMethod.Post, $"/Artist/{linked.ArtistIDList[0]}/Split", Json(new { nameList = new[] { "Simon", "garfunkel" } }));

        Assert.Equal(["Simon", "Garfunkel"], result.Select(x => x.Name).ToList());
        Assert.Equal(existing.ID, result[1].ID);
        ResponseMediaFile after = Assert.Single(await api.GetMediaFileList(user.Token));
        Assert.Equal("Simon; Garfunkel", after.Artist);
        Assert.Equal("Simon; Garfunkel", after.AlbumArtist);
        Assert.Equal(2, (await Send<List<ResponseArtist>>(api, user, HttpMethod.Get, "/Artist")).Count);
        Assert.Equal(2, Assert.Single(await Send<List<ResponseAlbum>>(api, user, HttpMethod.Get, "/Album")).ArtistIDList.Count);
    }

    [Fact]
    public async Task Genre_SplitAndMerge_Work()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile file = await UploadMediaFile(api, user, "One");
        ResponseMediaFile linked = await Send<ResponseMediaFile>(api, user, HttpMethod.Patch, $"/Media/{file.ID}", """{"genre":"Rock & Roll; Pop"}""");

        List<ResponseGenre> split = await Send<List<ResponseGenre>>(api, user, HttpMethod.Post, $"/Genre/{linked.GenreIDList[0]}/Split", Json(new { nameList = new[] { "Rock", "Roll" } }));
        ResponseGenre merged = await Send<ResponseGenre>(api, user, HttpMethod.Post, $"/Genre/{split[1].ID}/Merge", Json(new { targetID = split[0].ID }));

        Assert.Equal(["Rock", "Roll"], split.Select(x => x.Name).ToList());
        Assert.Equal(split[0].ID, merged.ID);
        Assert.Equal("Rock; Pop", Assert.Single(await api.GetMediaFileList(user.Token)).Genre);
        Assert.Equal(2, (await Send<List<ResponseGenre>>(api, user, HttpMethod.Get, "/Genre")).Count);
    }

    [Fact]
    public async Task Album_AddRemoveAndMerge_Work()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile first = await UploadMediaFile(api, user, "One");
        ResponseMediaFile second = await UploadMediaFile(api, user, "Two", 1);
        ResponseAlbum one = await Send<ResponseAlbum>(api, user, HttpMethod.Post, "/Album", """{"title":"One"}""", HttpStatusCode.Created);
        ResponseAlbum two = await Send<ResponseAlbum>(api, user, HttpMethod.Post, "/Album", """{"title":"Two"}""", HttpStatusCode.Created);

        ResponseAlbum added = await Send<ResponseAlbum>(api, user, HttpMethod.Post, $"/Album/{one.ID}/MediaFileList", Json(new { mediaFileIDList = new[] { first.ID, second.ID } }));
        ResponseAlbum replaced = await Send<ResponseAlbum>(api, user, HttpMethod.Post, $"/Album/{two.ID}/MediaFileList", Json(new { mediaFileIDList = new[] { second.ID } }));
        ResponseAlbum removed = await Send<ResponseAlbum>(api, user, HttpMethod.Post, $"/Album/{two.ID}/MediaFileList/Remove", Json(new { mediaFileIDList = new[] { second.ID } }));
        ResponseAlbum merged = await Send<ResponseAlbum>(api, user, HttpMethod.Post, $"/Album/{two.ID}/Merge", Json(new { targetID = one.ID }));

        Assert.Equal(2, added.MediaFileCount);
        Assert.Equal(1, replaced.MediaFileCount);
        Assert.Equal(0, removed.MediaFileCount);
        Assert.Equal(one.ID, merged.ID);
        Assert.Equal(one.ID, Assert.Single(await Send<List<ResponseAlbum>>(api, user, HttpMethod.Get, "/Album")).ID);
    }

    [Fact]
    public async Task AssignMergeAndSplit_InvalidOrForeign_Return400Or404_AndChangeNothing()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("b@example.com");
        ResponseMediaFile file = await UploadMediaFile(api, user, "One");
        ResponseMediaFile foreignFile = await UploadMediaFile(api, other, "Theirs");
        ResponseArtist artist = await Send<ResponseArtist>(api, user, HttpMethod.Post, "/Artist", """{"name":"A"}""", HttpStatusCode.Created);
        ResponseArtist foreignArtist = await Send<ResponseArtist>(api, other, HttpMethod.Post, "/Artist", """{"name":"Theirs"}""", HttpStatusCode.Created);

        Assert.Equal(HttpStatusCode.BadRequest, (await api.SendJson(user.Token, HttpMethod.Post, $"/Artist/{artist.ID}/MediaFileList", """{"mediaFileIDList":[]}""")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.SendJson(user.Token, HttpMethod.Post, $"/Artist/{artist.ID}/MediaFileList", Json(new { mediaFileIDList = new[] { file.ID, foreignFile.ID } }))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.SendJson(user.Token, HttpMethod.Post, $"/Artist/{foreignArtist.ID}/MediaFileList", Json(new { mediaFileIDList = new[] { file.ID } }))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await api.SendJson(user.Token, HttpMethod.Post, $"/Artist/{artist.ID}/Merge", Json(new { targetID = artist.ID }))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await api.SendJson(user.Token, HttpMethod.Post, $"/Artist/{artist.ID}/Merge", "{}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.SendJson(user.Token, HttpMethod.Post, $"/Artist/{artist.ID}/Merge", Json(new { targetID = foreignArtist.ID }))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await api.SendJson(user.Token, HttpMethod.Post, $"/Artist/{artist.ID}/Split", Json(new { nameList = new[] { "Only" } }))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await api.SendJson(user.Token, HttpMethod.Post, $"/Artist/{artist.ID}/Split", Json(new { nameList = new[] { "A", "a" } }))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.SendJson(user.Token, HttpMethod.Post, $"/Artist/{foreignArtist.ID}/Split", Json(new { nameList = new[] { "X", "Y" } }))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.SendJson(null, HttpMethod.Post, $"/Artist/{artist.ID}/Merge", Json(new { targetID = foreignArtist.ID }))).StatusCode);

        Assert.Empty(Assert.Single(await api.GetMediaFileList(user.Token)).ArtistIDList);
        Assert.Single(await Send<List<ResponseArtist>>(api, user, HttpMethod.Get, "/Artist"));
    }
}
