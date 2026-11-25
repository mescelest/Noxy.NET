using Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Album;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Art;

public class HandlerAlbumArtSetTest
{
    private readonly FakeAlbumArtService _service = new();
    private readonly HandlerAlbumArtSet _handler;

    public HandlerAlbumArtSetTest()
    {
        _handler = new(_service);
    }

    [Fact]
    public async Task Handle_ReplacesThePictureOfTheAlbum_ForTheUser()
    {
        Guid userID = Guid.CreateVersion7();
        Guid albumID = Guid.CreateVersion7();
        byte[] picture = [1, 2, 3];

        await _handler.Handle(new(userID, albumID, picture), TestContext.Current.CancellationToken);

        Assert.Equal((userID, albumID, picture), Assert.Single(_service.ReplaceList));
    }

    [Fact]
    public async Task Handle_WhenTheServiceRefuses_TheRefusalReachesTheCaller()
    {
        _service.Failure = new MetadataItemNotFoundException();

        await Assert.ThrowsAsync<MetadataItemNotFoundException>(() => _handler.Handle(new(Guid.CreateVersion7(), Guid.CreateVersion7(), [1]), TestContext.Current.CancellationToken).AsTask());
    }
}
