using Noxy.NET.EntityManagement.Domain.Entities.Authentication;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Persistence.Interfaces.Services;
using Noxy.NET.EntityManagement.Persistence.Tables.Authentication;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Services;

public class EntityToTableMapper : IEntityToTableMapper
{
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

    #endregion -- Many-To-Many --

    #region -- Schemas --

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
            Order = entity.Order,
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
            Order = entity.Order,
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
        };
    }

    #endregion -- Schemas --
}
