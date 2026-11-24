using Noxy.NET.MediaPlayer.Application.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeThumbnailService : IThumbnailService
{
    public List<byte[]> ImageList { get; } = [];

    // What every call returns: null means the bytes are not an image.
    public byte[]? Result { get; set; } = [0xFF, 0xD8, 9, 9];

    public byte[]? CreateThumbnail(byte[] image)
    {
        ImageList.Add(image);
        return Result;
    }
}
