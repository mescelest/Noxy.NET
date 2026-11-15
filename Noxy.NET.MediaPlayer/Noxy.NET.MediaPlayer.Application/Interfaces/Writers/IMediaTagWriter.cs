using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Writers;

public interface IMediaTagWriter
{
    // Writes the metadata into the tags of the audio file at filePath, in the file's own format. A null field clears that tag, and a tag the format cannot hold is skipped. Throws when the file cannot be read as audio.
    void Write(string filePath, EntityMediaFileMetadata metadata);
}
