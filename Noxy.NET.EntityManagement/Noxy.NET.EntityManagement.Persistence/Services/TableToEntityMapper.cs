using Noxy.NET.EntityManagement.Domain.Entities.Authentication;
using Noxy.NET.EntityManagement.Domain.Entities.Data;
using Noxy.NET.EntityManagement.Domain.Entities.Data.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Persistence.Interfaces.Services;
using Noxy.NET.EntityManagement.Persistence.Tables.Authentication;
using Noxy.NET.EntityManagement.Persistence.Tables.Data;
using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Services;

public class TableToEntityMapper : ITableToEntityMapper
{
    #region -- Private methods --

    private static bool TryExtendRelation(Guid id, Guid[]? listVisited, out Guid[] result)
    {
        result = listVisited ?? [];
        if (result.Contains(id)) return false;

        result = [..result, id];
        return true;
    }

    #endregion -- Private methods --

    #region -- Authentication --

    public EntityIdentity Map(TableIdentity table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityAuthentication Map(TableAuthentication table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
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

    public EntityDataElement Map(TableDataElement table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityDataParameter Map(TableDataParameter table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityDataParameterStyle Map(TableDataParameterStyle table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityDataParameterSystem Map(TableDataParameterSystem table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityDataParameterText Map(TableDataParameterText table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityDataProperty Map(TableDataProperty table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityDataPropertyBoolean Map(TableDataPropertyBoolean table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityDataPropertyDateTime Map(TableDataPropertyDateTime table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntityDataPropertyString Map(TableDataPropertyString table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));

    private EntityDataElement? MapInternal(TableDataElement? table)
    {
        if (table == null) return null;

        EntityDataElement mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            TimeCreated = table.TimeCreated,
            PropertyList = table.PropertyList?.Select(x => new EntityDataProperty.Discriminator(Map(x))).ToList()
        };

        return mapped;
    }

    private static EntityDataParameter? MapInternal(TableDataParameter? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        return table switch
        {
            TableDataParameterStyle property => MapInternal(property, listVisitedRelation),
            TableDataParameterSystem property => MapInternal(property, listVisitedRelation),
            TableDataParameterText property => MapInternal(property, listVisitedRelation),
            _ => throw new ArgumentOutOfRangeException(nameof(table), table, null)
        };
    }

    private static EntityDataParameterStyle? MapInternal(TableDataParameterStyle? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityDataParameterStyle mapped = new()
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

    private static EntityDataParameterSystem? MapInternal(TableDataParameterSystem? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityDataParameterSystem mapped = new()
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

    private static EntityDataParameterText? MapInternal(TableDataParameterText? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntityDataParameterText mapped = new()
        {
            ID = table.ID,
            Value = table.Value,
            SchemaIdentifier = table.SchemaIdentifier,
            TimeApproved = table.TimeApproved,
            TimeEffective = table.TimeEffective,
            TimeCreated = table.TimeCreated,
            Culture = table.Culture
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
            ElementID = table.ElementID,
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
            ElementID = table.ElementID,
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
            ElementID = table.ElementID,
            TimeCreated = table.TimeCreated
        };

        return mapped;
    }

    #endregion

    #region -- Many-To-Many --

    public EntitySchemaContextHasElement Map(TableSchemaContextHasElement? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaElementHasProperty Map(TableSchemaElementHasProperty? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaPropertyCollectionHasProperty Map(TableSchemaPropertyCollectionHasProperty? table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));

    private static EntitySchemaContextHasElement? MapInternal(TableSchemaContextHasElement? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaContextHasElement mapped = new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            EntityID = table.EntityID,
            RelationID = table.RelationID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Entity = MapInternal(table.Entity, listExtendedRelation);
        mapped.Relation = MapInternal(table.Relation, listExtendedRelation);

        return mapped;
    }

    private static EntitySchemaElementHasProperty? MapInternal(TableSchemaElementHasProperty? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaElementHasProperty mapped = new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            EntityID = table.EntityID,
            RelationID = table.RelationID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Entity = MapInternal(table.Entity, listExtendedRelation);
        mapped.Relation = new(MapInternal(table.Relation, listExtendedRelation));

        return mapped;
    }

    private static EntitySchemaPropertyCollectionHasProperty? MapInternal(TableSchemaPropertyCollectionHasProperty? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaPropertyCollectionHasProperty mapped = new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            EntityID = table.EntityID,
            RelationID = table.RelationID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Entity = MapInternal(table.Entity, listExtendedRelation);
        mapped.Relation = new(MapInternal(table.Relation, listExtendedRelation));

        return mapped;
    }

    private static EntitySchemaPropertyTableHasProperty? MapInternal(TableSchemaPropertyTableHasProperty? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaPropertyTableHasProperty mapped = new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            EntityID = table.EntityID,
            RelationID = table.RelationID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Entity = MapInternal(table.Entity, listExtendedRelation);
        mapped.Relation = new(MapInternal(table.Relation, listExtendedRelation));

        return mapped;
    }

    #endregion -- Many-To-Many --

    #region -- Schemas --

    public EntitySchema Map(TableSchema table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaContext Map(TableSchemaContext table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaElement Map(TableSchemaElement table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaParameter Map(TableSchemaParameter table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaParameterStyle Map(TableSchemaParameterStyle table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaParameterSystem Map(TableSchemaParameterSystem table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaParameterText Map(TableSchemaParameterText table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaProperty Map(TableSchemaProperty table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaPropertyBoolean Map(TableSchemaPropertyBoolean table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaPropertyCollection Map(TableSchemaPropertyCollection table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaPropertyDateTime Map(TableSchemaPropertyDateTime table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaPropertyDecimal Map(TableSchemaPropertyDecimal table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaPropertyImage Map(TableSchemaPropertyImage table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaPropertyInteger Map(TableSchemaPropertyInteger table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaPropertyString Map(TableSchemaPropertyString table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));
    public EntitySchemaPropertyTable Map(TableSchemaPropertyTable table) => MapInternal(table) ?? throw new ArgumentNullException(nameof(table));

    private static EntitySchema? MapInternal(TableSchema? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchema mapped = new()
        {
            ID = table.ID,
            Name = table.Name,
            Note = table.Note,
            IsActive = table.IsActive,
            TimeCreated = table.TimeCreated,
            TimeActivated = table.TimeActivated
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.ElementList = table.ElementList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.PropertyList = table.PropertyList?.Select(x => new EntitySchemaProperty.Discriminator(MapInternal(x, listExtendedRelation)!)).ToList();

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
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleTextParameterID = table.TitleParameterTextID,
            DescriptionTextParameterID = table.DescriptionParameterTextID,
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleTextParameter = MapInternal(table.TitleParameterText, listVisitedRelation);
        mapped.DescriptionTextParameter = MapInternal(table.DescriptionParameterText, listVisitedRelation);
        mapped.ElementList = table.ElementList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    private static EntitySchemaParameterStyle? MapInternal(TableSchemaParameterStyle? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaParameterStyle mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            IsSystemDefined = table.IsSystemDefined,
            IsApprovalRequired = table.IsApprovalRequired,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);

        return mapped;
    }

    private static EntitySchemaParameterSystem? MapInternal(TableSchemaParameterSystem? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaParameterSystem mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Type = table.Type,
            IsPublic = table.IsPublic,
            IsSystemDefined = table.IsSystemDefined,
            IsApprovalRequired = table.IsApprovalRequired,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);

        return mapped;
    }

    private static EntitySchemaParameterText? MapInternal(TableSchemaParameterText? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        EntitySchemaParameterText mapped = new()
        {
            ID = table.ID,
            SchemaIdentifier = table.SchemaIdentifier,
            Name = table.Name,
            Note = table.Note,
            Type = table.Type,
            IsSystemDefined = table.IsSystemDefined,
            IsApprovalRequired = table.IsApprovalRequired,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);

        return mapped;
    }

    private static EntitySchemaParameter? MapInternal(TableSchemaParameter? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        return table switch
        {
            TableSchemaParameterStyle value => MapInternal(value, listVisitedRelation),
            TableSchemaParameterSystem value => MapInternal(value, listVisitedRelation),
            TableSchemaParameterText value => MapInternal(value, listVisitedRelation),
            _ => throw new ArgumentOutOfRangeException(nameof(table), table, null)
        };
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
            Weight = table.Weight,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleParameterTextID = table.TitleParameterTextID,
            DescriptionParameterTextID = table.DescriptionParameterTextID,
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;
        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleTextParameter = MapInternal(table.TitleParameterText, listVisitedRelation);
        mapped.DescriptionTextParameter = MapInternal(table.DescriptionParameterText, listVisitedRelation);
        mapped.PropertyList = table.PropertyList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    private static EntitySchemaProperty? MapInternal(TableSchemaProperty? table, Guid[]? listVisitedRelation = null)
    {
        if (table == null) return null;

        return table switch
        {
            TableSchemaPropertyBoolean property => MapInternal(property, listVisitedRelation),
            TableSchemaPropertyDateTime property => MapInternal(property, listVisitedRelation),
            TableSchemaPropertyDecimal property => MapInternal(property, listVisitedRelation),
            TableSchemaPropertyInteger property => MapInternal(property, listVisitedRelation),
            TableSchemaPropertyString property => MapInternal(property, listVisitedRelation),
            _ => throw new ArgumentOutOfRangeException(nameof(table), table, null)
        };
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
            Weight = table.Weight,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleParameterTextID = table.TitleParameterTextID,
            DescriptionParameterTextID = table.DescriptionParameterTextID,
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);

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
            Weight = table.Weight,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleParameterTextID = table.TitleParameterTextID,
            DescriptionParameterTextID = table.DescriptionParameterTextID,
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleParameterText = MapInternal(table.TitleParameterText, listExtendedRelation);
        mapped.DescriptionParameterText = MapInternal(table.DescriptionParameterText, listExtendedRelation);

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
            Weight = table.Weight,
            Type = table.Type,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleParameterTextID = table.TitleParameterTextID,
            DescriptionParameterTextID = table.DescriptionParameterTextID,
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleParameterText = MapInternal(table.TitleParameterText, listExtendedRelation);
        mapped.DescriptionParameterText = MapInternal(table.DescriptionParameterText, listExtendedRelation);

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
            Weight = table.Weight,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleParameterTextID = table.TitleParameterTextID,
            DescriptionParameterTextID = table.DescriptionParameterTextID,
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleParameterText = MapInternal(table.TitleParameterText, listExtendedRelation);
        mapped.DescriptionParameterText = MapInternal(table.DescriptionParameterText, listExtendedRelation);

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
            Weight = table.Weight,
            AllowedExtensions = table.AllowedExtensions,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleParameterTextID = table.TitleParameterTextID,
            DescriptionParameterTextID = table.DescriptionParameterTextID,
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleParameterText = MapInternal(table.TitleParameterText, listExtendedRelation);
        mapped.DescriptionParameterText = MapInternal(table.DescriptionParameterText, listExtendedRelation);

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
            Weight = table.Weight,
            IsUnsigned = table.IsUnsigned,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleParameterTextID = table.TitleParameterTextID,
            DescriptionParameterTextID = table.DescriptionParameterTextID,
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleParameterText = MapInternal(table.TitleParameterText, listExtendedRelation);
        mapped.DescriptionParameterText = MapInternal(table.DescriptionParameterText, listExtendedRelation);

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
            Weight = table.Weight,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleParameterTextID = table.TitleParameterTextID,
            DescriptionParameterTextID = table.DescriptionParameterTextID,
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleParameterText = MapInternal(table.TitleParameterText, listExtendedRelation);
        mapped.DescriptionParameterText = MapInternal(table.DescriptionParameterText, listExtendedRelation);

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
            Weight = table.Weight,
            TimeCreated = table.TimeCreated,
            SchemaID = table.SchemaID,
            TitleParameterTextID = table.TitleParameterTextID,
            DescriptionParameterTextID = table.DescriptionParameterTextID,
        };

        if (!TryExtendRelation(table.ID, listVisitedRelation, out Guid[] listExtendedRelation)) return mapped;

        mapped.Schema = MapInternal(table.Schema, listExtendedRelation);
        mapped.TitleParameterText = MapInternal(table.TitleParameterText, listExtendedRelation);
        mapped.DescriptionParameterText = MapInternal(table.DescriptionParameterText, listExtendedRelation);

        mapped.RelationElementList = table.RelationElementList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.PropertyList = table.PropertyList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();
        mapped.RelationPropertyCollectionList = table.RelationPropertyCollectionList?.Select(x => MapInternal(x, listExtendedRelation)!).ToList();

        return mapped;
    }

    #endregion -- Schemas --
}
