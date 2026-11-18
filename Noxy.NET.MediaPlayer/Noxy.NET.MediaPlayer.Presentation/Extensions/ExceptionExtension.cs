using Noxy.NET.MediaPlayer.Presentation.Exceptions;

namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class ExceptionExtension
{
    extension(Exception exception)
    {
        // True when the call never reached the server: no address saved, the network down, or a timeout. A server that answered with an error is not offline.
        public bool IsOfflineFailure => exception is HttpRequestException or ServerAddressMissingException or TaskCanceledException;

        // What the sign-in form shows: the title the server answered with, or one fixed sentence. Never the text of another exception, which could carry an address or a token.
        public string ToSignInMessage()
        {
            if (exception is ApiException apiException && !string.IsNullOrWhiteSpace(apiException.Title)) return apiException.Title;
            if (exception is ServerAddressMissingException) return Constants.SignInMessageNoAddress;
            if (exception.IsOfflineFailure) return Constants.SignInMessageOffline;

            return Constants.SignInMessageFailed;
        }

        // What a page shows when an action failed: the server's title, the offline sentence, or one fixed sentence. Never the text of another exception.
        public string ToActionMessage(string? failedMessage = null)
        {
            if (exception is ApiException { Title.Length: > 0 } apiException) return apiException.Title;
            if (exception.IsOfflineFailure) return Constants.ActionMessageOffline;

            return failedMessage ?? Constants.ActionMessageFailed;
        }
    }
}
