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

public class MetadataFlowTest
{
    private static byte[] Track(int number, string title)
    {
        return MediaFileFixture.CreateTaggedMp3(tag =>
        {
            tag.Title = title;
            tag.Performers = ["Main feat. Guest"];
            tag.Album = "OK Computer";
            tag.AlbumArtists = ["Main"];
            tag.Genres = ["Rock"];
            tag.Track = (uint)number;
        });
    }

    private static async Task<List<ResponseMediaFile>> UploadAlbum(MediaApiClient api, ApiAccount user)
    {
        for (int number = 1; number <= 3; number++)
        {
            HttpResponseMessage response = await api.Upload(user.Token, Track(number, $"Track {number}"), $"track{number}.mp3");
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            await api.WaitForMediaFileList(user.Token, number);
        }

        return await api.GetMediaFileList(user.Token);
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

    private static string[] ReadPerformerList(byte[] bytes)
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.CreateVersion7()}.mp3");
        try
        {
            File.WriteAllBytes(path, bytes);
            using TagLib.File file = TagLib.File.Create(path);
            return file.Tag.Performers;
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ThreeTaggedTracksOfOneAlbum_MakeOneAlbumTheRightArtistsAndNoDuplicate()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        List<ResponseMediaFile> fileList = await UploadAlbum(api, user);

        List<ResponseArtist> artistList = await Send<List<ResponseArtist>>(api, user, HttpMethod.Get, "/Artist");
        ResponseAlbum album = Assert.Single(await Send<List<ResponseAlbum>>(api, user, HttpMethod.Get, "/Album"));
        ResponseGenre genre = Assert.Single(await Send<List<ResponseGenre>>(api, user, HttpMethod.Get, "/Genre"));
        Assert.Equal(["Guest", "Main"], artistList.Select(x => x.Name).Order().ToList());
        Assert.All(artistList, x => Assert.Equal(3, x.MediaFileCount));
        Assert.Equal(3, album.MediaFileCount);
        Assert.Equal([artistList.Single(x => x.Name == "Main").ID], album.ArtistIDList);
        Assert.Equal(3, genre.MediaFileCount);
        Assert.All(fileList, x => Assert.Equal(album.ID, x.AlbumID));
        Assert.All(fileList, x => Assert.Equal("Main; Guest", x.Artist));
    }

    [Fact]
    public async Task RenamingAnArtist_ChangesTheLibraryETag_AndPushesTheArtistAndEveryChangedFile()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<ResponseMediaFile> fileList = await UploadAlbum(api, user);
        ResponseArtist main = (await Send<List<ResponseArtist>>(api, user, HttpMethod.Get, "/Artist")).Single(x => x.Name == "Main");
        string libraryETag = (await api.GetLibrary(user.Token)).GetETag();
        string artistETag = (await api.GetPath(user.Token, "/Artist")).GetETag();
        await using LibraryHubListener hub = await LibraryHubListener.Connect(factory, user.Token);

        await Send<ResponseArtist>(api, user, HttpMethod.Patch, $"/Artist/{main.ID}", """{"name":"Renamed"}""");

