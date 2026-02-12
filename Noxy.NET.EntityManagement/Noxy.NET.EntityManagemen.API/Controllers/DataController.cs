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
    [HttpPost("Parameter/Style")]
    public async Task<ActionResult<ResponseDataParameterStyleCreate>> ParameterStyleCreate([FromBody] RequestDataParameterStyleCreate request)
    {
        return await mediator.Send(new QueryDataParameterStyleCreate(request));
    }

    [HttpPost("Parameter/System")]
    public async Task<ActionResult<ResponseDataParameterSystemCreate>> ParameterSystemCreate([FromBody] RequestDataParameterSystemCreate request)
    {
        return await mediator.Send(new QueryDataParameterSystemCreate(request));
    }

    [HttpPost("Parameter/Text")]
    public async Task<ActionResult<ResponseDataParameterTextCreate>> ParameterTextCreate([FromBody] RequestDataParameterTextCreate request)
    {
        return await mediator.Send(new QueryDataParameterTextCreate(request));
    }

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
