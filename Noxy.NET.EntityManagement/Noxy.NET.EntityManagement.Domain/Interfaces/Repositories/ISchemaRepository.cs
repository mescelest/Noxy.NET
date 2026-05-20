using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Models.Filters;

namespace Noxy.NET.EntityManagement.Domain.Interfaces.Repositories;

public interface ISchemaRepository
{
    Task<Guid> GetCurrentSchemaID();
    Task<EntitySchema> GetCurrentSchema();

    #region -- Schema --

    Task<EntitySchema> GetSchemaByID(Guid id);
    Task<List<EntitySchema>> GetSchemaList(FilterSchemaList filter);
    Task<int> GetSchemaCount(FilterSchemaCount filter);
    Task<EntitySchema> CreateSchema(EntitySchema entity);
    void UpdateSchema(EntitySchema entity);
    Task<EntitySchema> CloneSchema(EntitySchema entity);
    void DeleteSchema(EntitySchema entity);
    Task DeactivateSchemaExcept(Guid id);

    #endregion -- Schema --

    #region -- SchemaContext --

    Task<EntitySchemaContext> GetSchemaContextByID(Guid id);
    Task<List<EntitySchemaContext>> GetSchemaContextList(FilterSchemaContextList filter);
    Task<int> GetSchemaContextCount(FilterSchemaContextCount filter);
    Task<EntitySchemaContext> CreateSchemaContext(EntitySchemaContext entity);
    void UpdateSchemaContext(EntitySchemaContext entity);
    void DeleteSchemaContext(EntitySchemaContext entity);

    #endregion -- SchemaContext --

    #region -- SchemaContextHasElement --

    Task<EntitySchemaContextHasElement> GetSchemaContextHasElementByID(Guid id);
    Task<List<EntitySchemaContextHasElement>> GetSchemaContextHasElementList(FilterSchemaContextHasElementList filter);
    Task<EntitySchemaContextHasElement> CreateSchemaContextHasElement(EntitySchemaContextHasElement entity);
    void DeleteSchemaContextHasElement(EntitySchemaContextHasElement entity);

    #endregion -- SchemaContextHasElement --

    #region -- SchemaElement --

    Task<EntitySchemaElement> GetSchemaElementByID(Guid id);
    Task<List<EntitySchemaElement>> GetSchemaElementList(FilterSchemaElementList filter);
    Task<int> GetSchemaElementCount(FilterSchemaElementCount filter);
    Task<EntitySchemaElement> CreateSchemaElement(EntitySchemaElement entity);
    void UpdateSchemaElement(EntitySchemaElement entity);
    void DeleteSchemaElement(EntitySchemaElement entity);

    #endregion -- SchemaElement --

    #region -- SchemaElementHasProperty --

    Task<EntitySchemaElementHasProperty> GetSchemaElementHasPropertyByID(Guid id);
    Task<List<EntitySchemaElementHasProperty>> GetSchemaElementHasPropertyList(FilterSchemaElementHasPropertyList filter);
    Task<EntitySchemaElementHasProperty> CreateSchemaElementHasProperty(EntitySchemaElementHasProperty entity);
    void DeleteSchemaElementHasProperty(EntitySchemaElementHasProperty entity);

    #endregion -- SchemaElementHasProperty --

    #region -- SchemaParameter --

    Task<EntitySchemaParameter> GetSchemaParameterByID(Guid id);
    Task<EntitySchemaParameter> GetSchemaParameterByIdentifier(Guid schemaID, string identifier);
    Task<List<EntitySchemaParameter>> GetSchemaParameterList(FilterSchemaParameterList filter);
    Task<int> GetSchemaParameterCount(FilterSchemaParameterCount filter);
    Task<EntitySchemaParameterStyle> CreateSchemaParameterStyle(EntitySchemaParameterStyle entity);
    Task<EntitySchemaParameterSystem> CreateSchemaParameterSystem(EntitySchemaParameterSystem entity);
    Task<EntitySchemaParameterText> CreateSchemaParameterText(EntitySchemaParameterText entity);
    void UpdateSchemaParameterStyle(EntitySchemaParameterStyle entity);
    void UpdateSchemaParameterSystem(EntitySchemaParameterSystem entity);
    void UpdateSchemaParameterText(EntitySchemaParameterText entity);
    void DeleteSchemaParameter(EntitySchemaParameter entity);

    #endregion -- SchemaParameter --

    #region -- SchemaProperty --

    Task<EntitySchemaProperty> GetSchemaPropertyByID(Guid id);
    Task<List<EntitySchemaProperty>> GetSchemaPropertyList(FilterSchemaPropertyList filter);
    Task<int> GetSchemaPropertyCount(FilterSchemaPropertyCount filter);
    Task<EntitySchemaPropertyBoolean> CreateSchemaPropertyBoolean(EntitySchemaPropertyBoolean entity);
    Task<EntitySchemaPropertyDateTime> CreateSchemaPropertyDateTime(EntitySchemaPropertyDateTime entity);
    Task<EntitySchemaPropertyDecimal> CreateSchemaPropertyDecimal(EntitySchemaPropertyDecimal entity);
    Task<EntitySchemaPropertyImage> CreateSchemaPropertyImage(EntitySchemaPropertyImage entity);
    Task<EntitySchemaPropertyInteger> CreateSchemaPropertyInteger(EntitySchemaPropertyInteger entity);
    Task<EntitySchemaPropertyString> CreateSchemaPropertyString(EntitySchemaPropertyString entity);
    void UpdateSchemaPropertyBoolean(EntitySchemaPropertyBoolean entity);
    void UpdateSchemaPropertyDateTime(EntitySchemaPropertyDateTime entity);
    void UpdateSchemaPropertyDecimal(EntitySchemaPropertyDecimal entity);
    void UpdateSchemaPropertyImage(EntitySchemaPropertyImage entity);
    void UpdateSchemaPropertyInteger(EntitySchemaPropertyInteger entity);
    void UpdateSchemaPropertyString(EntitySchemaPropertyString entity);
    void DeleteSchemaProperty(EntitySchemaProperty entity);

    #endregion -- SchemaProperty --

    Task<List<EntitySchemaContext>> GetSchemaContextListBySchemaID(Guid id);
    Task<List<EntitySchemaElement>> GetSchemaElementListBySchemaID(Guid id);
    Task<List<EntitySchemaParameter>> GetSchemaParameterListBySchemaID(Guid id);
    Task<List<EntitySchemaProperty>> GetSchemaPropertyListBySchemaID(Guid id);

    Task<List<EntitySchemaContextHasElement>> GetSchemaContextHasElementListBySchemaID(Guid id);
    Task<List<EntitySchemaElementHasProperty>> GetSchemaElementHasPropertyListBySchemaID(Guid id);
}
