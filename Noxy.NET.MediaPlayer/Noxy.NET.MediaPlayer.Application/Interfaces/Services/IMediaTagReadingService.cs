using Noxy.NET.MediaPlayer.Application.Models.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IMediaTagReadingService
{
    // Reads the tags and properties of the audio file at path. Never throws for a file it cannot read: an unreadable or non-audio file gives an empty set, and the failure is logged without the file's contents. A tag the format cannot hold is empty, and zero numbers are not set.
    MediaTagSet Read(string path);
}
