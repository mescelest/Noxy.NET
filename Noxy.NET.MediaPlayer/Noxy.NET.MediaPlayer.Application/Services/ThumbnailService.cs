using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using SkiaSharp;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class ThumbnailService(IOptions<ConfigArt> optionsArt) : IThumbnailService
{
    private const int JpegQuality = 80;

    // A picture of more pixels than this is not decoded: it would take gigabytes of memory for a thumbnail. 40 million is 8000 x 5000.
    private const long MaxSourcePixels = 40_000_000;

    public byte[]? CreateThumbnail(byte[] image)
    {
        if (image.Length == 0)
        {
            return null;
        }

        try
        {
            using SKData data = SKData.CreateCopy(image);
            using SKCodec? codec = SKCodec.Create(data);
            if (codec is null || codec.Info.Width <= 0 || codec.Info.Height <= 0 || (long)codec.Info.Width * codec.Info.Height > MaxSourcePixels)
            {
                return null;
            }

            using SKBitmap? source = SKBitmap.Decode(codec);
            return source is null ? null : Encode(source);
        }
        catch (Exception)
        {
            // Skia reports a picture it cannot decode or allocate with a plain Exception, and a bad picture must never fail the upload that carried it.
            return null;
        }
    }

    private byte[]? Encode(SKBitmap source)
    {
        int longestSide = Math.Max(source.Width, source.Height);
        int maxPixels = optionsArt.Value.ThumbnailMaxPixels;

        if (longestSide <= maxPixels)
        {
            return EncodeJpeg(source);
        }

        double scale = (double)maxPixels / longestSide;
        int width = Math.Max(1, (int)Math.Round(source.Width * scale));
        int height = Math.Max(1, (int)Math.Round(source.Height * scale));

        using SKBitmap? resized = source.Resize(new SKImageInfo(width, height), new SKSamplingOptions(SKCubicResampler.Mitchell));
        return resized is null ? null : EncodeJpeg(resized);
    }

    private static byte[] EncodeJpeg(SKBitmap bitmap)
    {
        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Jpeg, JpegQuality);
        return data.ToArray();
    }
}
