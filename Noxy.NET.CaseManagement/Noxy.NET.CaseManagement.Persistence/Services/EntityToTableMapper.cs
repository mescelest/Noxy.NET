using Noxy.NET.CaseManagement.Domain.Entities.Authentication;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.CaseManagement.Persistence.Interfaces.Services;
using Noxy.NET.CaseManagement.Persistence.Tables.Authentication;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Persistence.Services;

public class EntityToTableMapper : IEntityToTableMapper
{
    #region -- Authentication --

    public TableAuthentication Map(EntityAuthentication entity)
    {
        return new()
        {
            ID = entity.ID,
            Salt = entity.Salt,
            Hash = entity.Hash,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            UserID = entity.UserID
        };
    }

    public TableIdentity Map(EntityIdentity entity)
    {
        return new()
        {
            ID = entity.ID,
            Handle = entity.Handle,
            Username = entity.Username,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            TimeSignIn = entity.TimeSignIn,
            UserID = entity.UserID
        };
    }

    public TableUser Map(EntityUser entity)
    {
        return new()
        {
            ID = entity.ID,
            Email = entity.Email,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            TimeSignIn = entity.TimeSignIn,
            TimeVerified = entity.TimeVerified,
            AuthenticationID = entity.AuthenticationID
        };
    }

    #endregion -- Authentication --

    #region -- Many-To-Many --

    public TableJunctionSchemaActionHasActionStep Map(EntityJunctionSchemaActionHasActionStep entity)
    {
        return new()
        {
            ID = entity.ID,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            EntityID = entity.EntityID,
            RelationID = entity.RelationID
        };
    }

    public TableJunctionSchemaActionHasDynamicValueCode Map(EntityJunctionSchemaActionHasDynamicValueCode entity)
    {
        return new()
        {
            ID = entity.ID,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            EntityID = entity.EntityID,
            RelationID = entity.RelationID
        };
    }

    public TableJunctionSchemaActionStepHasActionInput Map(EntityJunctionSchemaActionStepHasActionInput entity)
    {
        return new()
        {
            ID = entity.ID,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            EntityID = entity.EntityID,
            RelationID = entity.RelationID
        };
    }

    public TableJunctionSchemaContextHasAction Map(EntityJunctionSchemaContextHasAction entity)
    {
        return new()
        {
            ID = entity.ID,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            EntityID = entity.EntityID,
            RelationID = entity.RelationID
        };
    }

    public TableJunctionSchemaContextHasElement Map(EntityJunctionSchemaContextHasElement entity)
    {
        return new()
        {
            ID = entity.ID,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            EntityID = entity.EntityID,
            RelationID = entity.RelationID
        };
    }

    public TableJunctionSchemaElementHasAction Map(EntityJunctionSchemaElementHasAction entity)
    {
        return new()
        {
            ID = entity.ID,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            EntityID = entity.EntityID,
            RelationID = entity.RelationID
        };
    }

    public TableJunctionSchemaElementHasProperty Map(EntityJunctionSchemaElementHasProperty entity)
    {
        return new()
        {
            ID = entity.ID,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            EntityID = entity.EntityID,
            RelationID = entity.RelationID
        };
    }

    public TableJunctionSchemaInputHasAttribute Map(EntityJunctionSchemaInputHasAttribute entity)
    {
        return new()
        {
            ID = entity.ID,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            EntityID = entity.EntityID,
            RelationID = entity.RelationID,
            Order = entity.Order
        };
    }

    #endregion -- Many-To-Many --

    #region -- Schemas --

