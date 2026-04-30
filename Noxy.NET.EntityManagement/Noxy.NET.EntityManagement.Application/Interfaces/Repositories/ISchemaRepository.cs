using Noxy.NET.EntityManagement.Application.Models;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Repositories;

public interface ISchemaRepository
{
    Task<Guid> GetCurrentSchemaID();
    Task<EntitySchema> GetCurrentSchema();

    #region -- Schema --

    Task<EntitySchema> GetSchemaByID(Guid id);
    Task<List<EntitySchema>> GetSchemaList(FilterSchemaList filter);
    Task<EntitySchema> CreateSchema(EntitySchema entity);
    Task<EntitySchema> UpdateSchema(EntitySchema entity);
    Task<EntitySchema> CloneSchema(Guid id);
    Task<Guid> DeleteSchema(Guid id);
    Task<EntitySchema> ActivateSchema(Guid id);

    #endregion -- Schema --

    #region -- SchemaElement --

    Task<EntitySchemaElement> GetSchemaElementByID(Guid id);
    Task<List<EntitySchemaElement>> GetSchemaElementList(FilterSchemaElementList filter);
    Task<EntitySchemaElement> CreateSchemaElement(EntitySchemaElement entity);
    Task<EntitySchemaElement> UpdateSchemaElement(EntitySchemaElement entity);
    Task<EntitySchemaElement> CloneSchemaElement(Guid id);
    Task<Guid> DeleteSchemaElement(Guid id);

    #endregion -- SchemaElement --

    #region -- SchemaContext --

    Task<EntitySchemaContext> GetSchemaContextByID(Guid id);
    Task<List<EntitySchemaContext>> GetSchemaContextList(FilterSchemaContextList filter);
    Task<EntitySchemaContext> CreateSchemaContext(EntitySchemaContext entity);
    Task<EntitySchemaContext> UpdateSchemaContext(EntitySchemaContext entity);
    Task<EntitySchemaContext> CloneSchemaContext(Guid id);
    Task<Guid> DeleteSchemaContext(Guid id);

    #endregion -- SchemaContext --

    #region -- SchemaParameter --

    Task<EntitySchemaParameter.Discriminator> GetSchemaParameterByID(Guid id);
    Task<List<EntitySchemaParameter.Discriminator>> GetSchemaParameterList(FilterSchemaParameterList filter);
    Task<EntitySchemaParameterStyle> CreateSchemaParameterStyle(EntitySchemaParameterStyle entity);
    Task<EntitySchemaParameterSystem> CreateSchemaParameterSystem(EntitySchemaParameterSystem entity);
    Task<EntitySchemaParameterText> CreateSchemaParameterText(EntitySchemaParameterText entity);
    Task<EntitySchemaParameterStyle> UpdateSchemaParameterStyle(EntitySchemaParameterStyle entity);
    Task<EntitySchemaParameterSystem> UpdateSchemaParameterSystem(EntitySchemaParameterSystem entity);
    Task<EntitySchemaParameterText> UpdateSchemaParameterText(EntitySchemaParameterText entity);
    Task<EntitySchemaParameter.Discriminator> CloneSchemaParameter(Guid id);
    Task<Guid> DeleteSchemaParameter(Guid id);

    #endregion -- SchemaParameter --


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
    Task<List<EntitySchemaParameter.Discriminator>> GetSchemaParameterListBySchemaID(Guid id);
    Task<List<EntitySchemaProperty.Discriminator>> GetSchemaPropertyListBySchemaID(Guid id);

    Task<List<EntityJunctionSchemaContextHasElement>> GetSchemaContextHasElementListBySchemaID(Guid id);
    Task<List<EntityJunctionSchemaElementHasProperty>> GetSchemaElementHasPropertyListBySchemaID(Guid id);
}
