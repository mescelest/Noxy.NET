using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.MediaPlayer.API.Handlers;

public class ApiExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        (int Status, string Title)? mapped = exception switch
        {
            EmailAlreadyInUseException => (StatusCodes.Status409Conflict, "The email is already in use."),
            AuthenticationFailedException => (StatusCodes.Status401Unauthorized, "Invalid credentials."),
            GoogleLoginNotConfiguredException => (StatusCodes.Status503ServiceUnavailable, "Google login is not configured."),
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
