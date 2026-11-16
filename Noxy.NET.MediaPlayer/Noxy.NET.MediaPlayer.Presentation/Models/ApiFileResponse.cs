using System.Net;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

// Status is OK (the whole file) or PartialContent (the bytes from the requested start). TotalBytes is the size of the whole file. Disposing it disposes the stream and the underlying response; the caller disposes it.
public sealed record ApiFileResponse(HttpStatusCode Status, Stream Content, string? ETag, long? TotalBytes, IDisposable? Owner = null) : IDisposable
{
    public void Dispose()
    {
        Content.Dispose();
        Owner?.Dispose();
    }
}
