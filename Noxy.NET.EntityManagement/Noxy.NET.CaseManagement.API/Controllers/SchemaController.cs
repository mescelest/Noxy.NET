using Microsoft.AspNetCore.Mvc;
using Noxy.NET.CaseManagement.Application.Interfaces.Services;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Forms.Schemas.Forms;

namespace Noxy.NET.CaseManagement.API.Controllers;

[ApiController]
[Route("[controller]")]
public class SchemaController(ISchemaService serviceSchema) : ControllerBase
{
    [HttpPost("Action")]
    public async Task<ActionResult<EntitySchemaAction>> Create(FormModelSchemaAction model)
    {
        return await serviceSchema.CreateOrUpdate(model);
    }

    [HttpPost("ActionInput")]
    public async Task<ActionResult<EntitySchemaActionInput>> Create(FormModelSchemaActionInput model)
    {
        return await serviceSchema.CreateOrUpdate(model);
    }

    [HttpPost("ActionStep")]
    public async Task<ActionResult<EntitySchemaActionStep>> Create(FormModelSchemaActionStep model)
    {
        return await serviceSchema.CreateOrUpdate(model);
    }

    [HttpPost("Attribute")]
    public async Task<ActionResult<EntitySchemaAttribute>> Create(FormModelSchemaAttribute model)
    {
        return await serviceSchema.CreateOrUpdate(model);
    }

    [HttpPost("Context")]
    public async Task<ActionResult<EntitySchemaContext>> Create(FormModelSchemaContext model)
    {
        return await serviceSchema.CreateOrUpdate(model);
    }

    [HttpPost("DynamicValue/Code")]
    public async Task<ActionResult<EntitySchemaDynamicValue.Discriminator>> Create(FormModelSchemaDynamicValueCode model)
    {
        return await serviceSchema.CreateOrUpdate(model);
    }

    [HttpPost("DynamicValue/SystemParameter")]
    public async Task<ActionResult<EntitySchemaDynamicValue.Discriminator>> Create(FormModelSchemaDynamicValueSystemParameter model)
    {
        return await serviceSchema.CreateOrUpdate(model);
    }

    [HttpPost("DynamicValue/TextParameter")]
    public async Task<ActionResult<EntitySchemaDynamicValue.Discriminator>> Create(FormModelSchemaDynamicValueTextParameter model)
    {
        return await serviceSchema.CreateOrUpdate(model);
    }
    
    [HttpPost("Element")]
    public async Task<ActionResult<EntitySchemaElement>> Create(FormModelSchemaElement model)
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
