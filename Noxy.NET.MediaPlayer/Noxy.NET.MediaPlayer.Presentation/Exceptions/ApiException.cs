using System.Net;

namespace Noxy.NET.MediaPlayer.Presentation.Exceptions;

public class ApiException(HttpStatusCode status, string title) : Exception(title)
{
    public HttpStatusCode Status { get; } = status;
    public string Title { get; } = title;
}
