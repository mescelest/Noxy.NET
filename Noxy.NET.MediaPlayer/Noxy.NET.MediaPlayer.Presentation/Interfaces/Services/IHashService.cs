namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

public interface IHashService
{
    // Lower-case hex, the format of a media file's contentHash. Reads the stream from its current position to the end, a buffer at a time, so a large file is never held whole in memory.
    Task<string> Sha256Hex(Stream content, CancellationToken cancellationToken = default);
}
