using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Noxy.NET.MediaPlayer.API;
using Noxy.NET.MediaPlayer.Application;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces.Models.Upload;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Persistence;
using Noxy.NET.Test.MediaPlayer.Fixtures;
using Noxy.NET.Test.MediaPlayer.Fixtures.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class MediaUploadFlowTest
{
    private static async Task<ResponseUploadAccepted> ReadAccepted(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        return await response.ReadJson<ResponseUploadAccepted>();
    }

    private static async Task<List<ResponseMediaFile>> WaitForLibrary(MediaApiClient api, string token, int count)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(15);
        while (true)
        {
            List<ResponseMediaFile> list = await api.GetMediaFileList(token);
            if (list.Count >= count) return list;

            Assert.True(DateTime.UtcNow < deadline, "The library did not fill up in time.");
            await Task.Delay(25, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Upload_Returns202_ThenTheHubReportsSuccess_AndTheLibraryListsTheFile()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using LibraryHubListener hub = await LibraryHubListener.Connect(factory, user.Token);
        byte[] content = MediaApiClient.Mp3(1500);

        ResponseUploadAccepted accepted = await ReadAccepted(await api.Upload(user.Token, content, "media.test.mp3", "My Media"));
        (string name, ResponseUploadResult result) = await hub.Next();

        Assert.Equal(nameof(MediaHubEventEnum.UploadSucceeded), name);
        Assert.Equal(accepted.JobID, result.JobID);
        Assert.True(result.Succeeded);
        Assert.Equal("My Media", result.Title);
        Assert.Equal("audio/mpeg", result.Media?.ContentType);
        Assert.Equal(1500, result.Media?.SizeBytes);
        ResponseMediaFile listed = Assert.Single(await api.GetMediaFileList(user.Token));
        Assert.Equal(result.Media?.ID, listed.ID);
        Assert.Empty(Directory.GetFiles(factory.StagingFolder));
    }

    [Fact]
    public async Task Upload_ThenDownload_ReturnsTheSameBytesAndHash()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using LibraryHubListener hub = await LibraryHubListener.Connect(factory, user.Token);
        byte[] content = MediaApiClient.Mp3(4321);

        await api.Upload(user.Token, content, "media.mp3", "My Media");
        (_, ResponseUploadResult result) = await hub.Next();
        Assert.NotNull(result.Media);
        HttpResponseMessage download = await api.Download(user.Token, result.Media.ID);

        Assert.Equal(content, await download.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(content)), result.Media.ContentHash);
        Assert.Equal("My Media.mp3", download.Content.Headers.ContentDisposition?.FileNameStar ?? download.Content.Headers.ContentDisposition?.FileName);
    }

    [Fact]
    public async Task Upload_NoHubConnected_StillCompletesAndTheFileAppearsInTheList()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        await ReadAccepted(await api.Upload(user.Token, MediaApiClient.Mp3(), "media.test.mp3"));
        List<ResponseMediaFile> list = await WaitForLibrary(api, user.Token, 1);

        Assert.Equal("media.test", Assert.Single(list).Title);
    }

    private static byte[] TaggedTrack(int track, string title)
    {
        return MediaFileFixture.CreateTaggedMp3(tag =>
        {
            tag.Title = title;
            tag.Performers = ["Main feat. Guest"];
            tag.Album = "OK Computer";
            tag.AlbumArtists = ["Main"];
            tag.Genres = ["Rock"];
            tag.Track = (uint)track;
            tag.Year = 1997;
        });
    }

    [Fact]
    public async Task Upload_TwoTaggedTracksOfOneAlbum_MakeOneAlbumAndOneSetOfLinkedArtists()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        await ReadAccepted(await api.Upload(user.Token, TaggedTrack(1, "First"), "one.mp3"));
        await WaitForLibrary(api, user.Token, 1);
        await ReadAccepted(await api.Upload(user.Token, TaggedTrack(2, "Second"), "two.mp3"));
        List<ResponseMediaFile> list = await WaitForLibrary(api, user.Token, 2);

        Assert.Equal(["First", "Second"], list.Select(x => x.Title).Order().ToList());
        Assert.All(list, x =>
        {
            Assert.Equal("Main; Guest", x.Artist);
            Assert.Equal("OK Computer", x.Album);
            Assert.Equal("Main", x.AlbumArtist);
            Assert.Equal("Rock", x.Genre);
            Assert.Equal(1997, x.Year);
            Assert.NotNull(x.Duration);
            Assert.Equal(2, x.ArtistIDList.Count);
            Assert.Single(x.AlbumArtistIDList);
            Assert.Single(x.GenreIDList);
        });
        Assert.NotNull(list[0].AlbumID);
        Assert.Equal(list[0].AlbumID, list[1].AlbumID);
        Assert.Equal(list[0].ArtistIDList, list[1].ArtistIDList);
        Assert.Equal(list[0].GenreIDList, list[1].GenreIDList);

        await using DataContext context = await factory.Services.GetRequiredService<IDbContextFactory<DataContext>>().CreateDbContextAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, await context.Artist.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await context.Album.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await context.Genre.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Upload_ATaggedFileWithATitleSent_KeepsTheSentTitle()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        await ReadAccepted(await api.Upload(user.Token, TaggedTrack(1, "Embedded"), "one.mp3", "Sent"));
        List<ResponseMediaFile> list = await WaitForLibrary(api, user.Token, 1);

        Assert.Equal("Sent", Assert.Single(list).Title);
    }

    [Fact]
    public async Task Upload_AFileWithUnreadableTags_StillBecomesALibraryEntryTitledFromItsFileName()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        await ReadAccepted(await api.Upload(user.Token, MediaApiClient.Mp3(1500), "my song.mp3"));
        ResponseMediaFile file = Assert.Single(await WaitForLibrary(api, user.Token, 1));

        Assert.Equal("my song", file.Title);
        Assert.Null(file.Artist);
        Assert.Null(file.AlbumID);
        Assert.Empty(file.ArtistIDList);
    }

    [Fact]
    public async Task Upload_WithoutAFileField_Returns400_NamingTheField_AndNothingIsStaged()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        HttpResponseMessage response = await api.UploadWithoutFile(user.Token, "Only a title");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        JsonElement problem = (await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken));
        Assert.Equal($$"""Expected a "{{ApplicationConstants.FileField}}" field.""", problem.GetProperty("title").GetString());
        Assert.Empty(Directory.GetFiles(factory.StagingFolder));
    }

    [Fact]
    public async Task Upload_BadHeader_Returns415_AndNothingIsStaged()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        HttpResponseMessage response = await api.Upload(user.Token, new byte[500]);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        Assert.Empty(Directory.GetFiles(factory.StagingFolder));
    }

    [Fact]
    public async Task Upload_EmptyFile_Returns415_AndNothingIsStaged()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        HttpResponseMessage response = await api.Upload(user.Token, []);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        Assert.Empty(Directory.GetFiles(factory.StagingFolder));
    }

    [Fact]
    public async Task Upload_OverTheSizeLimit_Returns413_AndNothingIsStaged()
    {
        await using ApiFactory factory = new(new() { [ConfigurationPath.Combine(ConfigStorage.SectionName, nameof(ConfigStorage.MaxSizeBytes))] = "1000" });
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        HttpResponseMessage response = await api.Upload(user.Token, MediaApiClient.Mp3(2000));

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Empty(Directory.GetFiles(factory.StagingFolder));
    }

    [Fact]
    public async Task Upload_WithoutToken_Returns401()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);

        HttpResponseMessage response = await api.Upload(null, MediaApiClient.Mp3());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Hub_AcceptsTheTokenFromTheQueryString_LikeABrowserClient()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using LibraryHubListener hub = await LibraryHubListener.Connect(factory, user.Token, tokenInQueryString: true);

        await api.Upload(user.Token, MediaApiClient.Mp3());

        Assert.Equal(nameof(MediaHubEventEnum.UploadSucceeded), (await hub.Next()).Event);
    }

    [Fact]
    public async Task QueryStringToken_IsNotAcceptedOutsideTheHubPaths()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        HttpResponseMessage response = await factory.CreateClient().GetAsync($"/Media?{ApiConstants.AccessTokenQueryKey}={user.Token}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Hub_WithoutToken_IsRejected()
    {
        await using ApiFactory factory = new();

        HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(() => LibraryHubListener.Connect(factory, null));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
    }

    [Fact]
    public async Task Hub_WithAnInvalidToken_IsRejected()
    {
        await using ApiFactory factory = new();

        HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(() => LibraryHubListener.Connect(factory, "not-a-token"));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
    }

    [Fact]
    public async Task Upload_AtTheFileLimit_Returns409()
    {
        await using ApiFactory factory = new(new() { [ConfigurationPath.Combine(ConfigStorage.SectionName, nameof(ConfigStorage.MaxFilesPerUser))] = "1" });
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await new ApiSeeder(factory).SeedMediaFile(user.UserID);

        HttpResponseMessage response = await api.Upload(user.Token, MediaApiClient.Mp3());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Empty(Directory.GetFiles(factory.StagingFolder));
    }

    [Fact]
    public async Task Upload_AtThePendingCap_Returns429_AndNothingIsStaged()
    {
        await using ApiFactory factory = new(new() { [ConfigurationPath.Combine(ConfigUpload.SectionName, nameof(ConfigUpload.MaxPendingPerUser))] = "1" });
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        // Marked as in progress, so the worker leaves it alone and it keeps the queue full.
        await new ApiSeeder(factory).SeedJob(user.UserID, MediaUploadJobStatusEnum.Processing, withStagedFile: false);

        HttpResponseMessage response = await api.Upload(user.Token, MediaApiClient.Mp3());

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.Empty(Directory.GetFiles(factory.StagingFolder));
    }

    [Fact]
    public async Task Worker_LibraryFullAtProcessingTime_PushesUploadFailed_AndConsumesTheJob()
    {
        // Two uploads that both passed the request check before either finished: only one fits.
        await using ApiFactory factory = new(new() { [ConfigurationPath.Combine(ConfigStorage.SectionName, nameof(ConfigStorage.MaxFilesPerUser))] = "1" });
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiSeeder seeder = new(factory);
        await seeder.SeedMediaFile(user.UserID);
        EntityMediaUploadJob job = await seeder.SeedJob(user.UserID, MediaUploadJobStatusEnum.Pending, withStagedFile: true);
        await using LibraryHubListener hub = await LibraryHubListener.Connect(factory, user.Token);

        seeder.WakeTheWorker();
        (string name, ResponseUploadResult result) = await hub.Next();

        Assert.Equal(nameof(MediaHubEventEnum.UploadFailed), name);
        Assert.Equal(job.ID, result.JobID);
        Assert.False(result.Succeeded);
        Assert.Equal(UploadFailureReasonEnum.LibraryFull, result.Reason);
        Assert.Single(await api.GetMediaFileList(user.Token));
        Assert.Empty(Directory.GetFiles(factory.StagingFolder));
    }

    [Fact]
    public async Task Upload_OtherUsersHubConnection_DoesNotReceiveTheResult()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount owner = await api.SignUp("owner@example.com");
        ApiAccount other = await api.SignUp("other@example.com");
        await using LibraryHubListener ownerHub = await LibraryHubListener.Connect(factory, owner.Token);
        await using LibraryHubListener otherHub = await LibraryHubListener.Connect(factory, other.Token);

        await api.Upload(owner.Token, MediaApiClient.Mp3());
        (string name, _) = await ownerHub.Next();

        Assert.Equal(nameof(MediaHubEventEnum.UploadSucceeded), name);
        Assert.True(await otherHub.ReceivesNothingWithin(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public async Task UploadGate_IsOneInstanceSharedByEveryRequest()
    {
        await using ApiFactory factory = new();
        using IServiceScope firstRequest = factory.Services.CreateScope();
        using IServiceScope secondRequest = factory.Services.CreateScope();

        Assert.Same(firstRequest.ServiceProvider.GetRequiredService<IUploadGate>(), secondRequest.ServiceProvider.GetRequiredService<IUploadGate>());
    }

    [Fact]
    public async Task Upload_SeveralFilesInARow_AllFinishInOrderForTheOwner()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await using LibraryHubListener hub = await LibraryHubListener.Connect(factory, user.Token);

        List<Guid> jobIDList = [];
        for (int i = 0; i < 5; i++)
        {
            jobIDList.Add((await ReadAccepted(await api.Upload(user.Token, MediaApiClient.Mp3(500 + i), $"media{i}.mp3"))).JobID);
        }

        List<Guid> finished = [];
        for (int i = 0; i < 5; i++)
        {
            finished.Add((await hub.Next()).Result.JobID);
        }

        Assert.Equal(jobIDList, finished);
        Assert.Equal(5, (await api.GetMediaFileList(user.Token)).Count);
        Assert.Empty(Directory.GetFiles(factory.StagingFolder));
    }
}
