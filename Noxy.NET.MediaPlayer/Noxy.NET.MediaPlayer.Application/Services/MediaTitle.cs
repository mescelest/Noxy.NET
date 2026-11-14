namespace Noxy.NET.MediaPlayer.Application.Services;

public static class MediaTitle
{
    private const int MaxLength = 255;
    private const string DefaultTitle = "Untitled";

    public static string Resolve(string? title, string fileName)
    {
        string resolved = (string.IsNullOrWhiteSpace(title) ? Path.GetFileNameWithoutExtension(fileName) : title).Trim();
        switch (resolved.Length)
        {
            case 0:
                return DefaultTitle;
            case <= MaxLength:
                return resolved;
        }

        resolved = resolved[..MaxLength];

        // Don't leave half of a surrogate pair (an emoji, say), and don't end on the space the cut exposed.
        if (char.IsHighSurrogate(resolved[^1])) resolved = resolved[..^1];
        return resolved.TrimEnd();
    }
}
