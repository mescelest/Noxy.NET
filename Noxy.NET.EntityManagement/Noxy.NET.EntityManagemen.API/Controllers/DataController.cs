using MediatR;
using Microsoft.AspNetCore.Mvc;
using Noxy.NET.EntityManagement.API.Commands.Authentication;
using Noxy.NET.EntityManagement.API.Commands.Data;
using Noxy.NET.EntityManagement.API.Queries;
using Noxy.NET.EntityManagement.Domain.Requests.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DataController(IMediator mediator) : ControllerBase
{
    [HttpPost("Parameter/Style")]
    public async Task<ActionResult<ResponseDataParameterStyleCreate>> ParameterStyleCreate([FromBody] RequestDataParameterStyleCreate request)
    {
        return await mediator.Send(new CommandDataParameterStyleCreate(request));
    }

    [HttpPost("Parameter/System")]
    public async Task<ActionResult<ResponseDataParameterSystemCreate>> ParameterSystemCreate([FromBody] RequestDataParameterSystemCreate request)
    {
        return await mediator.Send(new CommandDataParameterSystemCreate(request));
    }

    [HttpPost("Parameter/Text")]
    public async Task<ActionResult<ResponseDataParameterTextCreate>> ParameterTextCreate([FromBody] RequestDataParameterTextCreate request)
    {
        return await mediator.Send(new CommandDataParameterTextCreate(request));
    }

    [HttpGet("Parameter/Text/{identifier}")]
    public async Task<ActionResult<ResponseDataParameterList>> ParameterListWithIdentifier(string identifier)
    {
        return await mediator.Send(new QueryDataParameterList(identifier));
    }

    [HttpPost("Parameter/{id:guid}/Delete")]
    public async Task<ActionResult<ResponseDataParameterDelete>> ParameterWithIDDelete(Guid id)
    {
        return await mediator.Send(new CommandDataParameterDelete(id));
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
