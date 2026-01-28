using Microsoft.AspNetCore.Mvc;
using Noxy.NET.CaseManagement.Application.Interfaces.Services;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Forms.Schemas.Forms;

namespace Noxy.NET.CaseManagement.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TemplateController(ITemplateService serviceTemplate) : ControllerBase
{
    [HttpGet("Schema")]
    public async Task<ActionResult<List<EntitySchema>>> GetSchemaList()
    {
        return await serviceTemplate.GetSchemaList();
    }

    [HttpGet("Schema/{id:guid}")]
    public async Task<ActionResult<EntitySchema>> GetSchemaWithID(Guid id)
    {
        return await serviceTemplate.GetSchemaWithID(id);
    }

    [HttpPost("Schema")]
    public async Task<ActionResult<EntitySchema>> CreateSchema(FormModelSchema model)
    {
        return await serviceTemplate.CreateOrUpdate(model);
    }

    [HttpPut("Schema")]
    public async Task<ActionResult<EntitySchema>> UpdateSchema(FormModelSchema model)
    {
        return await serviceTemplate.CreateOrUpdate(model);
    }
    
    [HttpPost("Schema/{id:guid}/Activate")]
    public async Task<ActionResult> ActivateSchema(Guid id)
    {
        await serviceTemplate.ActivateSchema(id);
        return Ok();
    }
}
