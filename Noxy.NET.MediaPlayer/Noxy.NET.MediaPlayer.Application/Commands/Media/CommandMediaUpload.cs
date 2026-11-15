using Mediator;
using Noxy.NET.MediaPlayer.Application.Extensions;
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

        return resolved.Length == 0 ? DefaultTitle : resolved.CutAt(MaxTitleLength);
    }
}
