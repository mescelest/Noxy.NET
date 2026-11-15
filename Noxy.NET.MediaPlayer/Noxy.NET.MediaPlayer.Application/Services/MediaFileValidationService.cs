using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Upload;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class MediaFileValidationService(IOptions<ConfigStorage> configStorage) : IMediaFileValidationService
{
    private delegate bool HeaderMatcher(ReadOnlySpan<byte> header);

    // Extensions we can verify. An allowed extension missing from here is rejected, since it cannot be checked.
    private static readonly Dictionary<string, (string ContentType, HeaderMatcher Matches)> Formats = new()
    {
        ["mp3"] = ("audio/mpeg", IsMp3),
        ["flac"] = ("audio/flac", header => header.StartsWith("fLaC"u8)),
        ["m4a"] = ("audio/mp4", header => header.Length >= 8 && header[4..8].SequenceEqual("ftyp"u8)),
        ["ogg"] = ("audio/ogg", header => header.StartsWith("OggS"u8)),
        ["opus"] = ("audio/ogg", header => header.StartsWith("OggS"u8)),
        ["wav"] = ("audio/wav", header => header.StartsWith("RIFF"u8) && header.Length >= 12 && header[8..12].SequenceEqual("WAVE"u8))
    };

    private readonly HashSet<string> _allowed = configStorage.Value.AllowedExtensionList
        .Select(extension => extension.TrimStart('.').ToLowerInvariant())
        .ToHashSet();

    public ValidatedMediaFile Validate(string fileName, ReadOnlySpan<byte> header)
    {
        // Only the last extension counts, so "media.test.mp3" is mp3 and "media.mp3.exe" is exe.
        string extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();

        if (!_allowed.Contains(extension) || !Formats.TryGetValue(extension, out var format) || !format.Matches(header))
        {
            throw new UnsupportedMediaTypeException();
        }

        return new(extension, format.ContentType);
    }

    private static bool IsMp3(ReadOnlySpan<byte> header)
    {
        // An ID3 tag, or an MPEG frame sync (11 set bits).
        return header.StartsWith("ID3"u8) || header is [0xFF, >= 0xE0, ..];
    }
}
