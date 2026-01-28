using Microsoft.AspNetCore.Mvc;
using Noxy.NET.CaseManagement.Application.Interfaces.Services;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Forms.Schemas.AssociationForms;
using Noxy.NET.CaseManagement.Domain.ViewModels;

namespace Noxy.NET.CaseManagement.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AssociationController(IAssociationService serviceAssociation) : ControllerBase
{
    [HttpPost("Schema/ActionInput/Attribute/String")]
    public async Task<ActionResult<List<EntityAssociationSchemaActionInputHasAttribute>>> RelateActionWithActionStepList(FormModelAssociationSchemaActionInputHasAttribute<string> model)
    {
        return await serviceAssociation.Associate(model);
    }

    [HttpPost("Schema/ActionInput/Attribute/Integer")]
    public async Task<ActionResult<List<EntityAssociationSchemaActionInputHasAttribute>>> RelateActionWithActionStepList(FormModelAssociationSchemaActionInputHasAttribute<int?> model)
    {
        return await serviceAssociation.Associate(model);
    }

    [HttpPost("Schema/ActionInput/Attribute/DateTime")]
    public async Task<ActionResult<List<EntityAssociationSchemaActionInputHasAttribute>>> RelateActionWithActionStepList(FormModelAssociationSchemaActionInputHasAttribute<DateTime?> model)
    {
        return await serviceAssociation.Associate(model);
    }

    [HttpPost("Schema/ActionInput/Attribute/DynamicValue")]
    public async Task<ActionResult<List<EntityAssociationSchemaActionInputHasAttribute>>> RelateActionWithActionStepList(FormModelAssociationSchemaActionInputHasAttribute<ViewModelSchemaDynamicValue?> model)
    {
        return await serviceAssociation.Associate(model);
    }
}
