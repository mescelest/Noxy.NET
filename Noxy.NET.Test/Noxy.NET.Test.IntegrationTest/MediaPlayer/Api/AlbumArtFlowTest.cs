using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fixtures;
using Noxy.NET.Test.MediaPlayer.Fixtures.Extensions;
using SkiaSharp;
using TagLib;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class AlbumArtFlowTest
{
    private const string ImageContentType = "image/jpeg";

    private static byte[] Png(int width, int height)
    {
        using SKBitmap bitmap = new(width, height);
        bitmap.Erase(SKColors.Teal);
        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static byte[] Track(string title, string album, byte[]? picture)
    {
        return MediaFileFixture.CreateTaggedMp3(tag =>
        {
            tag.Title = title;
            tag.Album = album;
            tag.AlbumArtists = ["Main"];
            if (picture is not null) tag.Pictures = [new Picture(new ByteVector(picture)) { Type = PictureType.FrontCover, MimeType = "image/png" }];
        });
    }

    private static async Task<List<ResponseAlbum>> GetAlbumList(MediaApiClient api, ApiAccount user)
    {
        return await (await api.GetPath(user.Token, "/Album")).ReadJson<List<ResponseAlbum>>();
    }

    // The picture is stored right after the upload is committed, so the list is read until the album shows it.
    private static async Task<ResponseAlbum> WaitForPicture(MediaApiClient api, ApiAccount user, string title)
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            ResponseAlbum? album = (await GetAlbumList(api, user)).FirstOrDefault(x => x.Title == title);
            if (album?.TimeArtModified is not null) return album;

            await Task.Delay(100, TestContext.Current.CancellationToken);
        }

        throw new TimeoutException($"The album {title} never got a picture.");
    }

    private static async Task Upload(MediaApiClient api, ApiAccount user, byte[] content, string fileName, int expectedCount)
    {
        await api.Upload(user.Token, content, fileName);
        await api.WaitForMediaFileList(user.Token, expectedCount);
    }

    [Fact]
    public async Task GetArt_AfterAnUploadWithAPicture_Returns200WithAJpegAndAQuotedETag()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await Upload(api, user, Track("One", "Pictured", Png(800, 400)), "one.mp3", 1);
        ResponseAlbum album = await WaitForPicture(api, user, "Pictured");

        HttpResponseMessage response = await api.GetPath(user.Token, $"/Album/{album.ID}/Art");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(ImageContentType, response.Content.Headers.ContentType?.MediaType);
        Assert.Matches("^\"[0-9a-f]{64}\"$", response.GetETag());
        Assert.Contains("private", response.Headers.CacheControl?.ToString());
        byte[] bytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
        Assert.Equal([0xFF, 0xD8], bytes[..2]);
    }

    [Fact]
    public async Task GetArt_AMatchingIfNoneMatch_Gets304WithNoBody()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await Upload(api, user, Track("One", "Pictured", Png(100, 100)), "one.mp3", 1);
        ResponseAlbum album = await WaitForPicture(api, user, "Pictured");
        string eTag = (await api.GetPath(user.Token, $"/Album/{album.ID}/Art")).GetETag();

        using HttpRequestMessage conditional = new(HttpMethod.Get, $"/Album/{album.ID}/Art");
        conditional.Headers.Authorization = new("Bearer", user.Token);
        conditional.Headers.TryAddWithoutValidation("If-None-Match", eTag);
        HttpResponseMessage response = await factory.CreateClient().SendAsync(conditional, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
        Assert.Equal(eTag, response.GetETag());
    }

    [Fact]
    public async Task GetArt_ForAnAlbumWithNoPicture_AnUnknownAlbum_AndAnotherUsersAlbum_Is404()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        ApiAccount other = await api.SignUp("b@example.com");
        await Upload(api, user, Track("One", "Plain", null), "one.mp3", 1);
        await Upload(api, user, Track("Two", "Pictured", Png(100, 100)), "two.mp3", 2);
        ResponseAlbum pictured = await WaitForPicture(api, user, "Pictured");
        ResponseAlbum plain = (await GetAlbumList(api, user)).Single(x => x.Title == "Plain");

        Assert.Equal(HttpStatusCode.NotFound, (await api.GetPath(user.Token, $"/Album/{plain.ID}/Art")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.GetPath(user.Token, $"/Album/{Guid.CreateVersion7()}/Art")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.GetPath(other.Token, $"/Album/{pictured.ID}/Art")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.GetPath(null, $"/Album/{pictured.ID}/Art")).StatusCode);
    }

    [Fact]
    public async Task AnUploadWithAPicture_ForAnAlbumThatHasOne_DoesNotReplaceIt()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await Upload(api, user, Track("One", "Pictured", Png(100, 100)), "one.mp3", 1);
        ResponseAlbum before = await WaitForPicture(api, user, "Pictured");
        string eTagBefore = (await api.GetPath(user.Token, $"/Album/{before.ID}/Art")).GetETag();

        await Upload(api, user, Track("Two", "Pictured", Png(200, 200)), "two.mp3", 2);
        ResponseAlbum after = (await GetAlbumList(api, user)).Single(x => x.Title == "Pictured");

        Assert.Equal(before.TimeArtModified, after.TimeArtModified);
        Assert.Equal(eTagBefore, (await api.GetPath(user.Token, $"/Album/{before.ID}/Art")).GetETag());
    }

    [Fact]
    public async Task AnUploadWithAPicture_ForAnAlbumThatHasNone_FillsIt()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await Upload(api, user, Track("One", "Late", null), "one.mp3", 1);
        Assert.Null((await GetAlbumList(api, user)).Single().TimeArtModified);

        await Upload(api, user, Track("Two", "Late", Png(100, 100)), "two.mp3", 2);
        ResponseAlbum album = await WaitForPicture(api, user, "Late");

        Assert.Equal(HttpStatusCode.OK, (await api.GetPath(user.Token, $"/Album/{album.ID}/Art")).StatusCode);
    }

    [Fact]
    public async Task AnUploadWithACorruptPicture_StillSucceeds_AndTheAlbumHasNoPicture()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");

        await Upload(api, user, Track("One", "Broken", [1, 2, 3, 4, 5, 6, 7, 8]), "one.mp3", 1);

        Assert.Null((await GetAlbumList(api, user)).Single(x => x.Title == "Broken").TimeArtModified);
    }

    [Fact]
    public async Task DeletingAnAlbum_DeletesItsPictureFile()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await Upload(api, user, Track("One", "Pictured", Png(100, 100)), "one.mp3", 1);
        ResponseAlbum album = await WaitForPicture(api, user, "Pictured");
        IArtStorageService storage = factory.Services.GetRequiredService<IArtStorageService>();
        using (Stream? stream = storage.OpenAlbumArt(album.ID)) Assert.NotNull(stream);

        HttpResponseMessage response = await api.SendJson(user.Token, HttpMethod.Delete, $"/Album/{album.ID}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null(storage.OpenAlbumArt(album.ID));
    }

    [Fact]
    public async Task MergingAnAlbum_DeletesThePictureOfTheMergedAway_AndKeepsTheTargets()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        ApiAccount user = await api.SignUp("a@example.com");
        await Upload(api, user, Track("One", "Source", Png(100, 100)), "one.mp3", 1);
        await Upload(api, user, Track("Two", "Target", Png(100, 100)), "two.mp3", 2);
        ResponseAlbum source = await WaitForPicture(api, user, "Source");
        ResponseAlbum target = await WaitForPicture(api, user, "Target");
        IArtStorageService storage = factory.Services.GetRequiredService<IArtStorageService>();

        HttpResponseMessage response = await api.SendJson(user.Token, HttpMethod.Post, $"/Album/{source.ID}/Merge", $$"""{"targetID":"{{target.ID}}"}""");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(storage.OpenAlbumArt(source.ID));
        Assert.NotNull(storage.OpenAlbumArt(target.ID));
    }
}
