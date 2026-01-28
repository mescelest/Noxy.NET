using Noxy.NET.CaseManagement.Domain.Entities.Data;
using Noxy.NET.CaseManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.CaseManagement.Domain.ViewModels;

namespace Noxy.NET.CaseManagement.Application.Interfaces.Services;

public interface IViewModelFactoryService
{
    ViewModelSchemaAction Create(EntitySchemaAction? entity);
    ViewModelSchemaActionHasActionStep Create(EntityJunctionSchemaActionHasActionStep? entity);
    ViewModelSchemaActionInput Create(EntitySchemaActionInput? entity);
    ViewModelSchemaActionInputHasAttribute Create(EntityAssociationSchemaActionInputHasAttribute? entity);
    ViewModelSchemaActionStep Create(EntitySchemaActionStep? entity);
    ViewModelSchemaActionStepHasActionInput Create(EntityJunctionSchemaActionStepHasActionInput? entity);
    ViewModelSchemaContext Create(EntitySchemaContext? entity);
    ViewModelSchemaDynamicValue Create(EntitySchemaDynamicValue? entity);
    ViewModelSchemaDynamicValue Create(EntitySchemaDynamicValue.Discriminator? entity);
    ViewModelSchemaInput Create(EntitySchemaInput? entity);
    ViewModelSchemaProperty Create(EntitySchemaProperty? entity);
    ViewModelDataElement Create(EntityDataElement? entity);
    ViewModelDataProperty Create(EntityDataProperty? entity);
    ViewModelDataProperty Create(EntityDataProperty.Discriminator? entity);
}
