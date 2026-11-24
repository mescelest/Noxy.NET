using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Queries.Metadata;
using Noxy.NET.MediaPlayer.Application.Requests.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.API.Controllers;

[Route("Album")]
[ApiController]
public class AlbumController(IMediator mediator) : UserControllerBase
{
    private const string IdRoute = "{id:guid}";
    private const string MediaFileListRoute = $"{IdRoute}/MediaFileList";
    private const string ArtContentType = "image/jpeg";

    private static readonly string CacheControl = new CacheControlHeaderValue { Private = true, NoCache = true }.ToString();
    private static readonly string ArtCacheControl = new CacheControlHeaderValue { Private = true }.ToString();

    // The albums of the user with their artists and file counts, for a device start-up diff. A matching If-None-Match gets a 304 and no body.
    [HttpGet]
    public async Task<ActionResult<List<ResponseAlbum>>> GetAlbumList(CancellationToken cancellationToken)
    {
        MetadataLibrary<ResponseAlbum> library = await mediator.Send(new QueryAlbumList(UserID, Request.Headers.IfNoneMatch.ToString()), cancellationToken);

        Response.Headers.ETag = library.ETag;
        Response.Headers.CacheControl = CacheControl;

        if (library.ItemList is null) return StatusCode(StatusCodes.Status304NotModified);

        return library.ItemList;
    }

    // The album's thumbnail. A matching If-None-Match gets a 304 and no body; an unknown album, another user's album or an album with no picture gets a 404.
    [HttpGet($"{IdRoute}/Art")]
    public async Task<ActionResult> GetArt(Guid id, CancellationToken cancellationToken)
    {
        AlbumArtPicture? picture = await mediator.Send(new QueryAlbumArt(UserID, id, Request.Headers.IfNoneMatch.ToString()), cancellationToken);
        if (picture is null) return NotFound();

        Response.Headers.ETag = picture.ETag;
        Response.Headers.CacheControl = ArtCacheControl;

        if (picture.Bytes is null) return StatusCode(StatusCodes.Status304NotModified);

        return File(picture.Bytes, ArtContentType);
    }

    [HttpPost]
    public async Task<ActionResult<ResponseAlbum>> Create(RequestCreateAlbum request, CancellationToken cancellationToken)
    {
        ResponseAlbum response = await mediator.Send(request.ToCommand(UserID), cancellationToken);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPatch(IdRoute)]
    public async Task<ActionResult<ResponseAlbum>> Update(Guid id, RequestUpdateAlbum request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToCommand(UserID, id), cancellationToken);
    }

    [HttpDelete(IdRoute)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new CommandAlbumDelete(UserID, id), cancellationToken);

        return NoContent();
    }

    // Sets the album of the files, replacing the one they had.
    [HttpPost(MediaFileListRoute)]
    public async Task<ActionResult<ResponseAlbum>> AddMediaFileList(Guid id, RequestAddMediaFileList request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToAlbumCommand(UserID, id), cancellationToken);
    }

    // Clears the album from the files.
    [HttpPost($"{MediaFileListRoute}/Remove")]
    public async Task<ActionResult<ResponseAlbum>> RemoveMediaFileList(Guid id, RequestRemoveMediaFileList request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToAlbumCommand(UserID, id), cancellationToken);
    }

    // Moves every file of the album to the target album and removes the album.
    [HttpPost($"{IdRoute}/Merge")]
    public async Task<ActionResult<ResponseAlbum>> Merge(Guid id, RequestMergeItem request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToAlbumCommand(UserID, id), cancellationToken);
    }
}
