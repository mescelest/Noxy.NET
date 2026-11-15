using System.Net;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fixtures;
using Noxy.NET.Test.MediaPlayer.Fixtures.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class MetadataListFlowTest
{
    private static byte[] Track(string title)
    {
        return MediaFileFixture.CreateTaggedMp3(tag =>
        {
            tag.Title = title;
            tag.Performers = ["Main feat. Guest"];
            tag.Album = "OK Computer";
            tag.AlbumArtists = ["Main"];
            tag.Genres = ["Rock"];
        });
    }

    private static async Task UploadTwoTracks(MediaApiClient api, ApiAccount user)
    {
        await api.Upload(user.Token, Track("One"), "one.mp3");
        await api.WaitForMediaFileList(user.Token, 1);
        await api.Upload(user.Token, Track("Two"), "two.mp3");
        await api.WaitForMediaFileList(user.Token, 2);
    }

    [Fact]
    public async Task GetArtistAlbumAndGenreLists_AfterTwoTaggedUploads_ReturnTheItemsWithTheirFileCounts()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await UploadTwoTracks(api, user);

        List<ResponseArtist> artistList = await (await api.GetPath(user.Token, "/Artist")).ReadJson<List<ResponseArtist>>();
        List<ResponseAlbum> albumList = await (await api.GetPath(user.Token, "/Album")).ReadJson<List<ResponseAlbum>>();
        List<ResponseGenre> genreList = await (await api.GetPath(user.Token, "/Genre")).ReadJson<List<ResponseGenre>>();

        Assert.Equal(["Guest", "Main"], artistList.Select(x => x.Name).Order().ToList());
        Assert.All(artistList, x => Assert.Equal(2, x.MediaFileCount));
        ResponseAlbum album = Assert.Single(albumList);
        Assert.Equal("OK Computer", album.Title);
        Assert.Equal(2, album.MediaFileCount);
        Assert.Equal([artistList.Single(x => x.Name == "Main").ID], album.ArtistIDList);
        ResponseGenre genre = Assert.Single(genreList);
        Assert.Equal("Rock", genre.Name);
        Assert.Equal(2, genre.MediaFileCount);
    }

    [Theory]
    [InlineData("/Artist")]
    [InlineData("/Album")]
    [InlineData("/Genre")]
    public async Task GetList_HasAStrongETag_AndAMatchingRequestGets304WithNoBody(string path)
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await UploadTwoTracks(api, user);

        HttpResponseMessage first = await api.GetPath(user.Token, path);
        string eTag = first.GetETag();
        using HttpRequestMessage conditional = new(HttpMethod.Get, path);
        conditional.Headers.Authorization = new("Bearer", user.Token);
        conditional.Headers.TryAddWithoutValidation("If-None-Match", eTag);
        HttpResponseMessage second = await factory.CreateClient().SendAsync(conditional, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Matches("^\"[0-9a-f]{64}\"$", eTag);
        Assert.Equal(HttpStatusCode.NotModified, second.StatusCode);
        Assert.Empty(await second.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
        Assert.Equal(eTag, second.GetETag());
    }

    [Theory]
    [InlineData("/Artist")]
    [InlineData("/Album")]
    [InlineData("/Genre")]
    public async Task GetList_ForANewUser_IsAnEmptyListAndUnauthenticatedIs401(string path)
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        HttpResponseMessage response = await api.GetPath(user.Token, path);
        HttpResponseMessage anonymous = await api.GetPath(null, path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("[]", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
    }

    [Fact]
    public async Task GetList_AnotherUserSeesNoneOfIt()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("b@example.com");
        await UploadTwoTracks(api, user);

        Assert.Equal("[]", await (await api.GetPath(other.Token, "/Artist")).Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal("[]", await (await api.GetPath(other.Token, "/Album")).Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal("[]", await (await api.GetPath(other.Token, "/Genre")).Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetArtistList_ETagChangesWhenAFileIsUploaded_EvenThoughNoArtistWasRenamed()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await api.Upload(user.Token, Track("One"), "one.mp3");
        await api.WaitForMediaFileList(user.Token, 1);
        string before = (await api.GetPath(user.Token, "/Artist")).GetETag();

        await api.Upload(user.Token, Track("Two"), "two.mp3");
        await api.WaitForMediaFileList(user.Token, 2);

        Assert.NotEqual(before, (await api.GetPath(user.Token, "/Artist")).GetETag());
    }

    [Fact]
    public async Task DeletingAMediaFile_ChangesTheItemCountsAndTheirETags()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await api.Upload(user.Token, Track("One"), "one.mp3");
        await api.WaitForMediaFileList(user.Token, 1);
        await api.Upload(user.Token, Track("Two"), "two.mp3");
        List<ResponseMediaFile> fileList = await api.WaitForMediaFileList(user.Token, 2);
        string before = (await api.GetPath(user.Token, "/Artist")).GetETag();

        HttpResponseMessage deleted = await api.Delete(user.Token, fileList[0].ID);

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        HttpResponseMessage after = await api.GetPath(user.Token, "/Artist");
        Assert.NotEqual(before, after.GetETag());
        Assert.All(await after.ReadJson<List<ResponseArtist>>(), x => Assert.Equal(1, x.MediaFileCount));
        Assert.Equal(1, Assert.Single(await (await api.GetPath(user.Token, "/Album")).ReadJson<List<ResponseAlbum>>()).MediaFileCount);
    }

    [Fact]
    public async Task MediaFileResponses_AndTheItemListsAgreeOnTheIDs()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await UploadTwoTracks(api, user);

        List<ResponseMediaFile> fileList = await api.GetMediaFileList(user.Token);
        List<ResponseArtist> artistList = await (await api.GetPath(user.Token, "/Artist")).ReadJson<List<ResponseArtist>>();

        Assert.All(fileList, file => Assert.All(file.ArtistIDList, id => Assert.Contains(artistList, artist => artist.ID == id)));
    }
}
