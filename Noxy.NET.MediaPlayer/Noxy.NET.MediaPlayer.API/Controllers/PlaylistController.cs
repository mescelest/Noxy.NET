using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Application.Models.Playlist;
using Noxy.NET.MediaPlayer.Application.Queries.Playlist;
using Noxy.NET.MediaPlayer.Application.Requests.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.API.Controllers;

[Route("Playlist")]
[ApiController]
public class PlaylistController(IMediator mediator) : UserControllerBase
{
    private const string IdRoute = "{id:guid}";
    private const string EntryListRoute = $"{IdRoute}/EntryList";
    private const string EntryRoute = $"{IdRoute}/Entry";

    private static readonly string CacheControl = new CacheControlHeaderValue { Private = true, NoCache = true }.ToString();

    // The playlists without their entries, for a device's start-up diff. A matching If-None-Match gets a 304 and no body.
    [HttpGet]
    public async Task<ActionResult<List<ResponsePlaylist>>> GetPlaylistList(CancellationToken cancellationToken)
    {
        PlaylistLibrary library = await mediator.Send(new QueryPlaylistList(UserID, Request.Headers.IfNoneMatch.ToString()), cancellationToken);

        Response.Headers.ETag = library.ETag;
        Response.Headers.CacheControl = CacheControl;

        if (library.PlaylistList is null) return StatusCode(StatusCodes.Status304NotModified);

        return library.PlaylistList;
    }

    // One playlist with its entries, loaded when the user opens it.
    [HttpGet(IdRoute)]
    public async Task<ActionResult<ResponsePlaylistDetail>> GetPlaylist(Guid id, CancellationToken cancellationToken)
    {
        PlaylistView view = await mediator.Send(new QueryPlaylist(UserID, id, Request.Headers.IfNoneMatch.ToString()), cancellationToken);

        Response.Headers.ETag = view.ETag;
        Response.Headers.CacheControl = CacheControl;

        if (view.Detail is null) return StatusCode(StatusCodes.Status304NotModified);

        return view.Detail;
    }

    [HttpPost]
    public async Task<ActionResult<ResponsePlaylist>> Create(RequestCreatePlaylist request, CancellationToken cancellationToken)
    {
        ResponsePlaylist response = await mediator.Send(request.ToCommand(UserID), cancellationToken);

        return CreatedAtAction(nameof(GetPlaylist), new { id = response.ID }, response);
    }

    [HttpPatch(IdRoute)]
    public async Task<ActionResult<ResponsePlaylist>> Update(Guid id, RequestUpdatePlaylist request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToCommand(UserID, id), cancellationToken);
    }

    [HttpDelete(IdRoute)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new CommandPlaylistDelete(UserID, id), cancellationToken);

        return NoContent();
    }

    [HttpPost(EntryListRoute)]
    public async Task<ActionResult<ResponsePlaylistDetail>> AddEntryList(Guid id, RequestAddPlaylistEntry request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToCommand(UserID, id), cancellationToken);
    }

    [HttpPost($"{EntryListRoute}/Remove")]
    public async Task<ActionResult<ResponsePlaylistDetail>> RemoveEntryList(Guid id, RequestRemovePlaylistEntry request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToCommand(UserID, id), cancellationToken);
    }

    [HttpPut($"{EntryRoute}/{{entryID:guid}}/Position")]
    public async Task<ActionResult<ResponsePlaylistDetail>> MoveEntry(Guid id, Guid entryID, RequestMovePlaylistEntry request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToCommand(UserID, id, entryID), cancellationToken);
    }
}