        ResponseArtist pushedArtist = await hub.NextArtistUpdated();
        List<ResponseMediaFile> pushedFileList = [await hub.NextMediaUpdated(), await hub.NextMediaUpdated(), await hub.NextMediaUpdated()];
        Assert.Equal("Renamed", pushedArtist.Name);
        Assert.Equal(fileList.Select(x => x.ID).Order().ToList(), pushedFileList.Select(x => x.ID).Order().ToList());
        Assert.All(pushedFileList, x => Assert.Equal("Renamed; Guest", x.Artist));
        Assert.All(pushedFileList, x => Assert.Equal("Renamed", x.AlbumArtist));
        Assert.NotEqual(libraryETag, (await api.GetLibrary(user.Token)).GetETag());
        Assert.NotEqual(artistETag, (await api.GetPath(user.Token, "/Artist")).GetETag());
    }

    [Fact]
    public async Task ACollidingRename_Is409WithTheSettingOff_AndMergesWithItOn_AndTheSettingIsPushed()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await UploadAlbum(api, user);
        List<ResponseArtist> artistList = await Send<List<ResponseArtist>>(api, user, HttpMethod.Get, "/Artist");
        ResponseArtist main = artistList.Single(x => x.Name == "Main");
        ResponseArtist guest = artistList.Single(x => x.Name == "Guest");
        await using LibraryHubListener hub = await LibraryHubListener.Connect(factory, user.Token);

        HttpResponseMessage conflict = await api.SendJson(user.Token, HttpMethod.Patch, $"/Artist/{guest.ID}", """{"name":"main"}""");
        await Send<ResponseSetting>(api, user, HttpMethod.Patch, $"/Settings/{SettingKey.MergeOnRename}", """{"value":true}""");
        ResponseSetting pushedSetting = await hub.NextSettingUpdated();
        ResponseArtist merged = await Send<ResponseArtist>(api, user, HttpMethod.Patch, $"/Artist/{guest.ID}", """{"name":"main"}""");
        Guid deleted = await hub.NextArtistDeleted();

        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.Equal(main.ID, (await conflict.ReadJson<JsonElement>()).GetProperty(ApiExceptionHandler.ExistingIDKey).GetGuid());
        Assert.Equal(SettingKey.MergeOnRename, pushedSetting.Key);
        Assert.True(JsonSerializer.SerializeToElement(pushedSetting.Value).GetBoolean());
        Assert.Equal(main.ID, merged.ID);
        Assert.Equal("Main", merged.Name);
        Assert.Equal(guest.ID, deleted);
        Assert.Equal(main.ID, Assert.Single(await Send<List<ResponseArtist>>(api, user, HttpMethod.Get, "/Artist")).ID);
        Assert.All(await api.GetMediaFileList(user.Token), x => Assert.Equal([main.ID], x.ArtistIDList));
    }

    [Fact]
    public async Task SplittingACombinedArtist_ReplacesItEverywhere()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await UploadAlbum(api, user);
        ResponseMediaFile first = (await api.GetMediaFileList(user.Token))[0];
        ResponseMediaFile combinedFile = await Send<ResponseMediaFile>(api, user, HttpMethod.Patch, $"/Media/{first.ID}", """{"artist":"A & B"}""");

        List<ResponseArtist> split = await Send<List<ResponseArtist>>(api, user, HttpMethod.Post, $"/Artist/{combinedFile.ArtistIDList[0]}/Split", Json(new { nameList = new[] { "A", "B" } }));

        Assert.Equal(["A", "B"], split.Select(x => x.Name).ToList());
        Assert.Equal("A; B", (await api.GetMediaFileList(user.Token)).Single(x => x.ID == first.ID).Artist);
        Assert.DoesNotContain(await Send<List<ResponseArtist>>(api, user, HttpMethod.Get, "/Artist"), x => x.Name == "A & B");
    }

    [Fact]
    public async Task DeletingAGenre_KeepsTheFilesWithoutIt_AndPushesTheDeletion()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await UploadAlbum(api, user);
        ResponseGenre genre = Assert.Single(await Send<List<ResponseGenre>>(api, user, HttpMethod.Get, "/Genre"));
        await using LibraryHubListener hub = await LibraryHubListener.Connect(factory, user.Token);

        HttpResponseMessage deleted = await api.SendJson(user.Token, HttpMethod.Delete, $"/Genre/{genre.ID}");

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(genre.ID, await hub.NextGenreDeleted());
        List<ResponseMediaFile> fileList = await api.GetMediaFileList(user.Token);
        Assert.Equal(3, fileList.Count);
        Assert.All(fileList, x => Assert.Null(x.Genre));
        Assert.All(fileList, x => Assert.Empty(x.GenreIDList));
        Assert.Equal("[]", await (await api.GetPath(user.Token, "/Genre")).Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AnotherUser_SeesNoneOfIt_AndCannotChangeIt()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("b@example.com");
        await UploadAlbum(api, user);
        ResponseArtist main = (await Send<List<ResponseArtist>>(api, user, HttpMethod.Get, "/Artist")).Single(x => x.Name == "Main");
        ResponseAlbum album = Assert.Single(await Send<List<ResponseAlbum>>(api, user, HttpMethod.Get, "/Album"));
        ResponseGenre genre = Assert.Single(await Send<List<ResponseGenre>>(api, user, HttpMethod.Get, "/Genre"));

        Assert.Equal("[]", await (await api.GetPath(other.Token, "/Artist")).Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal("[]", await (await api.GetPath(other.Token, "/Album")).Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal("[]", await (await api.GetPath(other.Token, "/Genre")).Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(HttpStatusCode.NotFound, (await api.SendJson(other.Token, HttpMethod.Patch, $"/Artist/{main.ID}", """{"name":"Mine"}""")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.SendJson(other.Token, HttpMethod.Delete, $"/Album/{album.ID}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.SendJson(other.Token, HttpMethod.Post, $"/Genre/{genre.ID}/Merge", Json(new { targetID = Guid.CreateVersion7() }))).StatusCode);
        Assert.Equal(2, (await Send<List<ResponseArtist>>(api, user, HttpMethod.Get, "/Artist")).Count);
        Assert.Equal(3, Assert.Single(await Send<List<ResponseAlbum>>(api, user, HttpMethod.Get, "/Album")).MediaFileCount);
    }

    [Fact]
    public async Task ATaggedDownload_CarriesTheItemNames_AndTheNewNameAfterARename()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        List<ResponseMediaFile> fileList = await UploadAlbum(api, user);
        ResponseMediaFile file = fileList[0];
        ResponseArtist main = (await Send<List<ResponseArtist>>(api, user, HttpMethod.Get, "/Artist")).Single(x => x.Name == "Main");

        byte[] before = await (await api.DownloadTagged(user.Token, file.ID)).Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
        await Send<ResponseArtist>(api, user, HttpMethod.Patch, $"/Artist/{main.ID}", """{"name":"Renamed"}""");
        byte[] after = await (await api.DownloadTagged(user.Token, file.ID)).Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["Main", "Guest"], ReadPerformerList(before));
        Assert.Equal(["Renamed", "Guest"], ReadPerformerList(after));
    }
}
