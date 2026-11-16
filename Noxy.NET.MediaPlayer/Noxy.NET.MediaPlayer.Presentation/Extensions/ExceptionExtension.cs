using Noxy.NET.MediaPlayer.Presentation.Exceptions;

namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class ExceptionExtension
{
    extension(Exception exception)
    {
        // True when the call never reached the server: no address saved, the network down, or a timeout. A server that answered with an error is not offline.
        public bool IsOfflineFailure => exception is HttpRequestException or ServerAddressMissingException or TaskCanceledException;
    }
}
