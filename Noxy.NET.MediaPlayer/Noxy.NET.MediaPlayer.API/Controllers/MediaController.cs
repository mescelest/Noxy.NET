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
    public async Task<ActionResult<ResponseMediaFile>> Upload()
    {
        CancellationToken cancellationToken = HttpContext.RequestAborted;

        if (!Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out Guid userID)) return Unauthorized();

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

            if (disposition.Name.Value == "title")
            {
                title = await ReadText(section.Body, cancellationToken);
            }
            else if (disposition.Name.Value == "file" && !StringSegment.IsNullOrEmpty(disposition.FileName))
            {
                ResponseMediaFile response = await mediaService.Upload(userID, HeaderUtilities.RemoveQuotes(disposition.FileName).Value!, title, section.Body, cancellationToken);

                return StatusCode(StatusCodes.Status201Created, response);
            }
        }

        return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Expected a \"file\" field.");
    }

    private static async Task<string> ReadText(Stream body, CancellationToken cancellationToken)
    {
        using StreamReader reader = new(body);
        char[] buffer = new char[MaxTitleChars];
        int read = await reader.ReadBlockAsync(buffer.AsMemory(), cancellationToken);

        return new(buffer, 0, read);
    }
}
