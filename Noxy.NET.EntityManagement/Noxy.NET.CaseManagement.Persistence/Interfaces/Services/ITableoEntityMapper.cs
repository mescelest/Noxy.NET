using Noxy.NET.CaseManagement.Domain.Entities.Authentication;
using Noxy.NET.CaseManagement.Domain.Entities.Data;
using Noxy.NET.CaseManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Associations;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.CaseManagement.Persistence.Tables.Authentication;
using Noxy.NET.CaseManagement.Persistence.Tables.Data;
using Noxy.NET.CaseManagement.Persistence.Tables.Data.Discriminators;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Associations;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Persistence.Interfaces.Services;

public interface ITableToEntityMapper
{
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

    EntityAssociationSchemaActionInputHasAttribute.Discriminator Map(TableAssociationSchemaActionInputHasAttribute? table);
    EntityAssociationSchemaActionInputHasAttributeDynamicValue Map(TableAssociationSchemaActionInputHasAttributeDynamicValue? table);
    EntityAssociationSchemaActionInputHasAttributeInteger Map(TableAssociationSchemaActionInputHasAttributeInteger? table);
    EntityAssociationSchemaActionInputHasAttributeString Map(TableAssociationSchemaActionInputHasAttributeString? table);

    EntityJunctionSchemaActionHasDynamicValueCode Map(TableJunctionSchemaActionHasDynamicValueCode? table);
    EntityJunctionSchemaActionStepHasActionInput Map(TableJunctionSchemaActionStepHasActionInput? table);
    EntityJunctionSchemaActionHasActionStep Map(TableJunctionSchemaActionHasActionStep? table);
    EntityJunctionSchemaContextHasAction Map(TableJunctionSchemaContextHasAction? table);
    EntityJunctionSchemaContextHasElement Map(TableJunctionSchemaContextHasElement? table);
    EntityJunctionSchemaElementHasAction Map(TableJunctionSchemaElementHasAction? table);
    EntityJunctionSchemaElementHasProperty Map(TableJunctionSchemaElementHasProperty? table);
    EntityJunctionSchemaInputHasAttribute Map(TableJunctionSchemaInputHasAttribute? table);

    #endregion -- Many-To-Many --

    #region -- Schemas --

    EntitySchemaAction Map(TableSchemaAction? table);
    EntitySchemaActionInput Map(TableSchemaActionInput? table);
    EntitySchemaActionStep Map(TableSchemaActionStep? table);
    EntitySchemaAttribute Map(TableSchemaAttribute? table);
    EntitySchemaContext Map(TableSchemaContext? table);
    EntitySchemaDynamicValue.Discriminator Map(TableSchemaDynamicValue? table);
    EntitySchemaDynamicValueCode Map(TableSchemaDynamicValueCode? table);
    EntitySchemaDynamicValueTextParameter Map(TableSchemaDynamicValueTextParameter? table);
    EntitySchemaDynamicValueSystemParameter Map(TableSchemaDynamicValueSystemParameter? table);
    EntitySchemaElement Map(TableSchemaElement? table);
    EntitySchemaInput Map(TableSchemaInput? table);
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

    #region -- Templates --

    EntitySchema Map(TableSchema? table);

    #endregion -- Templates --
}
