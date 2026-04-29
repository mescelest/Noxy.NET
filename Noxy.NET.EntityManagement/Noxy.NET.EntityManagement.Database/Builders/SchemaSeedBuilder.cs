using Noxy.NET.EntityManagement.Domain.Enums;
using Noxy.NET.EntityManagement.Persistence;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Database.Builders;

public class SchemaSeedBuilder(DataContext context, TableSchema schema)
{
    public DateTime Now { get; } = DateTime.UtcNow;

    public static TableSchema CreateSchema(string name, string note = "", bool isActive = false, DateTime? timeActivated = null, DateTime? timeCreated = null)
    {
        return new()
        {
            Name = name,
            Note = note,
            IsActive = isActive,
            TimeActivated = timeActivated,
            TimeCreated = timeCreated ?? DateTime.UtcNow
        };
    }

    public TableSchemaContext AddContext(string identifier, string name, TableSchemaParameterText title, string note = "", TableSchemaParameterText? description = null, DateTime? timeCreated = null)
    {
        return context.SchemaContext.Add(new()
        {
            SchemaIdentifier = identifier,
            Name = name,
            Note = note,
            SchemaID = schema.ID,
            TitleTextParameterID = title.ID,
            DescriptionTextParameterID = description?.ID,
            TimeCreated = timeCreated ?? Now
        }).Entity;
    }

    public TableSchemaElement AddElement(string identifier, string name, TableSchemaParameterText title, string note = "", TableSchemaParameterText? description = null, int weight = 0, DateTime? timeCreated = null)
    {
        return context.SchemaElement.Add(new()
        {
            SchemaIdentifier = identifier,
            Name = name,
            Note = note,
            Weight = weight,
            SchemaID = schema.ID,
            TitleTextParameterID = title.ID,
            DescriptionTextParameterID = description?.ID,
            TimeCreated = timeCreated ?? Now
        }).Entity;
    }

    public TableSchemaParameterStyle AddParameterStyle(string identifier, string name, string note = "", bool isSystemDefined = false, bool isApprovalRequired = false, DateTime? timeCreated = null)
    {
        return context.SchemaParameterStyle.Add(new()
        {
            SchemaIdentifier = identifier,
            Name = name,
            Note = note,
            IsSystemDefined = isSystemDefined,
            IsApprovalRequired = isApprovalRequired,
            SchemaID = schema.ID,
            TimeCreated = timeCreated ?? Now
        }).Entity;
    }

    public TableSchemaParameterSystem AddParameterSystem(string identifier, string name, string note = "", bool isSystemDefined = false, bool isApprovalRequired = false, DateTime? timeCreated = null)
    {
        return context.SchemaParameterSystem.Add(new()
        {
            SchemaIdentifier = identifier,
            Name = name,
            Note = note,
            IsSystemDefined = isSystemDefined,
            IsApprovalRequired = isApprovalRequired,
            SchemaID = schema.ID,
            TimeCreated = timeCreated ?? Now
        }).Entity;
    }

    public TableSchemaParameterText AddParameterText(string identifier, string name, TextParameterTypeEnum type = TextParameterTypeEnum.Line, string note = "", bool isSystemDefined = false, bool isApprovalRequired = false,
        DateTime? timeCreated = null)
    {
        return context.SchemaParameterText.Add(new()
        {
            SchemaIdentifier = identifier,
            Name = name,
            Note = note,
            Type = type,
            IsSystemDefined = isSystemDefined,
            IsApprovalRequired = isApprovalRequired,
            SchemaID = schema.ID,
            TimeCreated = timeCreated ?? Now
        }).Entity;
    }

    public TableSchemaPropertyBoolean AddPropertyBoolean(string identifier, string name, TableSchemaParameterText title, string note = "", TableSchemaParameterText? description = null, int weight = 0, DateTime? timeCreated = null)
    {
        return context.SchemaPropertyBoolean.Add(new()
        {
            SchemaIdentifier = identifier,
            Name = name,
            Note = note,
            Weight = weight,
            SchemaID = schema.ID,
            TitleTextParameterID = title.ID,
            DescriptionTextParameterID = description?.ID,
            TimeCreated = timeCreated ?? Now
        }).Entity;
    }

    public TableSchemaPropertyDateTime AddPropertyDateTime(string identifier, string name, TableSchemaParameterText title, string note = "", TableSchemaParameterText? description = null, int weight = 0, DateTimeTypeEnum type = DateTimeTypeEnum.Date,
        DateTime? timeCreated = null)
    {
        return context.SchemaPropertyDateTime.Add(new()
        {
            SchemaIdentifier = identifier,
            Name = name,
            Note = note,
            Weight = weight,
            Type = type,
            SchemaID = schema.ID,
            TitleTextParameterID = title.ID,
            DescriptionTextParameterID = description?.ID,
            TimeCreated = timeCreated ?? Now
        }).Entity;
    }

    public TableSchemaPropertyString AddPropertyString(string identifier, string name, TableSchemaParameterText title, string note = "", TableSchemaParameterText? description = null, int weight = 0, DateTime? timeCreated = null)
    {
        return context.SchemaPropertyString.Add(new()
        {
            SchemaIdentifier = identifier,
            Name = name,
            Note = note,
            Weight = weight,
            SchemaID = schema.ID,
            TitleTextParameterID = title.ID,
            DescriptionTextParameterID = description?.ID,
            TimeCreated = timeCreated ?? Now
        }).Entity;
    }

    public void Relate(TableSchemaContext refContext, TableSchemaElement refElement, int weight = 0, DateTime? timeCreated = null)
    {
        context.SchemaContextHasElement.Add(new()
        {
            Entity = refContext,
            EntityID = refContext.ID,
            Relation = refElement,
            RelationID = refElement.ID,
            TimeCreated = timeCreated ?? Now
        });
    }

    public void Relate(TableSchemaElement refElement, TableSchemaProperty refProperty, int weight = 0, DateTime? timeCreated = null)
    {
        context.SchemaElementHasProperty.Add(new()
        {
            Entity = refElement,
            EntityID = refElement.ID,
            Relation = refProperty,
            RelationID = refProperty.ID,
            TimeCreated = timeCreated ?? Now
        });
    }
}
