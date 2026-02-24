using MediatR;
using Microsoft.AspNetCore.Mvc;
using Noxy.NET.EntityManagement.API.Commands.Schema;
using Noxy.NET.EntityManagement.API.Queries;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Controllers;

[ApiController]
[Route("[controller]")]
public class SchemaController(IMediator mediator) : ControllerBase
{
    [HttpGet("Parameter")]
    public async Task<ActionResult<ResponseSchemaParameterList>> GetParameterList([FromQuery] RequestSchemaParameterList request)
    {
        return await mediator.Send(new QuerySchemaParameterList(request));
    }

    [HttpPost("")]
    public async Task<ActionResult<ResponseSchemaCreate>> CreateSchema(RequestSchemaCreate request)
    {
        return await mediator.Send(new CommandSchemaCreate(request));
    }

    [HttpPost("Context")]
    public async Task<ActionResult<ResponseSchemaContextCreate>> Create(RequestSchemaContextCreate request)
    {
        return await mediator.Send(new CommandSchemaContextCreate(request));
    }

    [HttpPost("Element")]
    public async Task<ActionResult<ResponseSchemaElementCreate>> Create(RequestSchemaElementCreate request)
    {
        return await mediator.Send(new CommandSchemaElementCreate(request));
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
