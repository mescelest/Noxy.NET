using System.Net;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

public record ApiResponse<T>(HttpStatusCode Status, T? Value, string? ETag)
{
    public bool IsNotModified => Status == HttpStatusCode.NotModified;

    public bool IsNoContent => Status == HttpStatusCode.NoContent;
}
