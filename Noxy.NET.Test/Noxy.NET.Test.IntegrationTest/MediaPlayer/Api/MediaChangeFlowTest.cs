using System.IO.Compression;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.Test.MediaPlayer.Fixtures.Extensions;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class MediaChangeFlowTest
{
    // Uploads a media file through the API and waits for the worker to put it in the library.
    private static async Task<ResponseMediaFile> UploadMediaFile(MediaApiClient api, ApiAccount user, string title, int alreadyThere = 0)
    {
        HttpResponseMessage response = await api.Upload(user.Token, MediaApiClient.Mp3(300 + alreadyThere), $"{title}.mp3", title);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        return (await api.WaitForMediaFileList(user.Token, alreadyThere + 1)).Single(x => x.Title == title);
    }

    private static async Task<string> CurrentETag(MediaApiClient api, ApiAccount user)
    {
        HttpResponseMessage response = await api.GetLibrary(user.Token);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return response.GetETag();
    }

    private static async Task<ResponseMediaFile> ReadMediaFile(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadJson<ResponseMediaFile>();
    }

    #region -- Library --

    [Fact]
    public async Task Library_Returns200WithAnETag_AndAMatchingRequestGets304WithNoBody()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await UploadMediaFile(api, user, "One");

        HttpResponseMessage first = await api.GetLibrary(user.Token);
        string eTag = first.GetETag();
        HttpResponseMessage second = await api.GetLibrary(user.Token, eTag);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Single(await first.ReadJson<List<ResponseMediaFile>>());
        Assert.True(first.Headers.CacheControl?.Private);
        Assert.True(first.Headers.CacheControl?.NoCache);
        Assert.Equal(HttpStatusCode.NotModified, second.StatusCode);
        Assert.Empty(await second.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
        Assert.Equal(eTag, second.Headers.ETag?.Tag);
    }

    [Fact]
    public async Task Library_ETagChangesAfterAnEdit_AfterAnUpload_AndAfterADelete()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, "One");
        string afterUpload = await CurrentETag(api, user);

        await api.Patch(user.Token, mediaFile.ID, """{"artist":"Someone"}""");
        string afterEdit = await CurrentETag(api, user);
        await UploadMediaFile(api, user, "Two", alreadyThere: 1);
        string afterSecondUpload = await CurrentETag(api, user);
        await api.Delete(user.Token, mediaFile.ID);
        string afterDelete = await CurrentETag(api, user);

        Assert.Equal(4, new[] { afterUpload, afterEdit, afterSecondUpload, afterDelete }.Distinct().Count());
    }

    [Fact]
    public async Task Library_NoOpPatch_DoesNotChangeTheETag()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, "One");
        string before = await CurrentETag(api, user);

        HttpResponseMessage response = await api.Patch(user.Token, mediaFile.ID, """{"title":"One"}""");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(before, await CurrentETag(api, user));
    }

    [Fact]
    public async Task Library_EmptyLibrary_ReturnsAnEmptyListAndAStableETag()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        HttpResponseMessage first = await api.GetLibrary(user.Token);
        HttpResponseMessage second = await api.GetLibrary(user.Token, first.GetETag());

        Assert.Empty(await first.ReadJson<List<ResponseMediaFile>>());
        Assert.Equal(HttpStatusCode.NotModified, second.StatusCode);
    }

    [Fact]
    public async Task Library_IsGzippedWhenAsked()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await UploadMediaFile(api, user, "One");

        HttpResponseMessage response = await api.GetLibrary(user.Token, acceptEncoding: "gzip");

        Assert.Contains("gzip", response.Content.Headers.ContentEncoding);
        await using GZipStream gzip = new(await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken), CompressionMode.Decompress);
        List<ResponseMediaFile> list = await JsonSerializer.DeserializeAsync<List<ResponseMediaFile>>(gzip, JsonSerializerOptions.Web, TestContext.Current.CancellationToken) ?? throw new InvalidOperationException("The body is not a media file list.");
        Assert.Equal("One", Assert.Single(list).Title);
    }

    [Fact]
    public async Task Library_WithoutToken_Returns401()
    {
        await using ApiFactory factory = new();

        Assert.Equal(HttpStatusCode.Unauthorized, (await new MediaApiClient(factory).GetLibrary(null)).StatusCode);
    }

    [Fact]
    public async Task Library_MalformedIfNoneMatch_StillReturnsTheList()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await UploadMediaFile(api, user, "One");

        HttpResponseMessage response = await api.GetLibrary(user.Token, "not a valid etag, ,,");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion -- Library --

    #region -- Patch --

    [Fact]
    public async Task Patch_ChangesTagsAndReturnsTheUpdatedMediaFile_AndTheListShowsThem()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, "One");

        ResponseMediaFile updated = await ReadMediaFile(await api.Patch(user.Token, mediaFile.ID, """{"title":"Renamed","artist":"Someone","album":"Record","trackNumber":3,"year":1999}"""));

        Assert.Equal(["Renamed", "Someone", "Record"], [updated.Title, updated.Artist, updated.Album]);
        Assert.Equal([3, 1999], [updated.TrackNumber, updated.Year]);
        Assert.True(updated.TimeModified > mediaFile.TimeModified);
        ResponseMediaFile listed = Assert.Single(await api.GetMediaFileList(user.Token));
        Assert.Equal("Someone", listed.Artist);
        Assert.Equal("Renamed", listed.Title);
    }

    [Fact]
    public async Task Patch_OnlySentFieldsChange_NullClears()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, "One");
        await api.Patch(user.Token, mediaFile.ID, """{"artist":"Someone","album":"Record"}""");

        ResponseMediaFile updated = await ReadMediaFile(await api.Patch(user.Token, mediaFile.ID, """{"artist":null}"""));

        Assert.Null(updated.Artist);
        Assert.Equal("Record", updated.Album);
        Assert.Equal("One", updated.Title);
    }

    [Theory]
    [InlineData("""{"title":""}""")]
    [InlineData("""{"title":"   "}""")]
    [InlineData("""{"title":null}""")]
    [InlineData("""{"year":10000}""")]
    [InlineData("""{"trackNumber":-1}""")]
    [InlineData("""{"discNumber":10000}""")]
    public async Task Patch_InvalidValues_Return400(string json)
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, "One");

        HttpResponseMessage response = await api.Patch(user.Token, mediaFile.ID, json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("One", Assert.Single(await api.GetMediaFileList(user.Token)).Title);
    }

    [Theory]
    [InlineData("5")]
    [InlineData("1999")]
    [InlineData("0")]
    [InlineData("3.14")]
    [InlineData("true")]
    [InlineData("null")]
    public async Task Patch_TextThatLooksLikeAnotherJsonType_IsStoredAsText(string text)
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, "One");

        ResponseMediaFile updated = await ReadMediaFile(await api.Patch(user.Token, mediaFile.ID, JsonSerializer.Serialize(new { title = text, artist = text, album = text })));

        Assert.Equal([text, text, text], [updated.Title, updated.Artist, updated.Album]);
        Assert.Equal(text, Assert.Single(await api.GetMediaFileList(user.Token)).Title);
    }

    [Fact]
    public async Task Patch_OverLongText_Returns400()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, "One");

        HttpResponseMessage response = await api.Patch(user.Token, mediaFile.ID, $$"""{"genre":"{{new string('a', 129)}}"}""");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("""{"title":5}""")]
    [InlineData("""{"year":"abc"}""")]
    [InlineData("[]")]
    [InlineData("\"x\"")]
    [InlineData("{bad json")]
    [InlineData("null")]
    [InlineData("")]
    public async Task Patch_WrongTypesOrNotAnObject_Return400(string json)
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, "One");

        HttpResponseMessage response = await api.Patch(user.Token, mediaFile.ID, json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Patch_AnotherUsersMediaFile_Returns404_AndChangesNothing()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount owner = await api.SignUp("owner@example.com");
        ApiAccount other = await api.SignUp("other@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, owner, "One");

        HttpResponseMessage response = await api.Patch(other.Token, mediaFile.ID, """{"title":"Hijacked"}""");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("One", Assert.Single(await api.GetMediaFileList(owner.Token)).Title);
    }

    [Fact]
    public async Task Patch_UnknownId_Returns404()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        Assert.Equal(HttpStatusCode.NotFound, (await api.Patch(user.Token, Guid.CreateVersion7(), """{"title":"x"}""")).StatusCode);
    }

    [Fact]
    public async Task Patch_WithoutToken_Returns401()
    {
        await using ApiFactory factory = new();

        Assert.Equal(HttpStatusCode.Unauthorized, (await new MediaApiClient(factory).Patch(null, Guid.CreateVersion7(), "{}")).StatusCode);
    }

    #endregion -- Patch --

    #region -- Delete --

    [Fact]
    public async Task Delete_Returns204_TheMediaFileIsGone_AndTheFileIsRemoved()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, "One");
        Assert.Single(Directory.GetFiles(factory.StorageRoot));

        HttpResponseMessage response = await api.Delete(user.Token, mediaFile.ID);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await api.GetMediaFileList(user.Token));
        Assert.Equal(HttpStatusCode.NotFound, (await api.Download(user.Token, mediaFile.ID)).StatusCode);
        Assert.Empty(Directory.GetFiles(factory.StorageRoot));
    }

    [Fact]
    public async Task Delete_AnotherUsersMediaFile_Returns404_AndKeepsTheMediaFile()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount owner = await api.SignUp("owner@example.com");
        ApiAccount other = await api.SignUp("other@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, owner, "One");

        HttpResponseMessage response = await api.Delete(other.Token, mediaFile.ID);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Single(await api.GetMediaFileList(owner.Token));
        Assert.Single(Directory.GetFiles(factory.StorageRoot));
    }

    [Fact]
    public async Task Delete_FileAlreadyMissing_Returns204()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, "One");
        File.Delete(Directory.GetFiles(factory.StorageRoot).Single());

        HttpResponseMessage response = await api.Delete(user.Token, mediaFile.ID);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await api.GetMediaFileList(user.Token));
    }

    [Fact]
    public async Task Delete_UnknownId_Returns404_AndWithoutToken401()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        Assert.Equal(HttpStatusCode.NotFound, (await api.Delete(user.Token, Guid.CreateVersion7())).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.Delete(null, Guid.CreateVersion7())).StatusCode);
    }

    #endregion -- Delete --

    #region -- Push --

    [Fact]
    public async Task Push_Patch_SendsMediaUpdatedToEveryConnectionOfTheUser_AndNotToAnotherUser()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("other@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, "One");
        await using LibraryHubListener phone = await LibraryHubListener.Connect(factory, user.Token);
        await using LibraryHubListener computer = await LibraryHubListener.Connect(factory, user.Token, tokenInQueryString: true);
        await using LibraryHubListener stranger = await LibraryHubListener.Connect(factory, other.Token);

        ResponseMediaFile updated = await ReadMediaFile(await api.Patch(user.Token, mediaFile.ID, """{"title":"Renamed","year":2001}"""));

        ResponseMediaFile onPhone = await phone.NextMediaUpdated();
        ResponseMediaFile onComputer = await computer.NextMediaUpdated();
        Assert.Equal([mediaFile.ID, mediaFile.ID], [onPhone.ID, onComputer.ID]);
        Assert.Equal(["Renamed", "Renamed"], [onPhone.Title, onComputer.Title]);
        Assert.Equal(2001, onPhone.Year);
        Assert.Equal(updated.TimeModified, onPhone.TimeModified);
        Assert.True(await stranger.ReceivesNothingWithin(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public async Task Push_AMediaUpdate_ReachesAConnectionExactlyOnce()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, "One");
        await using LibraryHubListener phone = await LibraryHubListener.Connect(factory, user.Token);

        await api.Patch(user.Token, mediaFile.ID, """{"title":"Renamed"}""");

        await phone.NextMediaUpdated();
        Assert.Equal(1, await phone.CountReceivedAfter(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public async Task Push_NoOpPatch_PushesNothing()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, "One");
        await using LibraryHubListener hub = await LibraryHubListener.Connect(factory, user.Token);

        await api.Patch(user.Token, mediaFile.ID, """{"title":"One"}""");

        Assert.True(await hub.ReceivesNothingWithin(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public async Task Push_Delete_SendsMediaDeletedWithTheId_ToEveryConnectionOfTheUser()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("other@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, "One");
        await using LibraryHubListener phone = await LibraryHubListener.Connect(factory, user.Token);
        await using LibraryHubListener computer = await LibraryHubListener.Connect(factory, user.Token);
        await using LibraryHubListener stranger = await LibraryHubListener.Connect(factory, other.Token);

        await api.Delete(user.Token, mediaFile.ID);

        Assert.Equal(mediaFile.ID, await phone.NextMediaDeleted());
        Assert.Equal(mediaFile.ID, await computer.NextMediaDeleted());
        Assert.True(await stranger.ReceivesNothingWithin(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public async Task Push_Upload_StillSendsUploadSucceeded()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using LibraryHubListener hub = await LibraryHubListener.Connect(factory, user.Token);

        await api.Upload(user.Token, MediaApiClient.Mp3(), "media.mp3", "One");

        (string name, ResponseUploadResult result) = await hub.Next();
        Assert.Equal(nameof(MediaHubEvent.UploadSucceeded), name);
        Assert.Equal("One", result.Media?.Title);
    }

    [Fact]
    public async Task Push_AnUpload_ReachesAConnectionExactlyOnce()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using LibraryHubListener hub = await LibraryHubListener.Connect(factory, user.Token);

        await api.Upload(user.Token, MediaApiClient.Mp3(), "media.mp3", "One");

        await hub.Next();
        Assert.Equal(1, await hub.CountReceivedAfter(TimeSpan.FromSeconds(1)));
    }

    #endregion -- Push --
}
