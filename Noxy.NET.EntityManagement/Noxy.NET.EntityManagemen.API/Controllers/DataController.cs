using Mediator;
using Microsoft.AspNetCore.Mvc;
using Noxy.NET.EntityManagement.API.Commands.Data;
using Noxy.NET.EntityManagement.API.Queries.Data;
using Noxy.NET.EntityManagement.Domain.Requests.Data.Parameter;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DataController(IMediator mediator) : ControllerBase
{
    [HttpGet("parameter/{id:guid}")]
    public async Task<ActionResult<ResponseDataParameterFind>> GetParameterByID(Guid id)
    {
        return await mediator.Send(new QueryDataParameterFind(id));
    }

    [HttpGet("parameter/by-identifier/{identifier}")]
    public async Task<ActionResult<ResponseDataParameterList>> GetParameterListWithIdentifier(string identifier, [FromQuery] RequestDataParameterList request)
    {
        return await mediator.Send(new QueryDataParameterList(identifier, request));
    }

    [HttpGet("parameter/{identifier}/count")]
    public async Task<ActionResult<ResponseDataParameterCount>> ParameterCountWithIdentifier(string identifier, [FromQuery] RequestDataParameterCount request)
    {
        return await mediator.Send(new QueryDataParameterCount(identifier, request));
    }

    [HttpPost("parameter/style")]
    public async Task<ActionResult<ResponseDataParameterStyleCreate>> CreateParameterStyle([FromBody] RequestDataParameterStyleCreate request)
    {
        return await mediator.Send(new CommandDataParameterStyleCreate(request));
    }

    [HttpPost("parameter/system")]
    public async Task<ActionResult<ResponseDataParameterSystemCreate>> CreateParameterSystem([FromBody] RequestDataParameterSystemCreate request)
    {
        return await mediator.Send(new CommandDataParameterSystemCreate(request));
    }

    [HttpPost("parameter/text")]
    public async Task<ActionResult<ResponseDataParameterTextCreate>> CreateParameterText([FromBody] RequestDataParameterTextCreate request)
    {
        return await mediator.Send(new CommandDataParameterTextCreate(request));
    }

    [HttpPost("parameter/{id:guid}/approve")]
    public async Task<ActionResult<ResponseDataParameterApprove>> ApproveParameterWithID(Guid id)
    {
        return await mediator.Send(new CommandDataParameterApprove(id));
    }

    [HttpPost("parameter/{id:guid}/delete")]
    public async Task<ActionResult<ResponseDataParameterDelete>> DeleteParameterWithID(Guid id)
    {
        return await mediator.Send(new CommandDataParameterDelete(id));
    }

    [HttpPost("parameter/style/resolve")]
    public async Task<ActionResult<ResponseDataParameterStyleResolveList>> ParameterStyleResolve([FromBody] RequestDataParameterStyleResolveList request)
    {
        return await mediator.Send(new QueryDataParameterStyleResolveList(request));
    }

    [HttpPost("parameter/system/resolve")]
    public async Task<ActionResult<ResponseDataParameterSystemResolveList>> ParameterSystemResolve([FromBody] RequestDataParameterSystemResolveList request)
    {
        return await mediator.Send(new QueryDataParameterSystemResolveList(request));
    }

    [HttpPost("parameter/text/resolve")]
    public async Task<ActionResult<ResponseDataParameterTextResolveList>> ParameterTextResolve([FromBody] RequestDataParameterTextResolveList request)
    {
        return await mediator.Send(new QueryDataParameterTextResolveList(request));
    }

    [HttpPost("parameter/text/{identifier}/resolve")]
    public async Task<ActionResult<ResponseDataParameterTextResolve>> ParameterTextResolve(string identifier)
    {
        return await mediator.Send(new QueryDataParameterTextResolve(identifier));
    }
}
