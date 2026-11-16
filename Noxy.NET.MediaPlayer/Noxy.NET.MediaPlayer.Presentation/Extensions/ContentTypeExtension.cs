namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class ContentTypeExtension
{
    private const string FallbackExtension = ".bin";

    private static readonly Dictionary<string, string> ExtensionMap = new()
    {
        ["audio/mpeg"] = ".mp3",
        ["audio/mp4"] = ".m4a",
        ["audio/x-m4a"] = ".m4a",
        ["audio/flac"] = ".flac",
        ["audio/ogg"] = ".ogg",
        ["audio/wav"] = ".wav",
        ["audio/x-wav"] = ".wav"
    };

    extension(string contentType)
    {
        // The platform players pick a decoder partly by the file extension. Parameters (audio/ogg; codecs=opus) and case are ignored.
        public string GetFileExtension()
        {
            string mediaType = contentType.Split(';')[0].Trim().ToLowerInvariant();

            return ExtensionMap.GetValueOrDefault(mediaType, FallbackExtension);
        }
    }
}
