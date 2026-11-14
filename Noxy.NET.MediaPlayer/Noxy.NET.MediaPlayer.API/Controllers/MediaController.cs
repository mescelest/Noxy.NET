using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Requests.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.API.Controllers;

[Route("[controller]")]
[ApiController]
public class MediaController(IMediaService mediaService, IOptions<ConfigStorage> storage) : ControllerBase
{
    // Room on top of the file for the multipart boundaries, headers and the title field.
    private const long MultipartOverheadBytes = 1024 * 1024;
    private const int MaxTitleChars = 512;

    // Multipart so the file is streamed to disk instead of buffered. The optional "title" field must come before the "file" field.
    // Keep this action free of parameters: any parameter makes MVC read the form first, which consumes the body.
    [HttpPost]
    [EnableRateLimiting(Constants.RateLimitPolicyUpload)]
    public async Task<ActionResult<ResponseUploadAccepted>> Upload()
    {
        CancellationToken cancellationToken = HttpContext.RequestAborted;

        if (UserID is not { } userID) return Unauthorized();

        if (!MediaTypeHeaderValue.TryParse(Request.ContentType, out MediaTypeHeaderValue? contentType)
            || !contentType.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase)
            || StringSegment.IsNullOrEmpty(contentType.Boundary))
        {
            return Problem(statusCode: StatusCodes.Status415UnsupportedMediaType, title: "Expected multipart/form-data.");
        }

        // Kestrel's default limit is lower than the configured maximum file size.
        IHttpMaxRequestBodySizeFeature? bodySize = HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (bodySize is { IsReadOnly: false }) bodySize.MaxRequestBodySize = storage.Value.MaxSizeBytes + MultipartOverheadBytes;

        MultipartReader reader = new(HeaderUtilities.RemoveQuotes(contentType.Boundary).Value!, Request.Body);
        string? title = null;

        while (await reader.ReadNextSectionAsync(cancellationToken) is { } section)
        {
            if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out ContentDispositionHeaderValue? disposition)) continue;

            if (disposition.Name.Value == Constants.UploadTitleField)
            {
                title = await ReadText(section.Body, cancellationToken);
            }
            else if (disposition.Name.Value == Constants.UploadFileField && !StringSegment.IsNullOrEmpty(disposition.FileName))
            {
                ResponseUploadAccepted response = await mediaService.Enqueue(userID, HeaderUtilities.RemoveQuotes(disposition.FileName).Value!, title, section.Body, cancellationToken);

                return StatusCode(StatusCodes.Status202Accepted, response);
            }
        }

        return Problem(statusCode: StatusCodes.Status400BadRequest, title: $$"""Expected a "{{Constants.UploadFileField}}" field.""");
    }

    [HttpGet]
    public async Task<ActionResult<List<ResponseMediaFile>>> GetMediaFileList([FromQuery] RequestListMedia request, CancellationToken cancellationToken)
    {
        if (UserID is not { } userID) return Unauthorized();

        return await mediaService.GetMediaFileList(userID, request.Skip, request.Take, cancellationToken);
    }

    // The whole library for a device's start-up diff. A matching If-None-Match gets a 304 and no body.
    [HttpGet("Library")]
    public async Task<ActionResult<List<ResponseMediaFile>>> GetMediaLibrary(CancellationToken cancellationToken)
    {
        if (UserID is not { } userID) return Unauthorized();

        MediaLibrary library = await mediaService.GetMediaLibrary(userID, Request.Headers.IfNoneMatch.ToString(), cancellationToken);

        Response.Headers.ETag = library.ETag;
        Response.Headers.CacheControl = "private, no-cache";

        if (library.MediaFileList == null) return StatusCode(StatusCodes.Status304NotModified);

        return library.MediaFileList;
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ResponseMediaFile>> Update(Guid id, RequestUpdateMedia request, CancellationToken cancellationToken)
    {
        if (UserID is not { } userID) return Unauthorized();

        return await mediaService.Update(userID, id, request, cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (UserID is not { } userID) return Unauthorized();

        await mediaService.Delete(userID, id, cancellationToken);

        return NoContent();
    }

    [HttpGet("{id:guid}/File")]
    public async Task<ActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        if (UserID is not { } userID) return Unauthorized();

        MediaDownload download = await mediaService.Open(userID, id, cancellationToken);

        // Range requests let a player seek. The file result disposes the stream.
        return File(download.Content, download.ContentType, download.FileName, true);
    }

    private Guid? UserID => Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out Guid userID) ? userID : null;

    private static async Task<string> ReadText(Stream body, CancellationToken cancellationToken)
    {
        using StreamReader reader = new(body);
        char[] buffer = new char[MaxTitleChars];
        int read = await reader.ReadBlockAsync(buffer.AsMemory(), cancellationToken);

        return new(buffer, 0, read);
    }
}
