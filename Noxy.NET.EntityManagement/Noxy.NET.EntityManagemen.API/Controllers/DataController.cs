using MediatR;
using Microsoft.AspNetCore.Mvc;
using Noxy.NET.EntityManagement.API.Queries;
using Noxy.NET.EntityManagement.Domain.Requests;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DataController(IMediator mediator) : ControllerBase
{
    [HttpPost("Parameter/Text/Resolve")]
    public async Task<ActionResult<ResponseDataParameterResolveList>> ParameterTextResolve([FromBody] RequestDataParameterTextResolveList requestData)
    {
        return await mediator.Send(new QueryDataParameterTextResolveList(requestData));
    }

    [HttpPost("Parameter/Text/{SchemaIdentifier}/Resolve")]
    public async Task<ActionResult<ResponseDataParameterResolve>> ParameterTextResolve(string schemaIdentifier)
    {
        return await mediator.Send(new QueryDataParameterTextResolve(schemaIdentifier));
    }
}
