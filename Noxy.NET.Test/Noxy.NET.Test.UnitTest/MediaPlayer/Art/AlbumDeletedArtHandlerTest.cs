using Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Album;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Art;

public class AlbumDeletedArtHandlerTest
{
    [Fact]
    public async Task Handle_DeletesThePictureFileOfTheDeletedAlbum()
    {
        FakeArtStorageService storage = new();
        Guid albumID = Guid.CreateVersion7();
        Guid otherID = Guid.CreateVersion7();
        storage.AlbumArtList[albumID] = [1];
        storage.AlbumArtList[otherID] = [2];
        HandlerAlbumDeletedArt handler = new(storage);

        await handler.Handle(new NotificationAlbumDeleted(Guid.CreateVersion7(), albumID), TestContext.Current.CancellationToken);

        Assert.DoesNotContain(albumID, storage.AlbumArtList.Keys);
        Assert.Contains(otherID, storage.AlbumArtList.Keys);
    }
}
