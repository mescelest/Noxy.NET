using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Application.Interfaces.Repositories;

public interface IJunctionRepository
{
    Task ClearSchemaActionHasActionStepByEntityID(Guid id);
    Task<EntityJunctionSchemaActionStepHasActionInput> Create(EntityJunctionSchemaActionStepHasActionInput entity);

    Task ClearSchemaActionStepHasActionInputByEntityID(Guid id);
    Task<EntityJunctionSchemaActionHasActionStep> Create(EntityJunctionSchemaActionHasActionStep entity);
    
    Task ClearSchemaContextHasActionByEntityID(Guid id);
    Task<EntityJunctionSchemaContextHasAction> Create(EntityJunctionSchemaContextHasAction entity);

    Task ClearSchemaContextHasElementByEntityID(Guid id);
    Task<EntityJunctionSchemaContextHasElement> Create(EntityJunctionSchemaContextHasElement entity);

    Task ClearSchemaElementHasPropertyByEntityID(Guid id);
    Task<EntityJunctionSchemaElementHasProperty> Create(EntityJunctionSchemaElementHasProperty entity);
    
    Task<List<EntityJunctionSchemaElementHasProperty>> RelateElementToPropertyList(Guid entityGuid, IEnumerable<Guid> listGuid);
    Task<List<EntityJunctionSchemaActionHasActionStep>> RelateActionToActionStepList(Guid entityGuid, IEnumerable<Guid> listGuid);
    Task<List<EntityJunctionSchemaActionHasDynamicValueCode>> RelateActionToDynamicValueCode(Guid entityGuid, IEnumerable<Guid> listGuid);
    Task<List<EntityJunctionSchemaActionStepHasActionInput>> RelateActionStepToActionInputList(Guid entityGuid, IEnumerable<Guid> listGuid);
    Task<List<EntityJunctionSchemaContextHasAction>> RelateContextToAction(Guid entityGuid, IEnumerable<Guid> listGuid);
    Task<List<EntityJunctionSchemaContextHasElement>> RelateContextToElement(Guid entityGuid, IEnumerable<Guid> listGuid);
}
