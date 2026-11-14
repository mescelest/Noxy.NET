using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Requests.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.API.Controllers;

[Route("Playlists")]
[ApiController]
public class PlaylistController(IPlaylistService playlistService) : ControllerBase
{
    private const string IdRoute = "{id:guid}";
    private const string EntryListRoute = $"{IdRoute}/Entries";

    private static readonly string CacheControl = new CacheControlHeaderValue { Private = true, NoCache = true }.ToString();

    // The playlists without their entries, for a device's start-up diff. A matching If-None-Match gets a 304 and no body.
    [HttpGet]
    public async Task<ActionResult<List<ResponsePlaylist>>> GetPlaylistLibrary(CancellationToken cancellationToken)
    {
        if (UserID is not { } userID) return Unauthorized();

        PlaylistLibrary library = await playlistService.GetPlaylistLibrary(userID, Request.Headers.IfNoneMatch.ToString(), cancellationToken);

        Response.Headers.ETag = library.ETag;
        Response.Headers.CacheControl = CacheControl;

        if (library.PlaylistList is null) return StatusCode(StatusCodes.Status304NotModified);

        return library.PlaylistList;
    }

    // One playlist with its entries, loaded when the user opens it.
    [HttpGet(IdRoute)]
    public async Task<ActionResult<ResponsePlaylistDetail>> GetPlaylist(Guid id, CancellationToken cancellationToken)
    {
        if (UserID is not { } userID) return Unauthorized();

        PlaylistView view = await playlistService.GetPlaylist(userID, id, Request.Headers.IfNoneMatch.ToString(), cancellationToken);

        Response.Headers.ETag = view.ETag;
        Response.Headers.CacheControl = CacheControl;

        if (view.Detail is null) return StatusCode(StatusCodes.Status304NotModified);

        return view.Detail;
    }

    [HttpPost]
    public async Task<ActionResult<ResponsePlaylist>> Create(RequestCreatePlaylist request, CancellationToken cancellationToken)
    {
        if (UserID is not { } userID) return Unauthorized();

        ResponsePlaylist response = await playlistService.Create(userID, request, cancellationToken);

        return CreatedAtAction(nameof(GetPlaylist), new { id = response.ID }, response);
    }

    [HttpPatch(IdRoute)]
    public async Task<ActionResult<ResponsePlaylist>> Update(Guid id, RequestUpdatePlaylist request, CancellationToken cancellationToken)
    {
        if (UserID is not { } userID) return Unauthorized();

        return await playlistService.Update(userID, id, request, cancellationToken);
    }

    [HttpDelete(IdRoute)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (UserID is not { } userID) return Unauthorized();

        await playlistService.Delete(userID, id, cancellationToken);

        return NoContent();
    }

    [HttpPost(EntryListRoute)]
    public async Task<ActionResult<ResponsePlaylistDetail>> AddEntryList(Guid id, RequestAddPlaylistEntry request, CancellationToken cancellationToken)
    {
        if (UserID is not { } userID) return Unauthorized();

        return await playlistService.AddEntryList(userID, id, request, cancellationToken);
    }

    [HttpPost($"{EntryListRoute}/Remove")]
    public async Task<ActionResult<ResponsePlaylistDetail>> RemoveEntryList(Guid id, RequestRemovePlaylistEntry request, CancellationToken cancellationToken)
    {
        if (UserID is not { } userID) return Unauthorized();

        return await playlistService.RemoveEntryList(userID, id, request, cancellationToken);
    }

    [HttpPut($"{EntryListRoute}/{{entryID:guid}}/Position")]
    public async Task<ActionResult<ResponsePlaylistDetail>> MoveEntry(Guid id, Guid entryID, RequestMovePlaylistEntry request, CancellationToken cancellationToken)
    {
        if (UserID is not { } userID) return Unauthorized();

        return await playlistService.MoveEntry(userID, id, entryID, request, cancellationToken);
    }

    private Guid? UserID => Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out Guid userID) ? userID : null;
}
