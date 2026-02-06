using MediatR;
using Microsoft.AspNetCore.Mvc;
using Noxy.NET.EntityManagement.API.Queries;
using Noxy.NET.EntityManagement.Domain.Requests;

namespace Noxy.NET.EntityManagement.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DataController(IMediator mediator) : ControllerBase
{
    [HttpPost("Parameter/Text/Resolve")]
    public async Task<ActionResult<Dictionary<string, string>>> ParameterTextResolve([FromBody] RequestDataParameterTextResolveList requestData)
    {
        return await mediator.Send(new QueryParameterTextResolveList(requestData));
    }

    [HttpPost("Parameter/Text/{SchemaIdentifier}/Resolve")]
    public async Task<ActionResult<string>> ParameterTextResolve(string schemaIdentifier)
    {
        return await mediator.Send(new QueryParameterTextResolve(schemaIdentifier));
    }
}
