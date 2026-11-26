using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Queries.Metadata;
using Noxy.NET.MediaPlayer.Application.Requests.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.API.Controllers;

[Route("Artist")]
[ApiController]
public class ArtistController(IMediator mediator) : UserControllerBase
{
    private const string IdRoute = "{id:guid}";
    private const string MediaFileListRoute = $"{IdRoute}/MediaFileList";
    private const string WeightRoute = $"{IdRoute}/Weight";

    private static readonly string CacheControl = new CacheControlHeaderValue { Private = true, NoCache = true }.ToString();

    // The items of the user with their file counts, for a device start-up diff. A matching If-None-Match gets a 304 and no body.
    [HttpGet]
    public async Task<ActionResult<List<ResponseArtist>>> GetArtistList(CancellationToken cancellationToken)
    {
        MetadataLibrary<ResponseArtist> library = await mediator.Send(new QueryArtistList(UserID, Request.Headers.IfNoneMatch.ToString()), cancellationToken);

        Response.Headers.ETag = library.ETag;
        Response.Headers.CacheControl = CacheControl;

        if (library.ItemList is null) return StatusCode(StatusCodes.Status304NotModified);

        return library.ItemList;
    }

    [HttpPost]
    public async Task<ActionResult<ResponseArtist>> Create(RequestCreateArtist request, CancellationToken cancellationToken)
    {
        ResponseArtist response = await mediator.Send(request.ToCommand(UserID), cancellationToken);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPatch(IdRoute)]
    public async Task<ActionResult<ResponseArtist>> Update(Guid id, RequestUpdateArtist request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToCommand(UserID, id), cancellationToken);
    }


    [HttpPut(WeightRoute)]
    public async Task<ActionResult<ResponseArtist>> SetWeight(Guid id, RequestSetArtistWeight request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToCommand(UserID, id), cancellationToken);
    }

    [HttpDelete(IdRoute)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new CommandArtistDelete(UserID, id), cancellationToken);

        return NoContent();
    }

    // Adds the item to the files, at the end of their list.
    [HttpPost(MediaFileListRoute)]
    public async Task<ActionResult<ResponseArtist>> AddMediaFileList(Guid id, RequestAddMediaFileList request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToArtistCommand(UserID, id), cancellationToken);
    }

    // Clears the item from the files.
    [HttpPost($"{MediaFileListRoute}/Remove")]
    public async Task<ActionResult<ResponseArtist>> RemoveMediaFileList(Guid id, RequestRemoveMediaFileList request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToArtistCommand(UserID, id), cancellationToken);
    }

    // Moves everything of the item to the target item and removes the item.
    [HttpPost($"{IdRoute}/Merge")]
    public async Task<ActionResult<ResponseArtist>> Merge(Guid id, RequestMergeItem request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToArtistCommand(UserID, id), cancellationToken);
    }

    // Replaces the item by several, named in the request, and returns them.
    [HttpPost($"{IdRoute}/Split")]
    public async Task<ActionResult<List<ResponseArtist>>> Split(Guid id, RequestSplitItem request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request.ToArtistCommand(UserID, id), cancellationToken);
    }
}
