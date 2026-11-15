using System.Net;
using Microsoft.Extensions.Configuration;
using Noxy.NET.MediaPlayer.API.Configurations;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.Test.MediaPlayer.Fixtures.Extensions;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class MediaDownloadFlowTest
{
    private static async Task<ResponseMediaFile> UploadMediaFile(MediaApiClient api, ApiAccount user, byte[] content, string title = "My Media")
    {
        HttpResponseMessage response = await api.Upload(user.Token, content, "media.mp3", title);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        return (await api.WaitForMediaFileList(user.Token, 1))[0];
    }

    [Fact]
    public async Task File_SendsTheContentHashAsAStrongETag()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, MediaApiClient.Mp3(500));

        HttpResponseMessage response = await api.Download(user.Token, mediaFile.ID);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"\"{mediaFile.ContentHash}\"", response.GetETag());
        Assert.NotNull(response.Headers.ETag);
        Assert.False(response.Headers.ETag.IsWeak);
        Assert.Contains("bytes", response.Headers.AcceptRanges);
    }

    [Fact]
    public async Task File_WithMatchingIfNoneMatch_Returns304()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, MediaApiClient.Mp3(500));

        HttpResponseMessage response = await api.Download(user.Token, mediaFile.ID, ifNoneMatch: $"\"{mediaFile.ContentHash}\"");

        Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
    }

    [Fact]
    public async Task File_RangeWithMatchingIfRange_Returns206WithThatRange()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        byte[] content = MediaApiClient.Mp3(500);
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, content);

        HttpResponseMessage response = await api.Download(user.Token, mediaFile.ID, ifRangeETag: $"\"{mediaFile.ContentHash}\"", rangeFrom: 100, rangeTo: 199);

        Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
        Assert.Equal(content[100..200], await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task File_RangeWithStaleIfRange_Returns200WithTheWholeFile()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        byte[] content = MediaApiClient.Mp3(500);
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, content);

        HttpResponseMessage response = await api.Download(user.Token, mediaFile.ID, ifRangeETag: $"\"{new string('0', 64)}\"", rangeFrom: 100, rangeTo: 199);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(content, await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task File_OfAnotherUser_Returns404()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount owner = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("b@example.com");
        ResponseMediaFile mediaFile = await UploadMediaFile(api, owner, MediaApiClient.Mp3(500));

        HttpResponseMessage response = await api.Download(other.Token, mediaFile.ID);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<ResponseMediaFile> UploadWav(MediaApiClient api, ApiAccount user, string title = "My Media")
    {
        HttpResponseMessage response = await api.Upload(user.Token, MediaFileFixture.CreateWav(), "media.wav", title);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        return (await api.WaitForMediaFileList(user.Token, 1))[0];
    }

    private static async Task<(string? Title, string[] Performers, uint Year)> ReadTags(HttpResponseMessage response)
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.CreateVersion7()}.wav");
        await File.WriteAllBytesAsync(path, await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken), TestContext.Current.CancellationToken);

        try
        {
            using TagLib.File file = TagLib.File.Create(path);
            return (file.Tag.Title, file.Tag.Performers, file.Tag.Year);
        }
        finally
        {
            File.Delete(path);
        }
    }

    // Different from the auth window, so the Retry-After test proves the download policy is the one that answered.
    private const int LimitedWindowSeconds = 7;

    private static ApiFactory LimitedFactory(string permitLimit)
    {
        return new(new()
        {
            [ConfigurationPath.Combine(ConfigRateLimitDownload.SectionName, nameof(ConfigRateLimitDownload.PermitLimit))] = permitLimit,
            [ConfigurationPath.Combine(ConfigRateLimitDownload.SectionName, nameof(ConfigRateLimitDownload.WindowSeconds))] = LimitedWindowSeconds.ToString()
        });
    }

    [Fact]
    public async Task Download_ReturnsTheFileWithTheCurrentTags_AfterAPatch()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadWav(api, user);
        Assert.Equal(HttpStatusCode.OK, (await api.Patch(user.Token, mediaFile.ID, """{"title":"Renamed","artist":"Someone","year":1999}""")).StatusCode);

        HttpResponseMessage response = await api.DownloadTagged(user.Token, mediaFile.ID);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        (string? title, string[] performers, uint year) = await ReadTags(response);
        Assert.Equal("Renamed", title);
        Assert.Equal("Someone", Assert.Single(performers));
        Assert.Equal(1999u, year);
    }

    [Fact]
    public async Task Download_DoesNotChangeTheStoredFileOrLibraryETag()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadWav(api, user);
        byte[] before = await (await api.Download(user.Token, mediaFile.ID)).Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
        string libraryETag = (await api.GetLibrary(user.Token)).GetETag();

        Assert.Equal(HttpStatusCode.OK, (await api.DownloadTagged(user.Token, mediaFile.ID)).StatusCode);

        HttpResponseMessage file = await api.Download(user.Token, mediaFile.ID);
        Assert.Equal(before, await file.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
        Assert.Equal($"\"{mediaFile.ContentHash}\"", file.GetETag());
        Assert.Equal(libraryETag, (await api.GetLibrary(user.Token)).GetETag());
    }

    [Fact]
    public async Task Download_NamesTheFileAfterTheTitle()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadWav(api, user);

        HttpResponseMessage response = await api.DownloadTagged(user.Token, mediaFile.ID);

        Assert.Equal("My Media.wav", response.Content.Headers.ContentDisposition?.FileNameStar ?? response.Content.Headers.ContentDisposition?.FileName);
    }

    [Fact]
    public async Task Download_OfAnotherUser_Returns404()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount owner = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("b@example.com");
        ResponseMediaFile mediaFile = await UploadWav(api, owner);

        HttpResponseMessage response = await api.DownloadTagged(other.Token, mediaFile.ID);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Download_ALegitimateFileTheTaggerCannotRead_StillDownloadsTheStoredBytes()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        byte[] content = MediaApiClient.Mp3(500);
        ResponseMediaFile mediaFile = await UploadMediaFile(api, user, content);

        HttpResponseMessage response = await api.DownloadTagged(user.Token, mediaFile.ID);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(content, await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
        Assert.Empty(Directory.GetFiles(factory.TemporaryFolder));
    }

    [Fact]
    public async Task Download_IgnoresARangeRequest_AndReturnsTheWholeFileWithNoRangeHeaders()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadWav(api, user);

        HttpResponseMessage response = await api.DownloadTagged(user.Token, mediaFile.ID, rangeFrom: 10, rangeTo: 99);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(response.Content.Headers.ContentRange);
        Assert.Empty(response.Headers.AcceptRanges);
        Assert.True((await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Length > 100);
    }

    [Fact]
    public async Task Download_LeavesNoTemporaryFileBehind()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadWav(api, user);

        await (await api.DownloadTagged(user.Token, mediaFile.ID)).Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        Assert.Empty(Directory.GetFiles(factory.TemporaryFolder));
    }

    [Fact]
    public async Task Download_OverTheLimit_Returns429WithTheDownloadWindowAsRetryAfter()
    {
        await using ApiFactory factory = LimitedFactory("1");
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadWav(api, user);

        HttpResponseMessage first = await api.DownloadTagged(user.Token, mediaFile.ID);
        HttpResponseMessage second = await api.DownloadTagged(user.Token, mediaFile.ID);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, second.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(LimitedWindowSeconds), second.Headers.RetryAfter?.Delta);
    }

    [Fact]
    public async Task File_IsNotRateLimitedByTheDownloadPolicy()
    {
        await using ApiFactory factory = LimitedFactory("1");
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ResponseMediaFile mediaFile = await UploadWav(api, user);

        for (int index = 0; index < 5; index++)
        {
            Assert.Equal(HttpStatusCode.OK, (await api.Download(user.Token, mediaFile.ID)).StatusCode);
        }
    }
}
