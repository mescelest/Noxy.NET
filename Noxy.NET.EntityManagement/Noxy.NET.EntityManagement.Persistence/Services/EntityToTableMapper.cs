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

    #region -- Data --

    public TableDataElement Map(EntityDataElement entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            TimeUpdated = entity.TimeUpdated,
        };
    }

    public TableDataParameter Map(EntityDataParameter entity)
    {
        return entity switch
        {
            EntityDataParameterStyle value => Map(value),
            EntityDataParameterSystem value => Map(value),
            EntityDataParameterText value => Map(value),
            _ => throw new ArgumentOutOfRangeException(nameof(entity))
        };
    }

    public TableDataParameterStyle Map(EntityDataParameterStyle entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Value = entity.Value,
            TimeApproved = entity.TimeApproved,
            TimeEffective = entity.TimeEffective,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            TimeUpdated = entity.TimeUpdated,
        };
    }

    public TableDataParameterSystem Map(EntityDataParameterSystem entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Value = entity.Value,
            TimeApproved = entity.TimeApproved,
            TimeEffective = entity.TimeEffective,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            TimeUpdated = entity.TimeUpdated,
        };
    }

    public TableDataParameterText Map(EntityDataParameterText entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Culture = entity.Culture,
            Value = entity.Value,
            TimeApproved = entity.TimeApproved,
            TimeEffective = entity.TimeEffective,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            TimeUpdated = entity.TimeUpdated,
        };
    }

    public TableDataProperty Map(EntityDataProperty entity)
    {
        return entity switch
        {
            EntityDataPropertyBoolean value => Map(value),
            EntityDataPropertyDateTime value => Map(value),
            EntityDataPropertyString value => Map(value),
            _ => throw new ArgumentOutOfRangeException(nameof(entity))
        };
    }

    public TableDataPropertyBoolean Map(EntityDataPropertyBoolean entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Value = entity.Value,
            ElementID = entity.ElementID,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            TimeUpdated = entity.TimeUpdated,
        };
    }

    public TableDataPropertyDateTime Map(EntityDataPropertyDateTime entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Value = entity.Value,
            ElementID = entity.ElementID,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            TimeUpdated = entity.TimeUpdated,
        };
    }

    public TableDataPropertyString Map(EntityDataPropertyString entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Value = entity.Value,
            ElementID = entity.ElementID,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            TimeUpdated = entity.TimeUpdated,
        };
    }

    #endregion -- Data --

    #region -- Many-To-Many --

    public TableSchemaContextHasElement Map(EntitySchemaContextHasElement entity)
    {
        return new()
        {
            ID = entity.ID,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            EntityID = entity.EntityID,
            RelationID = entity.RelationID
        };
    }

    public TableSchemaElementHasProperty Map(EntitySchemaElementHasProperty entity)
    {
        return new()
        {
            ID = entity.ID,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            EntityID = entity.EntityID,
            RelationID = entity.RelationID
        };
    }

    #endregion -- Many-To-Many --

    #region -- Schemas --

    public TableSchema Map(EntitySchema entity)
    {
        return new()
        {
            ID = entity.ID,
            Name = entity.Name,
            Note = entity.Note,
            IsActive = entity.IsActive,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            TimeActivated = entity.TimeActivated
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
            TitleParameterTextID = entity.TitleTextParameterID,
            DescriptionParameterTextID = entity.DescriptionTextParameterID,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
        };
    }

    public TableSchemaParameter Map(EntitySchemaParameter entity)
    {
        return entity switch
        {
            EntitySchemaParameterStyle value => Map(value),
            EntitySchemaParameterSystem value => Map(value),
            EntitySchemaParameterText value => Map(value),
            _ => throw new ArgumentOutOfRangeException(nameof(entity))
        };
    }

    public TableSchemaParameterStyle Map(EntitySchemaParameterStyle entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            IsSystemDefined = entity.IsSystemDefined,
            IsApprovalRequired = entity.IsApprovalRequired,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID
        };
    }

    public TableSchemaParameterSystem Map(EntitySchemaParameterSystem entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            Type = entity.Type,
            IsPublic = entity.IsPublic,
            IsSystemDefined = entity.IsSystemDefined,
            IsApprovalRequired = entity.IsApprovalRequired,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID
        };
    }

    public TableSchemaParameterText Map(EntitySchemaParameterText entity)
    {
        return new()
        {
            ID = entity.ID,
            SchemaIdentifier = entity.SchemaIdentifier,
            Name = entity.Name,
            Note = entity.Note,
            IsSystemDefined = entity.IsSystemDefined,
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
            Weight = entity.Weight,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleParameterTextID = entity.TitleParameterTextID,
            DescriptionParameterTextID = entity.DescriptionParameterTextID,
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
            Weight = entity.Weight,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleParameterTextID = entity.TitleParameterTextID,
            DescriptionParameterTextID = entity.DescriptionParameterTextID
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
            Weight = entity.Weight,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleParameterTextID = entity.TitleParameterTextID,
            DescriptionParameterTextID = entity.DescriptionParameterTextID,
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
            Weight = entity.Weight,
            Type = entity.Type,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleParameterTextID = entity.TitleParameterTextID,
            DescriptionParameterTextID = entity.DescriptionParameterTextID,
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
            Weight = entity.Weight,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleParameterTextID = entity.TitleParameterTextID,
            DescriptionParameterTextID = entity.DescriptionParameterTextID,
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
            Weight = entity.Weight,
            AllowedExtensions = entity.AllowedExtensions,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleParameterTextID = entity.TitleParameterTextID,
            DescriptionParameterTextID = entity.DescriptionParameterTextID,
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
            Weight = entity.Weight,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleParameterTextID = entity.TitleParameterTextID,
            DescriptionParameterTextID = entity.DescriptionParameterTextID,
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
            Weight = entity.Weight,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleParameterTextID = entity.TitleParameterTextID,
            DescriptionParameterTextID = entity.DescriptionParameterTextID,
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
            Weight = entity.Weight,
            TimeCreated = entity.TimeCreated ?? DateTime.UtcNow,
            SchemaID = entity.SchemaID,
            TitleParameterTextID = entity.TitleParameterTextID,
            DescriptionParameterTextID = entity.DescriptionParameterTextID,
        };
    }

    #endregion -- Schemas --
}
