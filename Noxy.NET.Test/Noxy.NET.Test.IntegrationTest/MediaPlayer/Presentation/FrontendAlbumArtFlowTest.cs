using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.Test.MediaPlayer.Fixtures;
using Noxy.NET.Test.MediaPlayer.Fixtures.Extensions;
using SkiaSharp;
using TagLib;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FrontendAlbumArtFlowTest
{
    private const string Password = "a-long-enough-password";
    private const string DataUriPrefix = "data:image/jpeg;base64,";

    private static byte[] Track(string title)
    {
        using SKBitmap bitmap = new(200, 200);
        bitmap.Erase(SKColors.Teal);
        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        byte[] picture = data.ToArray();

        return MediaFileFixture.CreateTaggedMp3(tag =>
        {
            tag.Title = title;
            tag.Album = "Pictured";
            tag.AlbumArtists = ["Main"];
            tag.Pictures = [new Picture(new ByteVector(picture)) { Type = PictureType.FrontCover, MimeType = "image/png" }];
        });
    }

    // The upload is processed in the background and the picture is stored right after it, so the server's album list is read until the album shows it.
    private static async Task<ResponseAlbum> UploadPicturedAlbum(ApiFactory factory, string email)
    {
        MediaApiClient api = new(factory);
        ApiAccount account = await api.SignUp(email);
        await api.Upload(account.Token, Track("One"), "one.mp3");
        await api.WaitForMediaFileList(account.Token, 1);

        ResponseAlbum? album = null;
        await FrontendClient.WaitFor(async () =>
        {
            album = (await (await api.GetPath(account.Token, "/Album")).ReadJson<List<ResponseAlbum>>()).FirstOrDefault();
            return album?.TimeArtModified is not null;
        }, "the album to get its picture");

        return album ?? throw new InvalidOperationException("The album did not appear.");
    }

    private static int ArtRequestCount(FrontendClient frontend)
    {
        return frontend.Network.RequestList.Count(x => x.Path.EndsWith("/Art", StringComparison.Ordinal));
    }

    [Fact]
    public async Task APicture_FetchedOnce_IsShownAgainAfterARestart_WithoutAnotherRequest()
    {
        await using ApiFactory factory = new();
        string email = $"{Guid.CreateVersion7()}@example.com";
        ResponseAlbum album = await UploadPicturedAlbum(factory, email);

        await using FrontendClient first = FrontendClient.Create(factory);
        await first.Session.SignIn(email, Password);
        await first.MetadataSync.SyncAlbumList();
        string? before = await first.Art.GetAlbumArtUri(album.ID);

        await using FrontendClient restarted = FrontendClient.Create(factory, first.SessionStorage, first.Folder);
        string? after = await restarted.Art.GetAlbumArtUri(album.ID);

        Assert.NotNull(before);
        Assert.StartsWith(DataUriPrefix, before);
        Assert.Equal(before, after);
        Assert.Equal(1, ArtRequestCount(first));
        Assert.Equal(0, ArtRequestCount(restarted));
    }

    [Fact]
    public async Task APicture_IsShownAfterARestart_AlsoWhenTheServerCannotBeReached()
    {
        await using ApiFactory factory = new();
        string email = $"{Guid.CreateVersion7()}@example.com";
        ResponseAlbum album = await UploadPicturedAlbum(factory, email);

        await using FrontendClient first = FrontendClient.Create(factory);
        await first.Session.SignIn(email, Password);
        await first.MetadataSync.SyncAlbumList();
        string? before = await first.Art.GetAlbumArtUri(album.ID);

        await using FrontendClient restarted = FrontendClient.Create(factory, first.SessionStorage, first.Folder);
        restarted.Network.Drop();
        string? after = await restarted.Art.GetAlbumArtUri(album.ID);

        Assert.NotNull(before);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task APicture_IsShownAfterARestart_AndTheStartUpSync()
    {
        await using ApiFactory factory = new();
        string email = $"{Guid.CreateVersion7()}@example.com";
        ResponseAlbum album = await UploadPicturedAlbum(factory, email);

        await using FrontendClient first = FrontendClient.Create(factory);
        await first.Session.SignIn(email, Password);
        await first.MetadataSync.SyncAlbumList();
        string? before = await first.Art.GetAlbumArtUri(album.ID);

        await using FrontendClient restarted = FrontendClient.Create(factory, first.SessionStorage, first.Folder);
        await restarted.Startup.Start();
        string? after = await restarted.Art.GetAlbumArtUri(album.ID);

        Assert.NotNull(before);
        Assert.Equal(before, after);
        Assert.Equal(0, ArtRequestCount(restarted));
    }
}
