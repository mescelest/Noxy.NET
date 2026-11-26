using Noxy.NET.MediaPlayer.Domain.Models;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IMediaTagWritingService
{
    // Writes the values into the tags of the audio file at filePath, in the file's own format. Several artists or genres are written as separate values where the format can hold them. A kind with nothing to write clears that tag, and a tag the format cannot hold is skipped. Throws when the file cannot be read as audio.
    void Write(string filePath, MediaTagValues values);
}
