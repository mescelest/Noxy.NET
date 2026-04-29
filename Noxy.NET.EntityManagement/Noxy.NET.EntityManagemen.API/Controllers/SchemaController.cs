using MediatR;
using Microsoft.AspNetCore.Mvc;
using Noxy.NET.EntityManagement.API.Commands.Schema;
using Noxy.NET.EntityManagement.API.Commands.Schema.Context;
using Noxy.NET.EntityManagement.API.Commands.Schema.Element;
using Noxy.NET.EntityManagement.API.Queries.Schema;
using Noxy.NET.EntityManagement.API.Queries.Schema.Context;
using Noxy.NET.EntityManagement.API.Queries.Schema.Element;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Context;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Element;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

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

    [HttpGet("Element")]
    public async Task<ActionResult<ResponseSchemaElementList>> GetSchemaParameterList([FromQuery] RequestSchemaElementList request)
    {
        return await mediator.Send(new QuerySchemaElementList(request));
    }

    [HttpGet("Parameter")]
    public async Task<ActionResult<ResponseSchemaParameterList>> GetSchemaParameterList([FromQuery] RequestSchemaParameterList request)
    {
        return await mediator.Send(new QuerySchemaParameterList(request));
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

    // [HttpPost("Parameter/Style")]
    // public async Task<ActionResult<EntitySchemaParameter.Discriminator>> Create(FormModelSchemaParameterStyle model)
    // {
    //     return await serviceSchema.CreateOrUpdate(model);
    // }
    //
    // [HttpPost("Parameter/System")]
    // public async Task<ActionResult<EntitySchemaParameter.Discriminator>> Create(FormModelSchemaParameterSystem model)
    // {
    //     return await serviceSchema.CreateOrUpdate(model);
    // }
    //
    // [HttpPost("Parameter/Text")]
    // public async Task<ActionResult<EntitySchemaParameter.Discriminator>> Create(FormModelSchemaParameterText model)
    // {
    //     return await serviceSchema.CreateOrUpdate(model);
    // }
    //
    // [HttpPost("Property/Boolean")]
    // public async Task<ActionResult<EntitySchemaPropertyBoolean>> Create(FormModelSchemaPropertyBoolean model)
    // {
    //     return (await serviceSchema.CreateOrUpdate(model)).Boolean ?? throw new();
    // }
    //
    // [HttpPost("Property/DateTime")]
    // public async Task<ActionResult<EntitySchemaPropertyDateTime>> CreateOrUpdate(FormModelSchemaPropertyDateTime model)
    // {
    //     return (await serviceSchema.CreateOrUpdate(model)).DateTime ?? throw new();
    // }
    //
    // [HttpPost("Property/Image")]
    // public async Task<ActionResult<EntitySchemaPropertyImage>> CreateOrUpdate(FormModelSchemaPropertyImage model)
    // {
    //     return (await serviceSchema.CreateOrUpdate(model)).Image ?? throw new();
    // }
    //
    // [HttpPost("Property/Decimal")]
    // public async Task<ActionResult<EntitySchemaPropertyDecimal>> CreateOrUpdate(FormModelSchemaPropertyDecimal model)
    // {
    //     return (await serviceSchema.CreateOrUpdate(model)).Decimal ?? throw new();
    // }
    //
    // [HttpPost("Property/Integer")]
    // public async Task<ActionResult<EntitySchemaPropertyInteger>> CreateOrUpdate(FormModelSchemaPropertyInteger model)
    // {
    //     return (await serviceSchema.CreateOrUpdate(model)).Integer ?? throw new();
    // }
    //
    // [HttpPost("Property/String")]
    // public async Task<ActionResult<EntitySchemaPropertyString>> Create(FormModelSchemaPropertyString model)
    // {
    //     return (await serviceSchema.CreateOrUpdate(model)).String ?? throw new();
    // }
}
