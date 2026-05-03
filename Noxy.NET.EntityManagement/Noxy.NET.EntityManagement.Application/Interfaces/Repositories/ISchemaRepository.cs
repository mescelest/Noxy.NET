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

    #region -- SchemaContext --

    Task<EntitySchemaContext> GetSchemaContextByID(Guid id);
    Task<List<EntitySchemaContext>> GetSchemaContextList(FilterSchemaContextList filter);
    Task<EntitySchemaContext> CreateSchemaContext(EntitySchemaContext entity);
    Task<EntitySchemaContext> UpdateSchemaContext(EntitySchemaContext entity);
    Task<EntitySchemaContext> CloneSchemaContext(Guid id);
    Task<Guid> DeleteSchemaContext(Guid id);

    #endregion -- SchemaContext --

    #region -- SchemaContextHasElement --

    Task<EntitySchemaContextHasElement> GetSchemaContextHasElementByID(Guid id);
    Task<List<EntitySchemaContextHasElement>> GetSchemaContextHasElementList(FilterSchemaContextHasElementList filter);
    Task<EntitySchemaContextHasElement> CreateSchemaContextHasElement(EntitySchemaContextHasElement entity);
    Task<Guid> DeleteSchemaContextHasElement(Guid id);

    #endregion -- SchemaContextHasElement --

    #region -- SchemaElement --

    Task<EntitySchemaElement> GetSchemaElementByID(Guid id);
    Task<List<EntitySchemaElement>> GetSchemaElementList(FilterSchemaElementList filter);
    Task<EntitySchemaElement> CreateSchemaElement(EntitySchemaElement entity);
    Task<EntitySchemaElement> UpdateSchemaElement(EntitySchemaElement entity);
    Task<EntitySchemaElement> CloneSchemaElement(Guid id);
    Task<Guid> DeleteSchemaElement(Guid id);

    #endregion -- SchemaElement --

    #region -- SchemaElementHasProperty --

    Task<EntitySchemaElementHasProperty> GetSchemaElementHasPropertyByID(Guid id);
    Task<List<EntitySchemaElementHasProperty>> GetSchemaElementHasPropertyList(FilterSchemaElementHasPropertyList filter);
    Task<EntitySchemaElementHasProperty> CreateSchemaElementHasProperty(EntitySchemaElementHasProperty entity);
    Task<Guid> DeleteSchemaElementHasProperty(Guid id);

    #endregion -- SchemaElementHasProperty --

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

    #region -- SchemaProperty --

    Task<EntitySchemaProperty.Discriminator> GetSchemaPropertyByID(Guid id);
    Task<List<EntitySchemaProperty.Discriminator>> GetSchemaPropertyList(FilterSchemaPropertyList filter);
    Task<EntitySchemaPropertyBoolean> CreateSchemaPropertyBoolean(EntitySchemaPropertyBoolean entity);
    Task<EntitySchemaPropertyDateTime> CreateSchemaPropertyDateTime(EntitySchemaPropertyDateTime entity);
    Task<EntitySchemaPropertyDecimal> CreateSchemaPropertyDecimal(EntitySchemaPropertyDecimal entity);
    Task<EntitySchemaPropertyImage> CreateSchemaPropertyImage(EntitySchemaPropertyImage entity);
    Task<EntitySchemaPropertyInteger> CreateSchemaPropertyInteger(EntitySchemaPropertyInteger entity);
    Task<EntitySchemaPropertyString> CreateSchemaPropertyString(EntitySchemaPropertyString entity);
    Task<EntitySchemaPropertyBoolean> UpdateSchemaPropertyBoolean(EntitySchemaPropertyBoolean entity);
    Task<EntitySchemaPropertyDateTime> UpdateSchemaPropertyDateTime(EntitySchemaPropertyDateTime entity);
    Task<EntitySchemaPropertyDecimal> UpdateSchemaPropertyDecimal(EntitySchemaPropertyDecimal entity);
    Task<EntitySchemaPropertyImage> UpdateSchemaPropertyImage(EntitySchemaPropertyImage entity);
    Task<EntitySchemaPropertyInteger> UpdateSchemaPropertyInteger(EntitySchemaPropertyInteger entity);
    Task<EntitySchemaPropertyString> UpdateSchemaPropertyString(EntitySchemaPropertyString entity);
    Task<EntitySchemaProperty.Discriminator> CloneSchemaProperty(Guid id);
    Task<Guid> DeleteSchemaProperty(Guid id);

    #endregion -- SchemaProperty --

    Task<List<EntitySchemaContext>> GetSchemaContextListBySchemaID(Guid id);
    Task<List<EntitySchemaElement>> GetSchemaElementListBySchemaID(Guid id);
    Task<List<EntitySchemaParameter.Discriminator>> GetSchemaParameterListBySchemaID(Guid id);
    Task<List<EntitySchemaProperty.Discriminator>> GetSchemaPropertyListBySchemaID(Guid id);

    Task<List<EntitySchemaContextHasElement>> GetSchemaContextHasElementListBySchemaID(Guid id);
    Task<List<EntitySchemaElementHasProperty>> GetSchemaElementHasPropertyListBySchemaID(Guid id);
}
