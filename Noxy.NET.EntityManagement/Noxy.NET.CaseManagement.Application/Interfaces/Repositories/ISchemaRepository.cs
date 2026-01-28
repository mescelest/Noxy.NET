using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Application.Interfaces.Repositories;

public interface ISchemaRepository
{
    Task<EntitySchemaAction> GetSchemaActionByID(Guid id);
    Task<EntitySchemaActionInput> GetSchemaActionInputByID(Guid id);
    Task<EntitySchemaActionStep> GetSchemaActionStepByID(Guid id);
    Task<EntitySchemaAttribute> GetSchemaAttributeByID(Guid id);
    Task<EntitySchemaContext> GetSchemaContextByID(Guid id);
    Task<EntitySchemaDynamicValue.Discriminator> GetSchemaDynamicValueByID(Guid id);
    Task<EntitySchemaElement> GetSchemaElementByID(Guid id);
    Task<EntitySchemaProperty.Discriminator> GetSchemaPropertyByID(Guid id);
    Task<EntitySchemaInput> GetSchemaInputByID(Guid id);

    Task<List<EntitySchemaAction>> GetSchemaActionListBySchemaID(Guid id);
    Task<List<EntitySchemaActionInput>> GetSchemaActionInputListBySchemaID(Guid id);
    Task<List<EntitySchemaActionStep>> GetSchemaActionStepListBySchemaID(Guid id);
    Task<List<EntitySchemaAttribute>> GetSchemaAttributeListBySchemaID(Guid id);
    Task<List<EntitySchemaContext>> GetSchemaContextListBySchemaID(Guid id);
    Task<List<EntitySchemaDynamicValue.Discriminator>> GetSchemaDynamicValueListBySchemaID(Guid id);
    Task<List<EntitySchemaElement>> GetSchemaElementListBySchemaID(Guid id);
    Task<List<EntitySchemaProperty.Discriminator>> GetSchemaPropertyListBySchemaID(Guid id);
    Task<List<EntitySchemaInput>> GetSchemaInputListBySchemaID(Guid id);

    Task<List<EntityJunctionSchemaActionHasActionStep>> GetSchemaActionHasActionStepListBySchemaID(Guid id);
    Task<List<EntityJunctionSchemaActionHasDynamicValueCode>> GetSchemaActionHasDynamicValueCodeListBySchemaID(Guid id);
    Task<List<EntityAssociationSchemaActionInputHasAttribute.Discriminator>> GetSchemaActionInputHasAttributeListBySchemaID(Guid id);
    Task<List<EntityJunctionSchemaActionStepHasActionInput>> GetSchemaActionStepHasActionInputListBySchemaID(Guid id);
    Task<List<EntityJunctionSchemaContextHasAction>> GetSchemaContextHasActionListBySchemaID(Guid id);
    Task<List<EntityJunctionSchemaContextHasElement>> GetSchemaContextHasElementListBySchemaID(Guid id);
    Task<List<EntityJunctionSchemaElementHasAction>> GetSchemaElementHasActionListBySchemaID(Guid id);
    Task<List<EntityJunctionSchemaElementHasProperty>> GetSchemaElementHasPropertyListBySchemaID(Guid id);
    Task<List<EntityJunctionSchemaInputHasAttribute>> GetSchemaInputHasAttributeListBySchemaID(Guid id);

    Task<EntitySchemaAction> Create(EntitySchemaAction entity);
    Task<EntitySchemaActionInput> Create(EntitySchemaActionInput entity);
    Task<EntitySchemaActionStep> Create(EntitySchemaActionStep entity);
    Task<EntitySchemaAttribute> Create(EntitySchemaAttribute entity);
    Task<EntitySchemaContext> Create(EntitySchemaContext entity);
    Task<EntitySchemaDynamicValue.Discriminator> Create(EntitySchemaDynamicValue entity);
    Task<EntitySchemaElement> Create(EntitySchemaElement entity);
    Task<EntitySchemaInput> Create(EntitySchemaInput entity);
    Task<EntitySchemaProperty.Discriminator> Create(EntitySchemaProperty entity);

    void Update(EntitySchemaAction entity);
    void Update(EntitySchemaActionInput entity);
    void Update(EntitySchemaActionStep entity);
    void Update(EntitySchemaAttribute entity);
    void Update(EntitySchemaContext entity);
    void Update(EntitySchemaDynamicValue entity);
    void Update(EntitySchemaElement entity);
    void Update(EntitySchemaInput entity);
    void Update(EntitySchemaProperty baseEntity);
}
