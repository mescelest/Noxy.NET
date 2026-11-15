using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Noxy.NET.MediaPlayer.Application.Models.Setting;
using Noxy.NET.MediaPlayer.Application.Queries.Setting;
using Noxy.NET.MediaPlayer.Application.Requests.Setting;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;

namespace Noxy.NET.MediaPlayer.API.Controllers;

[Route("Settings")]
[ApiController]
public class SettingController(IMediator mediator) : UserControllerBase
{
    private static readonly string CacheControl = new CacheControlHeaderValue { Private = true, NoCache = true }.ToString();

    // Every declared setting of the signed-in user, with the default where nothing is saved. A matching If-None-Match gets a 304 and no body.
    [HttpGet]
    public async Task<ActionResult<List<ResponseSetting>>> GetSettingList(CancellationToken cancellationToken)
    {
        SettingLibrary library = await mediator.Send(new QuerySettingList(UserID, Request.Headers.IfNoneMatch.ToString()), cancellationToken);

        Response.Headers.ETag = library.ETag;
        Response.Headers.CacheControl = CacheControl;

        if (library.SettingList is null) return StatusCode(StatusCodes.Status304NotModified);

        return library.SettingList;
    }

    [HttpPatch("{key}")]
    public async Task<ActionResult<ResponseSetting>> Update(string key, RequestUpdateSetting request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToCommand(UserID, key), cancellationToken);
    }
}
