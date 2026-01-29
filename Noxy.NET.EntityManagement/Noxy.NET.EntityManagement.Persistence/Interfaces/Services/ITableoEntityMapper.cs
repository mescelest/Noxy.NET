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

public interface ITableToEntityMapper
{
    #region -- Templates --

    EntitySchema Map(TableSchema? table);

    #endregion -- Templates --

    #region -- Authentication --

    EntityIdentity Map(TableIdentity table);
    EntityAuthentication Map(TableAuthentication table);
    EntityUser Map(TableUser table);

    #endregion -- Authentication --

    #region -- Data --

    EntityDataElement Map(TableDataElement? table);
    EntityDataProperty Map(TableDataProperty? table);
    EntityDataPropertyBoolean Map(TableDataPropertyBoolean? table);
    EntityDataPropertyDateTime Map(TableDataPropertyDateTime? table);
    EntityDataPropertyString Map(TableDataPropertyString? table);
    EntityDataSystemParameter Map(TableDataSystemParameter? table);
    EntityDataTextParameter Map(TableDataTextParameter? table);

    #endregion -- Data --

    #region -- Many-To-Many --

    EntityJunctionSchemaContextHasElement Map(TableJunctionSchemaContextHasElement? table);
    EntityJunctionSchemaElementHasProperty Map(TableJunctionSchemaElementHasProperty? table);

    #endregion -- Many-To-Many --

    #region -- Schemas --

    EntitySchemaContext Map(TableSchemaContext? table);
    EntitySchemaParameter.Discriminator Map(TableSchemaParameter? table);
    EntitySchemaParameterText Map(TableSchemaParameterText? table);
    EntitySchemaParameterSystem Map(TableSchemaParameterSystem? table);
    EntitySchemaElement Map(TableSchemaElement? table);
    EntitySchemaProperty.Discriminator Map(TableSchemaProperty? table);
    EntitySchemaPropertyBoolean Map(TableSchemaPropertyBoolean? table);
    EntitySchemaPropertyCollection Map(TableSchemaPropertyCollection? table);
    EntitySchemaPropertyDateTime Map(TableSchemaPropertyDateTime? table);
    EntitySchemaPropertyDecimal Map(TableSchemaPropertyDecimal? table);
    EntitySchemaPropertyImage Map(TableSchemaPropertyImage? table);
    EntitySchemaPropertyInteger Map(TableSchemaPropertyInteger? table);
    EntitySchemaPropertyString Map(TableSchemaPropertyString? table);
    EntitySchemaPropertyTable Map(TableSchemaPropertyTable? table);

    #endregion -- Schemas --
}
