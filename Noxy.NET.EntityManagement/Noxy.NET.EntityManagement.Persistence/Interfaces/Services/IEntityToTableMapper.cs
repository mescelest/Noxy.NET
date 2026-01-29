using Noxy.NET.EntityManagement.Domain.Entities.Authentication;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Persistence.Tables.Authentication;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Interfaces.Services;

public interface IEntityToTableMapper
{
    #region -- Templates --

    TableSchema Map(EntitySchema entity);

    #endregion -- Templates --

    #region -- Authentication --

    TableAuthentication Map(EntityAuthentication entity);
    TableIdentity Map(EntityIdentity entity);
    TableUser Map(EntityUser entity);

    #endregion -- Authentication --

    #region -- Many-To-Many --

    TableJunctionSchemaContextHasElement Map(EntityJunctionSchemaContextHasElement entity);
    TableJunctionSchemaElementHasProperty Map(EntityJunctionSchemaElementHasProperty entity);

    #endregion -- Many-To-Many --

    #region -- Schemas --

    TableSchemaContext Map(EntitySchemaContext entity);
    TableSchemaParameter Map(EntitySchemaParameter entity);
    TableSchemaParameterStyle Map(EntitySchemaParameterStyle entity);
    TableSchemaParameterSystem Map(EntitySchemaParameterSystem entity);
    TableSchemaParameterText Map(EntitySchemaParameterText entity);
    TableSchemaElement Map(EntitySchemaElement entity);
    TableSchemaProperty Map(EntitySchemaProperty baseEntity);
    TableSchemaPropertyBoolean Map(EntitySchemaPropertyBoolean entity);
    TableSchemaPropertyCollection Map(EntitySchemaPropertyCollection entity);
    TableSchemaPropertyDateTime Map(EntitySchemaPropertyDateTime entity);
    TableSchemaPropertyDecimal Map(EntitySchemaPropertyDecimal entity);
    TableSchemaPropertyInteger Map(EntitySchemaPropertyInteger entity);
    TableSchemaPropertyImage Map(EntitySchemaPropertyImage entity);
    TableSchemaPropertyString Map(EntitySchemaPropertyString entity);
    TableSchemaPropertyTable Map(EntitySchemaPropertyTable entity);

    #endregion -- Schemas --
}
