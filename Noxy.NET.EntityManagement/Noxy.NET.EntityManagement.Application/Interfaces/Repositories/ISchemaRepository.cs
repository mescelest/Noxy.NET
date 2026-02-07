using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Persistence.Models;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Repositories;

public interface ISchemaRepository
{
    Task<EntitySchemaContext> GetSchemaContextByID(Guid id);
    Task<EntitySchemaElement> GetSchemaElementByID(Guid id);
    Task<EntitySchemaParameter.Discriminator> GetSchemaParameterByID(Guid id);
    Task<EntitySchemaParameterStyle> GetSchemaParameterStyleByID(Guid id);
    Task<EntitySchemaParameterSystem> GetSchemaParameterSystemByID(Guid id);
    Task<EntitySchemaParameterText> GetSchemaParameterTextByID(Guid id);
    Task<EntitySchemaProperty.Discriminator> GetSchemaPropertyByID(Guid id);
    Task<EntitySchemaPropertyBoolean> GetSchemaPropertyBooleanByID(Guid id);
    Task<EntitySchemaPropertyCollection> GetSchemaPropertyCollectionByID(Guid id);
    Task<EntitySchemaPropertyDateTime> GetSchemaPropertyDateTimeByID(Guid id);
    Task<EntitySchemaPropertyDecimal> GetSchemaPropertyDecimalByID(Guid id);
    Task<EntitySchemaPropertyImage> GetSchemaPropertyImageByID(Guid id);
    Task<EntitySchemaPropertyInteger> GetSchemaPropertyIntegerByID(Guid id);
    Task<EntitySchemaPropertyString> GetSchemaPropertyStringByID(Guid id);
    Task<EntitySchemaPropertyTable> GetSchemaPropertyTableByID(Guid id);

    Task<List<EntitySchemaContext>> GetSchemaContextListBySchemaID(Guid id);
    Task<List<EntitySchemaElement>> GetSchemaElementListBySchemaID(Guid id);
    Task<List<EntitySchemaParameter.Discriminator>> GetSchemaParameterList(FilterSchemaParameterList filter);
    Task<List<EntitySchemaParameter.Discriminator>> GetSchemaParameterListBySchemaID(Guid id);
    Task<List<EntitySchemaProperty.Discriminator>> GetSchemaPropertyListBySchemaID(Guid id);

    Task<List<EntityJunctionSchemaContextHasElement>> GetSchemaContextHasElementListBySchemaID(Guid id);
    Task<List<EntityJunctionSchemaElementHasProperty>> GetSchemaElementHasPropertyListBySchemaID(Guid id);

    Task<EntitySchemaContext> Create(EntitySchemaContext entity);
    Task<EntitySchemaParameter.Discriminator> Create(EntitySchemaParameter entity);
    Task<EntitySchemaElement> Create(EntitySchemaElement entity);
    Task<EntitySchemaProperty.Discriminator> Create(EntitySchemaProperty entity);

    void Update(EntitySchemaContext entity);
    void Update(EntitySchemaParameter entity);
    void Update(EntitySchemaElement entity);
    void Update(EntitySchemaProperty baseEntity);
}
