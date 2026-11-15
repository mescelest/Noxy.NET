using System.Net.Mime;
using Mediator;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Noxy.NET.MediaPlayer.Application.Commands.Media;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Models.Media;
using Noxy.NET.MediaPlayer.Application.Queries.Media;
using Noxy.NET.MediaPlayer.Application.Requests.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.API.Controllers;

[Route("[controller]")]
[ApiController]
public class MediaController(IMediator mediator, IOptions<ConfigStorage> configStorage) : UserControllerBase
{
    // Room on top of the file for the multipart boundaries, headers and the title field.
    private const long MultipartOverheadBytes = 1024 * 1024;
    private const int MaxTitleChars = 512;
    private const string IdRoute = "{id:guid}";

    private static readonly string LibraryCacheControl = new CacheControlHeaderValue { Private = true, NoCache = true }.ToString();

    // Multipart so the file is streamed to disk instead of buffered. The optional "title" field must come before the "file" field.
    // Keep this action free of parameters: any parameter makes MVC read the form first, which consumes the body.
    [HttpPost]
    [EnableRateLimiting(Constants.RateLimitPolicyUpload)]
    public async Task<ActionResult<ResponseUploadAccepted>> Upload()
    {
        CancellationToken cancellationToken = HttpContext.RequestAborted;

        if (!MediaTypeHeaderValue.TryParse(Request.ContentType, out MediaTypeHeaderValue? contentType)
            || !contentType.MediaType.Equals(MediaTypeNames.Multipart.FormData, StringComparison.OrdinalIgnoreCase)
            || StringSegment.IsNullOrEmpty(contentType.Boundary))
        {
            return Problem(statusCode: StatusCodes.Status415UnsupportedMediaType, title: "Expected multipart/form-data.");
        }

        // Kestrel's default limit is lower than the configured maximum file size.
        IHttpMaxRequestBodySizeFeature? bodySize = HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (bodySize is { IsReadOnly: false }) bodySize.MaxRequestBodySize = configStorage.Value.MaxSizeBytes + MultipartOverheadBytes;

        MultipartReader reader = new(HeaderUtilities.RemoveQuotes(contentType.Boundary).ToString(), Request.Body);
        string? title = null;

        while (await reader.ReadNextSectionAsync(cancellationToken) is { } section)
        {
            if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out ContentDispositionHeaderValue? disposition)) continue;

            switch (disposition.Name.Value)
            {
                case Constants.UploadTitleField:
                    title = await ReadText(section.Body, cancellationToken);
                    break;
                case Constants.UploadFileField when !StringSegment.IsNullOrEmpty(disposition.FileName):
                {
                    ResponseUploadAccepted response = await mediator.Send(new CommandMediaUpload(UserID, HeaderUtilities.RemoveQuotes(disposition.FileName).ToString(), title, section.Body), cancellationToken);
                    return StatusCode(StatusCodes.Status202Accepted, response);
                }
            }
        }

        return Problem(statusCode: StatusCodes.Status400BadRequest, title: $$"""Expected a "{{Constants.UploadFileField}}" field.""");
    }

    [HttpGet]
    public async Task<ActionResult<List<ResponseMediaFile>>> GetMediaFileList([FromQuery] RequestListMedia request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToQuery(UserID), cancellationToken);
    }

    // The whole library for a device's start-up diff. A matching If-None-Match gets a 304 and no body.
    [HttpGet("Library")]
    public async Task<ActionResult<List<ResponseMediaFile>>> GetMediaLibrary(CancellationToken cancellationToken)
    {
        MediaLibrary library = await mediator.Send(new QueryMediaLibrary(UserID, Request.Headers.IfNoneMatch.ToString()), cancellationToken);

        Response.Headers.ETag = library.ETag;
        Response.Headers.CacheControl = LibraryCacheControl;

        if (library.MediaFileList is null) return StatusCode(StatusCodes.Status304NotModified);

        return library.MediaFileList;
    }

    [HttpPatch(IdRoute)]
    public async Task<ActionResult<ResponseMediaFile>> Update(Guid id, RequestUpdateMedia request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToCommand(UserID, id), cancellationToken);
    }

    [HttpDelete(IdRoute)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new CommandMediaDelete(UserID, id), cancellationToken);

        return NoContent();
    }

    [HttpGet($"{IdRoute}/File")]
    public async Task<ActionResult> GetFile(Guid id, CancellationToken cancellationToken)
    {
        MediaDownload download = await mediator.Send(new QueryMediaFile(UserID, id), cancellationToken);

        // The hash is the ETag, so a device can resume a partial download only against the same bytes. Range requests also let a player seek. The file result disposes the stream.
        return File(download.Content, download.ContentType, download.FileName, lastModified: null, entityTag: new EntityTagHeaderValue($"\"{download.ContentHash}\""), enableRangeProcessing: true);
    }

    // A copy of the media file with its current tags written into it, for a user who saves the file. Rate limited, because every request makes a copy.
    [HttpGet($"{IdRoute}/Download")]
    [EnableRateLimiting(Constants.RateLimitPolicyDownload)]
    public async Task<ActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        MediaDownload download = await mediator.Send(new QueryMediaFileWithTags(UserID, id), cancellationToken);

        // No range processing: every request makes its own copy, so the pieces of a resumed download could come from differently tagged copies.
        return File(download.Content, download.ContentType, download.FileName, false);
    }


    private static async Task<string> ReadText(Stream body, CancellationToken cancellationToken)
    {
        using StreamReader reader = new(body);
        char[] buffer = new char[MaxTitleChars];
        int read = await reader.ReadBlockAsync(buffer.AsMemory(), cancellationToken);

        return new(buffer, 0, read);
    }
}
