using MediatR;
using Microsoft.AspNetCore.Mvc;
using Noxy.NET.EntityManagement.API.Commands.Schema;
using Noxy.NET.EntityManagement.API.Commands.Schema.Context;
using Noxy.NET.EntityManagement.API.Commands.Schema.Element;
using Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;
using Noxy.NET.EntityManagement.API.Queries.Schema;
using Noxy.NET.EntityManagement.API.Queries.Schema.Context;
using Noxy.NET.EntityManagement.API.Queries.Schema.ContextHasElement;
using Noxy.NET.EntityManagement.API.Queries.Schema.Element;
using Noxy.NET.EntityManagement.API.Queries.Schema.Parameter;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Context;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.ContextHasElement;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Element;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Parameter;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Controllers;

[ApiController]
[Route("[controller]")]
public class SchemaController(IMediator mediator) : ControllerBase
{
    [HttpGet("")]
    public async Task<ActionResult<ResponseSchemaList>> GetSchemaByID([FromQuery] RequestSchemaList request)
    {
        return await mediator.Send(new QuerySchemaList(request));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResponseSchemaFind>> GetSchemaList(Guid id)
    {
        return await mediator.Send(new QuerySchemaFind(id));
    }

    [HttpGet("Context")]
    public async Task<ActionResult<ResponseSchemaContextList>> GetSchemaContextList([FromQuery] RequestSchemaContextList request)
    {
        return await mediator.Send(new QuerySchemaContextList(request));
    }

    [HttpGet("Context/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaContextFind>> GetSchemaContextByID(Guid id)
    {
        return await mediator.Send(new QuerySchemaContextFind(id));
    }

    [HttpGet("Context/Element")]
    public async Task<ActionResult<ResponseSchemaContextHasElementList>> GetSchemaContextHaselementList([FromQuery] RequestSchemaContextHasElementList request)
    {
        return await mediator.Send(new QuerySchemaContextHasElementList(request));
    }

    [HttpGet("Context/Element/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaContextHasElementFind>> GetSchemaContextHasElementByID(Guid id)
    {
        return await mediator.Send(new QuerySchemaContextHasElementFind(id));
    }

    [HttpGet("Element")]
    public async Task<ActionResult<ResponseSchemaElementList>> GetSchemaElementList([FromQuery] RequestSchemaElementList request)
    {
        return await mediator.Send(new QuerySchemaElementList(request));
    }

    [HttpGet("Element/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaElementFind>> GetSchemaElementByID(Guid id)
    {
        return await mediator.Send(new QuerySchemaElementFind(id));
    }

    [HttpGet("Parameter")]
    public async Task<ActionResult<ResponseSchemaParameterList>> GetSchemaParameterList([FromQuery] RequestSchemaParameterList request)
    {
        return await mediator.Send(new QuerySchemaParameterList(request));
    }

    [HttpGet("Parameter/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaParameterFind>> GetSchemaParameterByID(Guid id)
    {
        return await mediator.Send(new QuerySchemaParameterFind(id));
    }

    [HttpPost("")]
    public async Task<ActionResult<ResponseSchemaCreate>> CreateSchema(RequestSchemaCreate request)
    {
        return await mediator.Send(new CommandSchemaCreate(request));
    }

    [HttpPost("{id:guid}")]
    public async Task<ActionResult<ResponseSchemaUpdate>> UpdateSchema(Guid id, RequestSchemaUpdate request)
    {
        return await mediator.Send(new CommandSchemaUpdate(id, request));
    }

    [HttpPost("{id:guid}/Clone")]
    public async Task<ActionResult<ResponseSchemaClone>> CloneSchema(Guid id)
    {
        return await mediator.Send(new CommandSchemaClone(id));
    }

    [HttpPost("{id:guid}/Activate")]
    public async Task<ActionResult<ResponseSchemaActivate>> ActivateSchema(Guid id)
    {
        return await mediator.Send(new CommandSchemaActivate(id));
    }

    [HttpPost("{id:guid}/Delete")]
    public async Task<ActionResult<ResponseSchemaDelete>> DeleteSchema(Guid id)
    {
        return await mediator.Send(new CommandSchemaDelete(id));
    }

    [HttpPost("Context")]
    public async Task<ActionResult<ResponseSchemaContextCreate>> CreateSchemaContext(RequestSchemaContextCreate request)
    {
        return await mediator.Send(new CommandSchemaContextCreate(request));
    }

    [HttpPost("Context/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaContextUpdate>> UpdateSchemaContext(Guid id, RequestSchemaContextUpdate request)
    {
        return await mediator.Send(new CommandSchemaContextUpdate(id, request));
    }

    [HttpPost("Context/{id:guid}/Clone")]
    public async Task<ActionResult<ResponseSchemaContextClone>> CloneSchemaContext(Guid id)
    {
        return await mediator.Send(new CommandSchemaContextClone(id));
    }

    [HttpPost("Context/{id:guid}/Delete")]
    public async Task<ActionResult<ResponseSchemaContextDelete>> DeleteSchemaContext(Guid id)
    {
        return await mediator.Send(new CommandSchemaContextDelete(id));
    }

    [HttpPost("Element")]
    public async Task<ActionResult<ResponseSchemaElementCreate>> CreateSchemaElement(RequestSchemaElementCreate request)
    {
        return await mediator.Send(new CommandSchemaElementCreate(request));
    }

    [HttpPost("Element/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaElementUpdate>> UpdateSchemaElement(Guid id, RequestSchemaElementUpdate request)
    {
        return await mediator.Send(new CommandSchemaElementUpdate(id, request));
    }

    [HttpPost("Element/{id:guid}/Clone")]
    public async Task<ActionResult<ResponseSchemaElementClone>> CloneSchemaElement(Guid id)
    {
        return await mediator.Send(new CommandSchemaElementClone(id));
    }

    [HttpPost("Element/{id:guid}/Delete")]
    public async Task<ActionResult<ResponseSchemaElementDelete>> DeleteSchemaElement(Guid id)
    {
        return await mediator.Send(new CommandSchemaElementDelete(id));
    }

    [HttpPost("Parameter/Style")]
    public async Task<ActionResult<ResponseSchemaParameterStyleCreate>> Create(RequestSchemaParameterStyleCreate model)
    {
        return await mediator.Send(new CommandSchemaParameterStyleCreate(model));
    }

    [HttpPost("Parameter/System")]
    public async Task<ActionResult<ResponseSchemaParameterSystemCreate>> Create(RequestSchemaParameterSystemCreate model)
    {
        return await mediator.Send(new CommandSchemaParameterSystemCreate(model));
    }

    [HttpPost("Parameter/Text")]
    public async Task<ActionResult<ResponseSchemaParameterTextCreate>> Create(RequestSchemaParameterTextCreate model)
    {
        return await mediator.Send(new CommandSchemaParameterTextCreate(model));
    }

    [HttpPost("Parameter/Style/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaParameterStyleUpdate>> Update(Guid id, RequestSchemaParameterStyleUpdate model)
    {
        return await mediator.Send(new CommandSchemaParameterStyleUpdate(id, model));
    }

    [HttpPost("Parameter/System/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaParameterSystemUpdate>> Update(Guid id, RequestSchemaParameterSystemUpdate model)
    {
        return await mediator.Send(new CommandSchemaParameterSystemUpdate(id, model));
    }

    [HttpPost("Parameter/Text/{id:guid}")]
    public async Task<ActionResult<ResponseSchemaParameterTextUpdate>> Update(Guid id, RequestSchemaParameterTextUpdate model)
    {
        return await mediator.Send(new CommandSchemaParameterTextUpdate(id, model));
    }

    [HttpPost("Parameter/{id:guid}/Clone")]
    public async Task<ActionResult<ResponseSchemaParameterClone>> CloneSchemaParameter(Guid id)
    {
        return await mediator.Send(new CommandSchemaParameterClone(id));
    }

    [HttpPost("Parameter/{id:guid}/Delete")]
    public async Task<ActionResult<ResponseSchemaParameterDelete>> DeleteSchemaParameter(Guid id)
    {
        return await mediator.Send(new CommandSchemaParameterDelete(id));
    }
}
