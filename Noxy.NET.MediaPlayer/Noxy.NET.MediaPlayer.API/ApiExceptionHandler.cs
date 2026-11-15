using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Noxy.NET.MediaPlayer.API.Models;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.MediaPlayer.API;

public class ApiExceptionHandler(IProblemDetailsService serviceProblemDetails) : IExceptionHandler
{
    public const string ExistingIDKey = "existingID";

    private const string FileTooLargeTitle = "The file is too large.";

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ApiError? mapped = exception switch
        {
            EmailAlreadyInUseException => new(StatusCodes.Status409Conflict, "The email is already in use."),
            AuthenticationFailedException => new(StatusCodes.Status401Unauthorized, "Invalid credentials."),
            GoogleLoginNotConfiguredException => new(StatusCodes.Status503ServiceUnavailable, "Google login is not configured."),
            UnsupportedMediaTypeException => new(StatusCodes.Status415UnsupportedMediaType, "The file type is not supported."),
            UploadQueueFullException => new(StatusCodes.Status429TooManyRequests, "The upload queue is full."),
            UploadInProgressException => new(StatusCodes.Status429TooManyRequests, "An upload is already in progress."),
            LibraryFullException => new(StatusCodes.Status409Conflict, "The library limit has been reached."),
            MediaNotFoundException => new(StatusCodes.Status404NotFound, "The media file was not found."),
            PlaylistNotFoundException => new(StatusCodes.Status404NotFound, "The playlist was not found."),
            MetadataItemNotFoundException => new(StatusCodes.Status404NotFound, "The item was not found."),
            NameAlreadyInUseException => new(StatusCodes.Status409Conflict, "An item with that name already exists."),
            MetadataKeyConflictException => new(StatusCodes.Status409Conflict, "An item with that name already exists."),
            InvalidMetadataOperationException invalid => new(StatusCodes.Status400BadRequest, invalid.Message),
            SettingNotFoundException => new(StatusCodes.Status404NotFound, "The setting was not found."),
            InvalidSettingValueException invalidSetting => new(StatusCodes.Status400BadRequest, invalidSetting.Message),
            PlaylistEntryNotFoundException => new(StatusCodes.Status404NotFound, "The playlist entry was not found."),
            PlaylistLimitReachedException => new(StatusCodes.Status409Conflict, "The playlist limit has been reached."),
            PlaylistEntryLimitReachedException => new(StatusCodes.Status409Conflict, "The playlist is full."),
            PlaylistHasDuplicatesException => new(StatusCodes.Status409Conflict, "The playlist contains duplicate media files."),
            InvalidPlaylistPositionException => new(StatusCodes.Status400BadRequest, "The position is outside the playlist."),
            FileTooLargeException => new(StatusCodes.Status413PayloadTooLarge, FileTooLargeTitle),
            // Thrown by Kestrel when the body goes over the limit the upload action set.
            BadHttpRequestException { StatusCode: StatusCodes.Status413PayloadTooLarge } => new(StatusCodes.Status413PayloadTooLarge, FileTooLargeTitle),
            _ => null
        };

        // Anything unrecognized falls through to the default 500 handling.
        if (mapped is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = mapped.Status;

        // A name that is taken tells the client which item has it, so it can offer to merge.
        return await serviceProblemDetails.TryWriteAsync(new()
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = CreateProblemDetails(mapped, exception)
        });
    }

    private static ProblemDetails CreateProblemDetails(ApiError mapped, Exception exception)
    {
        ProblemDetails details = new() { Status = mapped.Status, Title = mapped.Title };
        if (exception is NameAlreadyInUseException nameInUse) details.Extensions[ExistingIDKey] = nameInUse.ExistingID;

        return details;
    }
}
