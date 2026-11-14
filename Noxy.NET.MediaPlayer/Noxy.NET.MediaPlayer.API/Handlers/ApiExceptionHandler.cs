using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Noxy.NET.MediaPlayer.API.Models;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.MediaPlayer.API.Handlers;

public class ApiExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ApiError? mapped = exception switch
        {
            EmailAlreadyInUseException => new(StatusCodes.Status409Conflict, "The email is already in use."),
            AuthenticationFailedException => new(StatusCodes.Status401Unauthorized, "Invalid credentials."),
            GoogleLoginNotConfiguredException => new(StatusCodes.Status503ServiceUnavailable, "Google login is not configured."),
            UnsupportedMediaTypeException => new(StatusCodes.Status415UnsupportedMediaType, "The file type is not supported."),
            FileTooLargeException => new(StatusCodes.Status413PayloadTooLarge, "The file is too large."),
            // Thrown by Kestrel when the body goes over the limit the upload action set.
            BadHttpRequestException { StatusCode: StatusCodes.Status413PayloadTooLarge } => new(StatusCodes.Status413PayloadTooLarge, "The file is too large."),
            _ => null
        };

        // Anything unrecognised falls through to the default 500 handling.
        if (mapped is not { } error)
        {
            return false;
        }

        httpContext.Response.StatusCode = error.Status;

        return await problemDetailsService.TryWriteAsync(new()
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = error.Status, Title = error.Title }
        });
    }
}
