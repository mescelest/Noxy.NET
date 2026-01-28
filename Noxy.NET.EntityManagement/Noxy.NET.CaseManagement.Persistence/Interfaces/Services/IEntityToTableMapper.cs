using Noxy.NET.CaseManagement.Domain.Entities.Authentication;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.CaseManagement.Persistence.Tables.Authentication;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Persistence.Interfaces.Services;

public interface IEntityToTableMapper
{
    #region -- Authentication --

    TableAuthentication Map(EntityAuthentication entity);
    TableIdentity Map(EntityIdentity entity);
    TableUser Map(EntityUser entity);

    #endregion -- Authentication --

    #region -- Many-To-Many --

    TableJunctionSchemaActionHasActionStep Map(EntityJunctionSchemaActionHasActionStep entity);
    TableJunctionSchemaActionHasDynamicValueCode Map(EntityJunctionSchemaActionHasDynamicValueCode entity);
    TableJunctionSchemaActionStepHasActionInput Map(EntityJunctionSchemaActionStepHasActionInput entity);
    TableJunctionSchemaContextHasAction Map(EntityJunctionSchemaContextHasAction entity);
    TableJunctionSchemaContextHasElement Map(EntityJunctionSchemaContextHasElement entity);
    TableJunctionSchemaElementHasAction Map(EntityJunctionSchemaElementHasAction entity);
    TableJunctionSchemaElementHasProperty Map(EntityJunctionSchemaElementHasProperty entity);
    TableJunctionSchemaInputHasAttribute Map(EntityJunctionSchemaInputHasAttribute entity);

    #endregion -- Many-To-Many --

    #region -- Schemas --

    TableSchemaAction Map(EntitySchemaAction entity);
    TableSchemaActionInput Map(EntitySchemaActionInput entity);
    TableSchemaActionStep Map(EntitySchemaActionStep entity);
    TableSchemaAttribute Map(EntitySchemaAttribute entity);
    TableSchemaContext Map(EntitySchemaContext entity);
    TableSchemaDynamicValue Map(EntitySchemaDynamicValue entity);
    TableSchemaDynamicValueCode Map(EntitySchemaDynamicValueCode entity);
    TableSchemaDynamicValueStyleParameter Map(EntitySchemaDynamicValueStyleParameter entity);
    TableSchemaDynamicValueSystemParameter Map(EntitySchemaDynamicValueSystemParameter entity);
    TableSchemaDynamicValueTextParameter Map(EntitySchemaDynamicValueTextParameter entity);
    TableSchemaElement Map(EntitySchemaElement entity);
    TableSchemaInput Map(EntitySchemaInput entity);
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

    #region -- Templates --

    TableSchema Map(EntitySchema entity);

    #endregion -- Templates --
}
