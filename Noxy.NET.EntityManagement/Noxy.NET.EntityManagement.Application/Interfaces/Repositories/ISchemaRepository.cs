using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Repositories;

public interface ISchemaRepository
{
    Task<EntitySchemaContext> GetSchemaContextByID(Guid id);
    Task<EntitySchemaDynamicValue.Discriminator> GetSchemaDynamicValueByID(Guid id);
    Task<EntitySchemaElement> GetSchemaElementByID(Guid id);
    Task<EntitySchemaProperty.Discriminator> GetSchemaPropertyByID(Guid id);

    Task<List<EntitySchemaContext>> GetSchemaContextListBySchemaID(Guid id);
    Task<List<EntitySchemaDynamicValue.Discriminator>> GetSchemaDynamicValueListBySchemaID(Guid id);
    Task<List<EntitySchemaElement>> GetSchemaElementListBySchemaID(Guid id);
    Task<List<EntitySchemaProperty.Discriminator>> GetSchemaPropertyListBySchemaID(Guid id);

    Task<List<EntityJunctionSchemaContextHasElement>> GetSchemaContextHasElementListBySchemaID(Guid id);
    Task<List<EntityJunctionSchemaElementHasProperty>> GetSchemaElementHasPropertyListBySchemaID(Guid id);

    Task<EntitySchemaContext> Create(EntitySchemaContext entity);
    Task<EntitySchemaDynamicValue.Discriminator> Create(EntitySchemaDynamicValue entity);
    Task<EntitySchemaElement> Create(EntitySchemaElement entity);
    Task<EntitySchemaProperty.Discriminator> Create(EntitySchemaProperty entity);

    void Update(EntitySchemaContext entity);
    void Update(EntitySchemaDynamicValue entity);
    void Update(EntitySchemaElement entity);
    void Update(EntitySchemaProperty baseEntity);
}
