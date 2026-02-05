using Microsoft.AspNetCore.Mvc;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas;

namespace Noxy.NET.EntityManagement.API.Controllers;

[ApiController]
[Route("[controller]")]
public class SchemaController(ISchemaService serviceSchema) : ControllerBase
{
    [HttpPost("Context")]
    public async Task<ActionResult<EntitySchemaContext>> Create(FormModelSchemaContext model)
    {
        return await serviceSchema.CreateOrUpdate(model);
    }

    [HttpPost("Element")]
    public async Task<ActionResult<EntitySchemaElement>> Create(FormModelSchemaElement model)
    {
        return await serviceSchema.CreateOrUpdate(model);
    }

    [HttpGet("Parameter")]
    public async Task<ActionResult<List<EntitySchemaParameter.Discriminator>>> Create([FromQuery] FormModelSchemaParameterList model)
    {
        return await serviceSchema.GetSchemaParameterList(model);
    }

    [HttpPost("Parameter/Style")]
    public async Task<ActionResult<EntitySchemaParameter.Discriminator>> Create(FormModelSchemaParameterStyle model)
    {
        return await serviceSchema.CreateOrUpdate(model);
    }

    [HttpPost("Parameter/System")]
    public async Task<ActionResult<EntitySchemaParameter.Discriminator>> Create(FormModelSchemaParameterSystem model)
    {
        return await serviceSchema.CreateOrUpdate(model);
    }

    [HttpPost("Parameter/Text")]
    public async Task<ActionResult<EntitySchemaParameter.Discriminator>> Create(FormModelSchemaParameterText model)
    {
        return await serviceSchema.CreateOrUpdate(model);
    }

    [HttpPost("Property/Boolean")]
    public async Task<ActionResult<EntitySchemaPropertyBoolean>> Create(FormModelSchemaPropertyBoolean model)
    {
        return (await serviceSchema.CreateOrUpdate(model)).Boolean ?? throw new();
    }

    [HttpPost("Property/DateTime")]
    public async Task<ActionResult<EntitySchemaPropertyDateTime>> CreateOrUpdate(FormModelSchemaPropertyDateTime model)
    {
        return (await serviceSchema.CreateOrUpdate(model)).DateTime ?? throw new();
    }

    [HttpPost("Property/Image")]
    public async Task<ActionResult<EntitySchemaPropertyImage>> CreateOrUpdate(FormModelSchemaPropertyImage model)
    {
        return (await serviceSchema.CreateOrUpdate(model)).Image ?? throw new();
    }

    [HttpPost("Property/Decimal")]
    public async Task<ActionResult<EntitySchemaPropertyDecimal>> CreateOrUpdate(FormModelSchemaPropertyDecimal model)
    {
        return (await serviceSchema.CreateOrUpdate(model)).Decimal ?? throw new();
    }

    [HttpPost("Property/Integer")]
    public async Task<ActionResult<EntitySchemaPropertyInteger>> CreateOrUpdate(FormModelSchemaPropertyInteger model)
    {
        return (await serviceSchema.CreateOrUpdate(model)).Integer ?? throw new();
    }

    [HttpPost("Property/String")]
    public async Task<ActionResult<EntitySchemaPropertyString>> Create(FormModelSchemaPropertyString model)
    {
        return (await serviceSchema.CreateOrUpdate(model)).String ?? throw new();
    }
}
