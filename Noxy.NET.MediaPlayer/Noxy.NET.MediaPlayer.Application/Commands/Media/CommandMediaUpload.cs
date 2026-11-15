using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Commands.Media;

// The caller owns Content and disposes it: the handler only reads it.
public record CommandMediaUpload(Guid UserID, string FileName, string? Title, Stream Content) : ICommand<ResponseUploadAccepted>
{
    private const int MaxTitleLength = 255;
    private const string DefaultTitle = "Untitled";

    // The title that was sent, or the file name without its extension when none was.
    public string ResolveTitle()
    {
        string resolved = (string.IsNullOrWhiteSpace(Title) ? Path.GetFileNameWithoutExtension(FileName) : Title).Trim();
        switch (resolved.Length)
        {
            case 0:
                return DefaultTitle;
            case <= MaxTitleLength:
                return resolved;
        }

        resolved = resolved[..MaxTitleLength];

        // Don't leave half of a surrogate pair (an emoji, say), and don't end on the space the cut exposed.
        if (char.IsHighSurrogate(resolved[^1])) resolved = resolved[..^1];
        return resolved.TrimEnd();
    }
}
