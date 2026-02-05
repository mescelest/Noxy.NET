using MediatR;
using Microsoft.AspNetCore.Mvc;
using Noxy.NET.EntityManagement.API.Queries;

namespace Noxy.NET.EntityManagement.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DataController(IMediator mediator) : ControllerBase
{
    [HttpPost("Parameter/Text/Resolve")]
    public async Task<ActionResult<Dictionary<string, string>>> ResolveTextParameterList([FromBody] ResolveTextParameterListQuery query)
    {
        return await mediator.Send(query);
    }
}
