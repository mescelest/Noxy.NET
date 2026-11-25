using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Hashing;
using System.Text;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Services;
using SkiaSharp;

namespace Noxy.NET.Test.MediaPlayer.Art;

public class ThumbnailServiceTest
{
    private const byte JpegFirstByte = 0xFF;
    private const byte JpegSecondByte = 0xD8;

    private static ThumbnailService Create(int maxPixels = 300)
    {
        return new(Options.Create(new ConfigArt { ThumbnailMaxPixels = maxPixels }));
    }

    private static byte[] MakePng(int width, int height)
    {
        using SKBitmap bitmap = new(width, height);
        bitmap.Erase(SKColors.Red);
        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static SKBitmap Decode(byte[] bytes)
    {
        return SKBitmap.Decode(bytes) ?? throw new InvalidOperationException("The thumbnail did not decode.");
    }

    [Fact]
    public void CreateThumbnail_ShrinksALargeImage_ToTheLongestSide_KeepingTheAspect()
    {
        byte[]? thumbnail = Create().CreateThumbnail(MakePng(1000, 500));

        Assert.NotNull(thumbnail);
        using SKBitmap bitmap = Decode(thumbnail);
        Assert.Equal(300, bitmap.Width);
        Assert.Equal(150, bitmap.Height);
    }

    [Fact]
    public void CreateThumbnail_ShrinksATallImage_ByItsHeight()
    {
        byte[]? thumbnail = Create().CreateThumbnail(MakePng(500, 1000));

        Assert.NotNull(thumbnail);
        using SKBitmap bitmap = Decode(thumbnail);
        Assert.Equal(150, bitmap.Width);
        Assert.Equal(300, bitmap.Height);
    }

    [Fact]
    public void CreateThumbnail_DoesNotEnlargeASmallImage()
    {
        byte[]? thumbnail = Create().CreateThumbnail(MakePng(100, 50));

        Assert.NotNull(thumbnail);
        using SKBitmap bitmap = Decode(thumbnail);
        Assert.Equal(100, bitmap.Width);
        Assert.Equal(50, bitmap.Height);
    }

    [Fact]
    public void CreateThumbnail_ReturnsJpegBytes()
    {
        byte[]? thumbnail = Create().CreateThumbnail(MakePng(400, 400));

        Assert.NotNull(thumbnail);
        Assert.Equal(JpegFirstByte, thumbnail[0]);
        Assert.Equal(JpegSecondByte, thumbnail[1]);
    }

    [Fact]
    public void CreateThumbnail_OfBytesThatAreNotAnImage_ReturnsNull()
    {
        Assert.Null(Create().CreateThumbnail([1, 2, 3, 4, 5]));
    }

    [Fact]
    public void CreateThumbnail_OfNoBytes_ReturnsNull()
    {
        Assert.Null(Create().CreateThumbnail([]));
    }

    [Fact]
    public void CreateThumbnail_UsesTheConfiguredSize()
    {
        byte[]? thumbnail = Create(64).CreateThumbnail(MakePng(1000, 1000));

        Assert.NotNull(thumbnail);
        using SKBitmap bitmap = Decode(thumbnail);
        Assert.Equal(64, bitmap.Width);
        Assert.Equal(64, bitmap.Height);
    }

    // A PNG that is only a header: it says it is 30000 x 30000 and holds no pixels, so decoding it would need 3.6 GB.
    private static byte[] HugePngHeader()
    {
        using MemoryStream stream = new();
        stream.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        byte[] header = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(0), 30000);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), 30000);
        header[8] = 8;
        header[9] = 6;
        WriteChunk(stream, "IHDR", header);
        WriteChunk(stream, "IDAT", [0x78, 0x9C, 0x03, 0x00, 0x00, 0x00, 0x00, 0x01]);
        WriteChunk(stream, "IEND", []);
        return stream.ToArray();
    }

    private static void WriteChunk(Stream stream, string type, byte[] data)
    {
        byte[] length = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
        byte[] typeBytes = Encoding.ASCII.GetBytes(type);
        byte[] crc = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crc, Crc32.HashToUInt32([.. typeBytes, .. data]));
        stream.Write(length);
        stream.Write(typeBytes);
        stream.Write(data);
        stream.Write(crc);
    }

    [Fact]
    public void CreateThumbnail_OfAnImageWithFarTooManyPixels_ReturnsNull_WithoutDecodingIt()
    {
        Stopwatch watch = Stopwatch.StartNew();

        byte[]? thumbnail = Create().CreateThumbnail(HugePngHeader());

        Assert.Null(thumbnail);
        Assert.True(watch.Elapsed < TimeSpan.FromSeconds(2));
    }
}
