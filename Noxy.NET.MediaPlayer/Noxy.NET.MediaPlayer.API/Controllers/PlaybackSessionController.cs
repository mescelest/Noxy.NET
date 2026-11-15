using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Noxy.NET.MediaPlayer.Application.Commands.Playback;
using Noxy.NET.MediaPlayer.Application.Models.Playback;
using Noxy.NET.MediaPlayer.Application.Queries.Playback;
using Noxy.NET.MediaPlayer.Application.Requests.Playback;
using Noxy.NET.MediaPlayer.Domain.Responses.Playback;

namespace Noxy.NET.MediaPlayer.API.Controllers;

[Route("PlaybackSession")]
[ApiController]
public class PlaybackSessionController(IMediator mediator) : UserControllerBase
{
    private static readonly string CacheControl = new CacheControlHeaderValue { Private = true, NoCache = true }.ToString();

    // The user's saved session: 200 with it and its ETag, 304 for a matching If-None-Match, 204 when there is none.
    [HttpGet]
    public async Task<ActionResult<ResponsePlaybackSession>> Get(CancellationToken cancellationToken)
    {
        PlaybackSessionView view = await mediator.Send(new QueryPlaybackSession(UserID, Request.Headers.IfNoneMatch.ToString()), cancellationToken);

        if (view.ETag is null) return NoContent();

        Response.Headers.ETag = view.ETag;
        Response.Headers.CacheControl = CacheControl;

        if (view.Session is null) return StatusCode(StatusCodes.Status304NotModified);

        return view.Session;
    }

    [HttpPut]
    public async Task<ActionResult<ResponsePlaybackSession>> Save(RequestSavePlaybackSession request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToCommand(UserID), cancellationToken);
    }

    [HttpDelete]
    public async Task<ActionResult> Clear(CancellationToken cancellationToken)
    {
        await mediator.Send(new CommandPlaybackSessionClear(UserID), cancellationToken);

        return NoContent();
    }
}
