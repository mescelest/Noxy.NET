using Noxy.NET.EntityManagement.Domain.Entities.Authentication;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Persistence.Tables.Authentication;
using Noxy.NET.EntityManagement.Persistence.Tables.Data;
using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Interfaces.Services;

public interface IEntityToTableMapper
{
    #region -- Authentication --

    TableAuthentication Map(EntityAuthentication entity);
    TableIdentity Map(EntityIdentity entity);
    TableUser Map(EntityUser entity);

    #endregion -- Authentication --

    #region -- Data --

    TableDataElement Map(EntityDataElement entity);
    TableDataParameter Map(EntityDataParameter entity);
    TableDataParameterStyle Map(EntityDataParameterStyle entity);
    TableDataParameterSystem Map(EntityDataParameterSystem entity);
    TableDataParameterText Map(EntityDataParameterText entity);
    TableDataProperty Map(EntityDataProperty entity);
    TableDataPropertyBoolean Map(EntityDataPropertyBoolean entity);
    TableDataPropertyDateTime Map(EntityDataPropertyDateTime entity);
    TableDataPropertyString Map(EntityDataPropertyString entity);

    #endregion -- Data --

    #region -- Many-To-Many --

    TableSchemaContextHasElement Map(EntitySchemaContextHasElement entity);
    TableSchemaElementHasProperty Map(EntitySchemaElementHasProperty entity);

    #endregion -- Many-To-Many --

    #region -- Schemas --

    TableSchema Map(EntitySchema entity);
    TableSchemaContext Map(EntitySchemaContext entity);
    TableSchemaElement Map(EntitySchemaElement entity);
    TableSchemaParameter Map(EntitySchemaParameter entity);
    TableSchemaParameterStyle Map(EntitySchemaParameterStyle entity);
    TableSchemaParameterSystem Map(EntitySchemaParameterSystem entity);
    TableSchemaParameterText Map(EntitySchemaParameterText entity);
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
