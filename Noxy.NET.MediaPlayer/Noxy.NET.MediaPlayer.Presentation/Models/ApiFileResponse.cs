using System.Net;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

public sealed record ApiFileResponse(HttpStatusCode Status, Stream Content, string? ETag, long? TotalBytes, IDisposable? Owner = null) : IDisposable
{
    public void Dispose()
    {
        Content.Dispose();
        Owner?.Dispose();
    }
}
