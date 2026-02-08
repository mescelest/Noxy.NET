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
    [HttpGet("Parameter/Text/{identifier}")]
    public async Task<ActionResult<ResponseDataParameterList>> ParameterListWithIdentifier(string identifier)
    {
        return await mediator.Send(new QueryDataParameterList(identifier));
    }

    [HttpPost("Parameter/Text/Resolve")]
    public async Task<ActionResult<ResponseDataParameterResolveList>> ParameterTextResolve([FromBody] RequestDataParameterTextResolveList request)
    {
        return await mediator.Send(new QueryDataParameterTextResolveList(request));
    }

    [HttpPost("Parameter/Text/{identifier}/Resolve")]
    public async Task<ActionResult<ResponseDataParameterTextResolve>> ParameterTextResolve(string identifier)
    {
        return await mediator.Send(new QueryDataParameterTextResolve(identifier));
    }
}