    public TableSchemaAction Map(EntitySchemaAction entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleDynamicID = entity.TitleDynamicID,
            DescriptionDynamicID = entity.DescriptionDynamicID,
            IsActiveDynamicID = entity.IsActiveDynamicID
        };
    }

    public TableSchemaActionInput Map(EntitySchemaActionInput entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            InputID = entity.InputID,
            TitleDynamicID = entity.TitleDynamicID,
            DescriptionDynamicID = entity.DescriptionDynamicID,
            IsActiveDynamicID = entity.IsActiveDynamicID,
            IsValidDynamicID = entity.IsValidDynamicID
        };
    }

    public TableSchemaActionStep Map(EntitySchemaActionStep entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleDynamicID = entity.TitleDynamicID,
            DescriptionDynamicID = entity.DescriptionDynamicID
        };
    }

    public TableSchemaAttribute Map(EntitySchemaAttribute entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            Type = entity.Type,
            IsList = entity.IsValueList,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID
        };
    }

    public TableSchemaContext Map(EntitySchemaContext entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleDynamicID = entity.TitleDynamicID,
            DescriptionDynamicID = entity.DescriptionDynamicID
        };
    }

    public TableSchemaDynamicValue Map(EntitySchemaDynamicValue entity)
    {
        return entity switch
        {
            EntitySchemaDynamicValueCode value => Map(value),
            EntitySchemaDynamicValueStyleParameter value => Map(value),
            EntitySchemaDynamicValueSystemParameter value => Map(value),
            EntitySchemaDynamicValueTextParameter value => Map(value),
            _ => throw new ArgumentOutOfRangeException(nameof(entity))
        };
    }

    public TableSchemaDynamicValueCode Map(EntitySchemaDynamicValueCode entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            IsAsynchronous = entity.IsAsynchronous,
            Value = entity.Value,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID
        };
    }

    public TableSchemaDynamicValueStyleParameter Map(EntitySchemaDynamicValueStyleParameter entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            IsApprovalRequired = entity.IsApprovalRequired,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID
        };
    }

    public TableSchemaDynamicValueSystemParameter Map(EntitySchemaDynamicValueSystemParameter entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            IsApprovalRequired = entity.IsApprovalRequired,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID
        };
    }

    public TableSchemaDynamicValueTextParameter Map(EntitySchemaDynamicValueTextParameter entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            IsApprovalRequired = entity.IsApprovalRequired,
            Type = entity.Type,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID
        };
    }

    public TableSchemaElement Map(EntitySchemaElement entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleDynamicID = entity.TitleDynamicID,
            DescriptionDynamicID = entity.DescriptionDynamicID
        };
    }

    public TableSchemaInput Map(EntitySchemaInput entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID
        };
    }

    public TableSchemaProperty Map(EntitySchemaProperty entity)
    {
        return entity switch
        {
            EntitySchemaPropertyBoolean value => Map(value),
            EntitySchemaPropertyDateTime value => Map(value),
            EntitySchemaPropertyString value => Map(value),
            EntitySchemaPropertyDecimal value => Map(value),
            EntitySchemaPropertyInteger value => Map(value),
            _ => throw new ArgumentOutOfRangeException(nameof(entity))
        };
    }

    public TableSchemaPropertyBoolean Map(EntitySchemaPropertyBoolean entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleDynamicID = entity.TitleDynamicID,
            DescriptionDynamicID = entity.DescriptionDynamicID,
            DefaultValueDynamicID = entity.DefaultValueDynamicID,
        };
    }

    public TableSchemaPropertyCollection Map(EntitySchemaPropertyCollection entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleDynamicID = entity.TitleDynamicID,
            DescriptionDynamicID = entity.DescriptionDynamicID,
        };
    }

    public TableSchemaPropertyDateTime Map(EntitySchemaPropertyDateTime entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            Type = entity.Type,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleDynamicID = entity.TitleDynamicID,
            DescriptionDynamicID = entity.DescriptionDynamicID,
            DefaultValueDynamicID = entity.DefaultValueDynamicID,
            MinDynamicID = entity.MinDynamicID,
            MaxDynamicID = entity.MaxDynamicID,
        };
    }

    public TableSchemaPropertyDecimal Map(EntitySchemaPropertyDecimal entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleDynamicID = entity.TitleDynamicID,
            DescriptionDynamicID = entity.DescriptionDynamicID,
            DefaultValueDynamicID = entity.DefaultValueDynamicID,
            MinDynamicID = entity.MinDynamicID,
            MaxDynamicID = entity.MaxDynamicID,
        };
    }

    public TableSchemaPropertyImage Map(EntitySchemaPropertyImage entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            AllowedExtensions = entity.AllowedExtensions,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleDynamicID = entity.TitleDynamicID,
            DescriptionDynamicID = entity.DescriptionDynamicID,
            DefaultValueDynamicID = entity.DefaultValueDynamicID,
            WidthDynamicID = entity.WidthDynamicID,
            HeightDynamicID = entity.HeightDynamicID,
        };
    }

    public TableSchemaPropertyInteger Map(EntitySchemaPropertyInteger entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleDynamicID = entity.TitleDynamicID,
            DescriptionDynamicID = entity.DescriptionDynamicID,
            DefaultValueDynamicID = entity.DefaultValueDynamicID,
            MinDynamicID = entity.MinDynamicID,
            MaxDynamicID = entity.MaxDynamicID,
        };
    }

    public TableSchemaPropertyString Map(EntitySchemaPropertyString entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleDynamicID = entity.TitleDynamicID,
            DescriptionDynamicID = entity.DescriptionDynamicID,
            DefaultValueDynamicID = entity.DefaultValueDynamicID,
            MinDynamicID = entity.MinDynamicID,
            MaxDynamicID = entity.MaxDynamicID,
        };
    }

    public TableSchemaPropertyTable Map(EntitySchemaPropertyTable entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleDynamicID = entity.TitleDynamicID,
            DescriptionDynamicID = entity.DescriptionDynamicID,
        };
    }

    #endregion -- Schemas --
    
    #region -- Templates --

    public TableSchema Map(EntitySchema entity)
    {
        return new()
        {
            ID = entity.ID,
            Name = entity.Name,
            Note = entity.Note,
            Order = entity.Order,
            IsActive = entity.IsActive,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            TimeActivated = entity.TimeActivated
        };
    }

    #endregion -- Templates --
}
