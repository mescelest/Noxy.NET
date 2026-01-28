using Noxy.NET.CaseManagement.Domain.Entities.Authentication;
using Noxy.NET.CaseManagement.Domain.Entities.Data;
using Noxy.NET.CaseManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Associations;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.CaseManagement.Persistence.Interfaces.Services;
using Noxy.NET.CaseManagement.Persistence.Tables.Authentication;
using Noxy.NET.CaseManagement.Persistence.Tables.Data;
using Noxy.NET.CaseManagement.Persistence.Tables.Data.Discriminators;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Associations;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Persistence.Services;

public class TableToEntityMapper : ITableToEntityMapper
{
    #region -- Authentication --

    public EntityIdentity Map(TableIdentity? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityAuthentication Map(TableAuthentication? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityUser Map(TableUser table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));

    private static EntityIdentity? MapInternal(TableIdentity? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityIdentity mapped = new()
        {
            ID = table.ID,
            Handle = table.Handle,
            Username = table.Username,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            TimeSignIn = table.TimeSignIn,
            User = null,
            UserID = table.UserID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.User = MapInternal(table.User, listExtendedRelation);

        return mapped;
    }

    private static EntityAuthentication? MapInternal(TableAuthentication? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityAuthentication mapped = new()
        {
            ID = table.ID,
            Hash = table.Hash,
            Salt = table.Salt,
            User = null,
            UserID = table.UserID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.User = MapInternal(table.User, listExtendedRelation);

        return mapped;
    }

    private static EntityUser? MapInternal(TableUser? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityUser mapped = new()
        {
            ID = table.ID,
            Email = table.Email,
            TimeCreated = table.TimeCreated,
            TimeVerified = table.TimeVerified,
            TimeSignIn = table.TimeSignIn,
            Authentication = null,
            AuthenticationID = table.AuthenticationID,
            IdentityList = null
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.Authentication = MapInternal(table.Authentication, listExtendedRelation);
        mapped.IdentityList = table.IdentityList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    #endregion -- Authentication --

    #region -- Data --

    public EntityDataElement Map(TableDataElement? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityDataProperty Map(TableDataProperty? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityDataPropertyBoolean Map(TableDataPropertyBoolean? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityDataPropertyDateTime Map(TableDataPropertyDateTime? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityDataPropertyString Map(TableDataPropertyString? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityDataSystemParameter Map(TableDataSystemParameter? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityDataTextParameter Map(TableDataTextParameter? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));

    private static EntityDataElement? MapInternal(TableDataElement? table)
    {
        if (table == null) return null;

        EntityDataElement mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            TimeCreated = table.TimeCreated,
            PropertyList = table.PropertyList?.Select(x => new EntityDataProperty.Discriminator(MapInternal(x))).ToList()
        };

        return mapped;
    }

    private static EntityDataProperty? MapInternal(TableDataProperty? table)
    {
        if (table == null) return null;

        return table switch
        {
            TableDataPropertyBoolean property => MapInternal(property),
            TableDataPropertyDateTime property => MapInternal(property),
            TableDataPropertyString property => MapInternal(property),
            _ => throw new ArgumentOutOfRangeException(nameof(table), table, null)
        };
    }

    private static EntityDataPropertyBoolean? MapInternal(TableDataPropertyBoolean? table)
    {
        if (table == null) return null;

        EntityDataPropertyBoolean mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Value = table.Value,
            TimeCreated = table.TimeCreated
        };

        return mapped;
    }

    private static EntityDataPropertyDateTime? MapInternal(TableDataPropertyDateTime? table)
    {
        if (table == null) return null;

        EntityDataPropertyDateTime mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Value = table.Value,
            TimeCreated = table.TimeCreated
        };

        return mapped;
    }

    private static EntityDataPropertyString? MapInternal(TableDataPropertyString? table)
    {
        if (table == null) return null;

        EntityDataPropertyString mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Value = table.Value,
            TimeCreated = table.TimeCreated
        };

        return mapped;
    }

    private static EntityDataSystemParameter? MapInternal(TableDataSystemParameter? table)
    {
        if (table == null) return null;

        EntityDataSystemParameter mapped = new()
        {
            ID = table.ID,
            Value = table.Value,
            SchemaIdentifier = table.SchemaIdentifier,
            TimeApproved = table.TimeApproved,
            TimeEffective = table.TimeEffective,
            TimeCreated = table.TimeCreated
        };

        return mapped;
    }

    private static EntityDataTextParameter? MapInternal(TableDataTextParameter? table)
    {
        if (table == null) return null;

        EntityDataTextParameter mapped = new()
        {
            ID = table.ID,
            Value = table.Value,
            SchemaIdentifier = table.SchemaIdentifier,
            TimeApproved = table.TimeApproved,
            TimeEffective = table.TimeEffective,
            TimeCreated = table.TimeCreated
        };

        return mapped;
    }

    #endregion

    #region -- Many-To-Many --

    public EntityJunctionSchemaActionHasActionStep Map(TableJunctionSchemaActionHasActionStep? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityJunctionSchemaActionHasDynamicValueCode Map(TableJunctionSchemaActionHasDynamicValueCode? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityJunctionSchemaActionStepHasActionInput Map(TableJunctionSchemaActionStepHasActionInput? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityAssociationSchemaActionInputHasAttribute.Discriminator Map(TableAssociationSchemaActionInputHasAttribute? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityAssociationSchemaActionInputHasAttributeDynamicValue Map(TableAssociationSchemaActionInputHasAttributeDynamicValue? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityAssociationSchemaActionInputHasAttributeInteger Map(TableAssociationSchemaActionInputHasAttributeInteger? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityAssociationSchemaActionInputHasAttributeString Map(TableAssociationSchemaActionInputHasAttributeString? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityJunctionSchemaContextHasAction Map(TableJunctionSchemaContextHasAction? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityJunctionSchemaContextHasElement Map(TableJunctionSchemaContextHasElement? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityJunctionSchemaElementHasAction Map(TableJunctionSchemaElementHasAction? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityJunctionSchemaElementHasProperty Map(TableJunctionSchemaElementHasProperty? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityJunctionSchemaInputHasAttribute Map(TableJunctionSchemaInputHasAttribute? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityJunctionSchemaPropertyCollectionHasProperty Map(TableJunctionSchemaPropertyCollectionHasProperty? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));

    private static EntityJunctionSchemaActionHasActionStep? MapInternal(TableJunctionSchemaActionHasActionStep? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityJunctionSchemaActionHasActionStep mapped = new()
        {
            ID = table.ID,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            EntityID = table.EntityID,
            RelationID = table.RelationID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Entity = MapInternal(table.Entity, listExtendedRelation);
        mapped.Relation = MapInternal(table.Relation, listExtendedRelation);

        return mapped;
    }

    private static EntityJunctionSchemaActionHasDynamicValueCode? MapInternal(TableJunctionSchemaActionHasDynamicValueCode? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityJunctionSchemaActionHasDynamicValueCode mapped = new()
        {
            ID = table.ID,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            EntityID = table.EntityID,
            RelationID = table.RelationID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Entity = MapInternal(table.Entity, listExtendedRelation);
        mapped.Relation = MapInternal(table.Relation, listExtendedRelation);

        return mapped;
    }

    private static EntityJunctionSchemaActionStepHasActionInput? MapInternal(TableJunctionSchemaActionStepHasActionInput? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityJunctionSchemaActionStepHasActionInput mapped = new()
        {
            ID = table.ID,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            EntityID = table.EntityID,
            RelationID = table.RelationID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Entity = MapInternal(table.Entity, listExtendedRelation);
        mapped.Relation = MapInternal(table.Relation, listExtendedRelation);

        return mapped;
    }


    private static EntityAssociationSchemaActionInputHasAttribute.Discriminator? MapInternal(TableAssociationSchemaActionInputHasAttribute? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        return new(table switch
        {
            TableAssociationSchemaActionInputHasAttributeDynamicValue value => MapInternal(value, listVisitedRelation),
            TableAssociationSchemaActionInputHasAttributeInteger value => MapInternal(value, listVisitedRelation),
            TableAssociationSchemaActionInputHasAttributeString value => MapInternal(value, listVisitedRelation),
            _ => throw new ArgumentOutOfRangeException(nameof(table), table, null)
        });
    }

    private static EntityAssociationSchemaActionInputHasAttributeDynamicValue? MapInternal(TableAssociationSchemaActionInputHasAttributeDynamicValue? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityAssociationSchemaActionInputHasAttributeDynamicValue mapped = new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            EntityID = table.EntityID,
            RelationID = table.RelationID,
            ValueID = table.ValueID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Entity = MapInternal(table.Entity, listExtendedRelation);
        mapped.Relation = MapInternal(table.Relation, listExtendedRelation);

        return mapped;
    }

    private static EntityAssociationSchemaActionInputHasAttributeInteger? MapInternal(TableAssociationSchemaActionInputHasAttributeInteger? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityAssociationSchemaActionInputHasAttributeInteger mapped = new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            EntityID = table.EntityID,
            RelationID = table.RelationID,
            Value = table.Value
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Entity = MapInternal(table.Entity, listExtendedRelation);
        mapped.Relation = MapInternal(table.Relation, listExtendedRelation);

        return mapped;
    }

    private static EntityAssociationSchemaActionInputHasAttributeString? MapInternal(TableAssociationSchemaActionInputHasAttributeString? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityAssociationSchemaActionInputHasAttributeString mapped = new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            EntityID = table.EntityID,
            RelationID = table.RelationID,
            Value = table.Value
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Entity = MapInternal(table.Entity, listExtendedRelation);
        mapped.Relation = MapInternal(table.Relation, listExtendedRelation);

        return mapped;
    }

    private static EntityJunctionSchemaContextHasAction? MapInternal(TableJunctionSchemaContextHasAction? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityJunctionSchemaContextHasAction mapped = new()
        {
            ID = table.ID,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            EntityID = table.EntityID,
            RelationID = table.RelationID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Entity = MapInternal(table.Entity, listExtendedRelation);
        mapped.Relation = MapInternal(table.Relation, listExtendedRelation);

        return mapped;
    }

    private static EntityJunctionSchemaContextHasElement? MapInternal(TableJunctionSchemaContextHasElement? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityJunctionSchemaContextHasElement mapped = new()
        {
            ID = table.ID,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            EntityID = table.EntityID,
            RelationID = table.RelationID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Entity = MapInternal(table.Entity, listExtendedRelation);
        mapped.Relation = MapInternal(table.Relation, listExtendedRelation);

        return mapped;
    }

    private static EntityJunctionSchemaElementHasAction? MapInternal(TableJunctionSchemaElementHasAction? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityJunctionSchemaElementHasAction mapped = new()
        {
            ID = table.ID,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            EntityID = table.EntityID,
            RelationID = table.RelationID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Entity = MapInternal(table.Entity, listExtendedRelation);
        mapped.Relation = MapInternal(table.Relation, listExtendedRelation);

        return mapped;
    }

    private static EntityJunctionSchemaElementHasProperty? MapInternal(TableJunctionSchemaElementHasProperty? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityJunctionSchemaElementHasProperty mapped = new()
        {
            ID = table.ID,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            EntityID = table.EntityID,
            RelationID = table.RelationID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Entity = MapInternal(table.Entity, listExtendedRelation);
        mapped.Relation = MapInternal(table.Relation, listExtendedRelation);

        return mapped;
    }

    private static EntityJunctionSchemaInputHasAttribute? MapInternal(TableJunctionSchemaInputHasAttribute? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityJunctionSchemaInputHasAttribute mapped = new()
        {
            ID = table.ID,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            EntityID = table.EntityID,
            RelationID = table.RelationID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Entity = MapInternal(table.Entity, listExtendedRelation);
        mapped.Relation = MapInternal(table.Relation, listExtendedRelation);

        return mapped;
    }

    private static EntityJunctionSchemaPropertyCollectionHasProperty? MapInternal(TableJunctionSchemaPropertyCollectionHasProperty? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityJunctionSchemaPropertyCollectionHasProperty mapped = new()
        {
            ID = table.ID,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            EntityID = table.EntityID,
            RelationID = table.RelationID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Entity = MapInternal(table.Entity, listExtendedRelation);
        mapped.Relation = MapInternal(table.Relation, listExtendedRelation);

        return mapped;
    }

    private static EntityJunctionSchemaPropertyTableHasProperty? MapInternal(TableJunctionSchemaPropertyTableHasProperty? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityJunctionSchemaPropertyTableHasProperty mapped = new()
        {
            ID = table.ID,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            EntityID = table.EntityID,
            RelationID = table.RelationID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Entity = MapInternal(table.Entity, listExtendedRelation);
        mapped.Relation = MapInternal(table.Relation, listExtendedRelation);

        return mapped;
    }

    #endregion -- Many-To-Many --

    #region -- Schemas --

    public EntitySchemaAction Map(TableSchemaAction? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaActionInput Map(TableSchemaActionInput? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaActionStep Map(TableSchemaActionStep? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaAttribute Map(TableSchemaAttribute? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));

    public EntitySchemaContext Map(TableSchemaContext? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaDynamicValue.Discriminator Map(TableSchemaDynamicValue? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaDynamicValueCode Map(TableSchemaDynamicValueCode? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaDynamicValueTextParameter Map(TableSchemaDynamicValueTextParameter? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaDynamicValueSystemParameter Map(TableSchemaDynamicValueSystemParameter? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaElement Map(TableSchemaElement? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaInput Map(TableSchemaInput? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaProperty.Discriminator Map(TableSchemaProperty? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaPropertyBoolean Map(TableSchemaPropertyBoolean? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaPropertyCollection Map(TableSchemaPropertyCollection? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaPropertyDateTime Map(TableSchemaPropertyDateTime? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaPropertyDecimal Map(TableSchemaPropertyDecimal? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaPropertyImage Map(TableSchemaPropertyImage? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaPropertyInteger Map(TableSchemaPropertyInteger? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaPropertyString Map(TableSchemaPropertyString? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaPropertyTable Map(TableSchemaPropertyTable? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));

    private static EntitySchemaAction? MapInternal(TableSchemaAction? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaAction mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleDynamicID = table.TitleDynamicID,
            DescriptionDynamicID = table.DescriptionDynamicID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleDynamic = MapInternal(table.TitleDynamic, listExtendedRelation);
        mapped.DescriptionDynamic = MapInternal(table.DescriptionDynamic, listExtendedRelation);
        mapped.ActionStepList = table.ActionStepList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    private static EntitySchemaActionInput? MapInternal(TableSchemaActionInput? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaActionInput mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            InputID = table.InputID,
            TitleDynamicID = table.TitleDynamicID,
            DescriptionDynamicID = table.DescriptionDynamicID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleDynamic = MapInternal(table.TitleDynamic, listExtendedRelation);
        mapped.DescriptionDynamic = MapInternal(table.DescriptionDynamic, listExtendedRelation);
        mapped.ActionStepList = table.RelationActionStepList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    private static EntitySchemaActionStep? MapInternal(TableSchemaActionStep? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaActionStep mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            IsRepeatable = table.IsRepeatable,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleDynamicID = table.TitleDynamicID,
            DescriptionDynamicID = table.DescriptionDynamicID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleDynamic = MapInternal(table.TitleDynamic, listExtendedRelation);
        mapped.DescriptionDynamic = MapInternal(table.DescriptionDynamic, listExtendedRelation);
        mapped.ActionList = table.RelationActionList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.ActionInputList = table.ActionInputList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    private static EntitySchemaAttribute? MapInternal(TableSchemaAttribute? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaAttribute mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            Type = table.Type,
            IsValueList = table.IsList,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.InputList = table.RelationInputList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    private static EntitySchemaContext? MapInternal(TableSchemaContext? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaContext mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleDynamicID = table.TitleDynamicID,
            DescriptionDynamicID = table.DescriptionDynamicID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleDynamic = MapInternal(table.TitleDynamic, listExtendedRelation);
        mapped.DescriptionDynamic = MapInternal(table.DescriptionDynamic, listExtendedRelation);
        mapped.ActionList = table.ActionList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.ElementList = table.ElementList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    private static EntitySchemaDynamicValueCode? MapInternal(TableSchemaDynamicValueCode? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaDynamicValueCode mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            Value = table.Value,
            IsAsynchronous = table.IsAsynchronous,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);

        return mapped;
    }

    private static EntitySchemaDynamicValueSystemParameter? MapInternal(TableSchemaDynamicValueSystemParameter? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaDynamicValueSystemParameter mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            IsApprovalRequired = table.IsApprovalRequired,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);

        return mapped;
    }


    private static EntitySchemaDynamicValueTextParameter? MapInternal(TableSchemaDynamicValueTextParameter? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaDynamicValueTextParameter mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            Type = table.Type,
            IsApprovalRequired = table.IsApprovalRequired,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);

        return mapped;
    }

    private static EntitySchemaDynamicValue.Discriminator? MapInternal(TableSchemaDynamicValue? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        return new(table switch
        {
            TableSchemaDynamicValueCode value => MapInternal(value, listVisitedRelation),
            TableSchemaDynamicValueSystemParameter value => MapInternal(value, listVisitedRelation),
            TableSchemaDynamicValueTextParameter value => MapInternal(value, listVisitedRelation),
            _ => throw new ArgumentOutOfRangeException(nameof(table), table, null)
        });
    }

    private static EntitySchemaElement? MapInternal(TableSchemaElement? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaElement mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleDynamicID = table.TitleDynamicID,
            DescriptionDynamicID = table.DescriptionDynamicID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleDynamic = MapInternal(table.TitleDynamic, listExtendedRelation);
        mapped.DescriptionDynamic = MapInternal(table.DescriptionDynamic, listExtendedRelation);
        mapped.PropertyList = table.PropertyList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    private static EntitySchemaInput? MapInternal(TableSchemaInput? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaInput mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.ActionInputList = table.ActionInputList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList() ?? [];

        return mapped;
    }

    private static EntitySchemaProperty.Discriminator? MapInternal(TableSchemaProperty? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        return new(table switch
        {
            TableSchemaPropertyBoolean property => MapInternal(property, listVisitedRelation),
            TableSchemaPropertyDateTime property => MapInternal(property, listVisitedRelation),
            TableSchemaPropertyDecimal property => MapInternal(property, listVisitedRelation),
            TableSchemaPropertyInteger property => MapInternal(property, listVisitedRelation),
            TableSchemaPropertyString property => MapInternal(property, listVisitedRelation),
            _ => throw new ArgumentOutOfRangeException(nameof(table), table, null)
        });
    }

    private static EntitySchemaPropertyBoolean? MapInternal(TableSchemaPropertyBoolean? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaPropertyBoolean mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleDynamicID = table.TitleDynamicID,
            DescriptionDynamicID = table.DescriptionDynamicID,
            DefaultValueDynamicID = table.DefaultValueDynamicID,
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleDynamic = MapInternal(table.TitleDynamic, listExtendedRelation);
        mapped.DescriptionDynamic = MapInternal(table.DescriptionDynamic, listExtendedRelation);
        mapped.DefaultValueDynamic = MapInternal(table.DefaultValueDynamic, listExtendedRelation);

        mapped.RelationElementList = table.RelationElementList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.RelationPropertyCollectionList = table.RelationPropertyCollectionList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    private static EntitySchemaPropertyCollection? MapInternal(TableSchemaPropertyCollection? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaPropertyCollection mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleDynamicID = table.TitleDynamicID,
            DescriptionDynamicID = table.DescriptionDynamicID,
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleDynamic = MapInternal(table.TitleDynamic, listExtendedRelation);
        mapped.DescriptionDynamic = MapInternal(table.DescriptionDynamic, listExtendedRelation);

        mapped.RelationElementList = table.RelationElementList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.PropertyList = table.PropertyList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.RelationPropertyCollectionList = table.RelationPropertyCollectionList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    private static EntitySchemaPropertyDateTime? MapInternal(TableSchemaPropertyDateTime? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaPropertyDateTime mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            Type = table.Type,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleDynamicID = table.TitleDynamicID,
            DescriptionDynamicID = table.DescriptionDynamicID,
            DefaultValueDynamicID = table.DefaultValueDynamicID,
            MinDynamicID = table.MinDynamicID,
            MaxDynamicID = table.MaxDynamicID,
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleDynamic = MapInternal(table.TitleDynamic, listExtendedRelation);
        mapped.DescriptionDynamic = MapInternal(table.DescriptionDynamic, listExtendedRelation);
        mapped.DefaultValueDynamic = MapInternal(table.DefaultValueDynamic, listExtendedRelation);
        mapped.MinDynamic = MapInternal(table.MinDynamic, listExtendedRelation);
        mapped.MaxDynamic = MapInternal(table.MaxDynamic, listExtendedRelation);

        mapped.RelationElementList = table.RelationElementList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.RelationPropertyCollectionList = table.RelationPropertyCollectionList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    private static EntitySchemaPropertyDecimal? MapInternal(TableSchemaPropertyDecimal? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaPropertyDecimal mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleDynamicID = table.TitleDynamicID,
            DescriptionDynamicID = table.DescriptionDynamicID,
            DefaultValueDynamicID = table.DefaultValueDynamicID,
            MinDynamicID = table.MinDynamicID,
            MaxDynamicID = table.MaxDynamicID,
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleDynamic = MapInternal(table.TitleDynamic, listExtendedRelation);
        mapped.DescriptionDynamic = MapInternal(table.DescriptionDynamic, listExtendedRelation);
        mapped.DefaultValueDynamic = MapInternal(table.DefaultValueDynamic, listExtendedRelation);
        mapped.MinDynamic = MapInternal(table.MinDynamic, listExtendedRelation);
        mapped.MaxDynamic = MapInternal(table.MaxDynamic, listExtendedRelation);

        mapped.RelationElementList = table.RelationElementList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.RelationPropertyCollectionList = table.RelationPropertyCollectionList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    private static EntitySchemaPropertyImage? MapInternal(TableSchemaPropertyImage? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaPropertyImage mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            AllowedExtensions = table.AllowedExtensions,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleDynamicID = table.TitleDynamicID,
            DescriptionDynamicID = table.DescriptionDynamicID,
            DefaultValueDynamicID = table.DefaultValueDynamicID,
            WidthDynamicID = table.WidthDynamicID,
            HeightDynamicID = table.HeightDynamicID,
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleDynamic = MapInternal(table.TitleDynamic, listExtendedRelation);
        mapped.DescriptionDynamic = MapInternal(table.DescriptionDynamic, listExtendedRelation);
        mapped.DefaultValueDynamic = MapInternal(table.DefaultValueDynamic, listExtendedRelation);
        mapped.WidthDynamic = MapInternal(table.WidthDynamic, listExtendedRelation);
        mapped.HeightDynamic = MapInternal(table.HeightDynamic, listExtendedRelation);

        mapped.RelationElementList = table.RelationElementList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.RelationPropertyCollectionList = table.RelationPropertyCollectionList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    private static EntitySchemaPropertyInteger? MapInternal(TableSchemaPropertyInteger? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaPropertyInteger mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            IsUnsigned = table.IsUnsigned,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleDynamicID = table.TitleDynamicID,
            DescriptionDynamicID = table.DescriptionDynamicID,
            DefaultValueDynamicID = table.DefaultValueDynamicID,
            MinDynamicID = table.MinDynamicID,
            MaxDynamicID = table.MaxDynamicID,
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleDynamic = MapInternal(table.TitleDynamic, listExtendedRelation);
        mapped.DescriptionDynamic = MapInternal(table.DescriptionDynamic, listExtendedRelation);
        mapped.DefaultValueDynamic = MapInternal(table.DefaultValueDynamic, listExtendedRelation);
        mapped.MinDynamic = MapInternal(table.MinDynamic, listExtendedRelation);
        mapped.MaxDynamic = MapInternal(table.MaxDynamic, listExtendedRelation);

        mapped.RelationElementList = table.RelationElementList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.RelationPropertyCollectionList = table.RelationPropertyCollectionList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    private static EntitySchemaPropertyString? MapInternal(TableSchemaPropertyString? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaPropertyString mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleDynamicID = table.TitleDynamicID,
            DescriptionDynamicID = table.DescriptionDynamicID,
            DefaultValueDynamicID = table.DefaultValueDynamicID,
            MinDynamicID = table.MinDynamicID,
            MaxDynamicID = table.MaxDynamicID,
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleDynamic = MapInternal(table.TitleDynamic, listExtendedRelation);
        mapped.DescriptionDynamic = MapInternal(table.DescriptionDynamic, listExtendedRelation);
        mapped.DefaultValueDynamic = MapInternal(table.DefaultValueDynamic, listExtendedRelation);
        mapped.MinDynamic = MapInternal(table.MinDynamic, listExtendedRelation);
        mapped.MaxDynamic = MapInternal(table.MaxDynamic, listExtendedRelation);

        mapped.RelationElementList = table.RelationElementList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.RelationPropertyCollectionList = table.RelationPropertyCollectionList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    private static EntitySchemaPropertyTable? MapInternal(TableSchemaPropertyTable? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaPropertyTable mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleDynamicID = table.TitleDynamicID,
            DescriptionDynamicID = table.DescriptionDynamicID,
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleDynamic = MapInternal(table.TitleDynamic, listExtendedRelation);
        mapped.DescriptionDynamic = MapInternal(table.DescriptionDynamic, listExtendedRelation);

        mapped.RelationElementList = table.RelationElementList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.PropertyList = table.PropertyList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.RelationPropertyCollectionList = table.RelationPropertyCollectionList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    #endregion -- Schemas --

    #region -- Templates --

    public EntitySchema Map(TableSchema? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));

    private static EntitySchema? MapInternal(TableSchema? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchema mapped = new()
        {
            ID = table.ID,
            Name = table.Name,
            Note = table.Note,
            Order = table.Order,
            IsActive = table.IsActive,
            TimeCreated = table.TimeCreated,
            TimeActivated = table.TimeActivated
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.ActionList = table.ActionList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.ActionInputList = table.ActionInputList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.ElementList = table.ElementList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.InputList = table.InputList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.PropertyList = table.PropertyList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    #endregion -- Templates --

    #region -- Private methods --

    private static bool TryExtendRelation(Guid id, Guid[]? listVisited, out Guid[] result)
    {
        result = listVisited ?? [];
        if (result.Contains(id)) return false;

        result = [..result, id];
        return true;
    }

    #endregion -- Private methods --
}
