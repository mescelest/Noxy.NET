using Noxy.NET.EntityManagement.Domain.Enums;
using Noxy.NET.EntityManagement.Persistence;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Database.Builders;

public class SchemaSeedBuilder(DataContext context, TableSchema schema)
{
    public DateTime Now { get; } = DateTime.UtcNow;
    private Dictionary<string, int> OrderCollection { get; } = [];

    public static TableSchema CreateSchema(string name, string note = "", int order = 1, bool isActive = false, DateTime? timeActivated = null, DateTime? timeCreated = null)
    {
        return new()
        {
            Name = name,
            Note = note,
            Order = order,
            IsActive = isActive,
            TimeActivated = timeActivated,
            TimeCreated = timeCreated ?? DateTime.UtcNow
        };
    }

    public TableSchemaContext AddContext(string identifier, string name, string note = "", DateTime? timeCreated = null)
    {
        return context.SchemaContext.Add(new()
        {
            SchemaIdentifier = identifier,
            Name = name,
            Note = note,
            Order = GetNextOrder(nameof(TableSchemaContext)),
            SchemaID = schema.ID,
            TimeCreated = timeCreated ?? Now
        }).Entity;
    }

    public TableSchemaParameterSystem AddDynamicValueSystemParameter(string identifier, string name, string note = "", bool isApprovalRequired = false, DateTime? timeCreated = null)
    {
        return context.SchemaParameterSystem.Add(new()
        {
            SchemaIdentifier = identifier,
            Name = name,
            Note = note,
            Order = GetNextOrder(nameof(TableSchemaParameterText)),
            IsApprovalRequired = isApprovalRequired,
            SchemaID = schema.ID,
            TimeCreated = timeCreated ?? Now
        }).Entity;
    }

    public TableSchemaParameterText AddDynamicValueTextParameter(string identifier, string name, TextParameterTypeEnum type = TextParameterTypeEnum.Line, string note = "", bool isApprovalRequired = false, DateTime? timeCreated = null)
    {
        return context.SchemaParameterText.Add(new()
        {
            SchemaIdentifier = identifier,
            Name = name,
            Note = note,
            Order = GetNextOrder(nameof(TableSchemaParameterText)),
            Type = type,
            IsApprovalRequired = isApprovalRequired,
            SchemaID = schema.ID,
            TimeCreated = timeCreated ?? Now
        }).Entity;
    }

    public TableSchemaElement AddElement(string identifier, string name, string note = "", DateTime? timeCreated = null)
    {
        return context.SchemaElement.Add(new()
        {
            SchemaIdentifier = identifier,
            Name = name,
            Note = note,
            Order = GetNextOrder(nameof(TableSchemaElement)),
            SchemaID = schema.ID,
            TimeCreated = timeCreated ?? Now
        }).Entity;
    }

    public TableSchemaPropertyBoolean AddPropertyBoolean(string identifier, string name, string note = "", string @default = "", DateTime? timeCreated = null)
    {
        return context.SchemaPropertyBoolean.Add(new()
        {
            SchemaIdentifier = identifier,
            Name = name,
            Note = note,
            Order = GetNextOrder(nameof(TableSchemaProperty)),
            SchemaID = schema.ID,
            TimeCreated = timeCreated ?? Now
        }).Entity;
    }

    public TableSchemaPropertyDateTime AddPropertyDateTime(string identifier, string name, string note = "", DateTimeTypeEnum type = DateTimeTypeEnum.Date, string @default = "",
        DateTime? timeCreated = null)
    {
        return context.SchemaPropertyDateTime.Add(new()
        {
            SchemaIdentifier = identifier,
            Name = name,
            Note = note,
            Order = GetNextOrder(nameof(TableSchemaProperty)),
            Type = type,
            SchemaID = schema.ID,
            TimeCreated = timeCreated ?? Now
        }).Entity;
    }

    public TableSchemaPropertyString AddPropertyString(string identifier, string name, string note = "", string @default = "", DateTime? timeCreated = null)
    {
        return context.SchemaPropertyString.Add(new()
        {
            SchemaIdentifier = identifier,
            Name = name,
            Note = note,
            Order = GetNextOrder(nameof(TableSchemaProperty)),
            SchemaID = schema.ID,
            TimeCreated = timeCreated ?? Now
        }).Entity;
    }

    public void Relate(TableSchemaContext refContext, TableSchemaElement refElement, DateTime? timeCreated = null)
    {
        context.SchemaContextHasElement.Add(new()
        {
            Order = GetNextOrder(nameof(TableSchemaContext) + nameof(TableSchemaElement)),
            Entity = refContext,
            EntityID = refContext.ID,
            Relation = refElement,
            RelationID = refElement.ID,
            TimeCreated = timeCreated ?? Now
        });
    }

    public void Relate(TableSchemaElement refElement, TableSchemaProperty refProperty, DateTime? timeCreated = null)
    {
        context.SchemaElementHasProperty.Add(new()
        {
            Entity = refElement,
            EntityID = refElement.ID,
            Relation = refProperty,
            RelationID = refProperty.ID,
            Order = GetNextOrder(nameof(TableSchemaElement) + nameof(TableSchemaProperty)),
            TimeCreated = timeCreated ?? Now
        });
    }

    private int GetNextOrder(string identifier)
    {
        if (!OrderCollection.TryGetValue(identifier, out int value))
        {
            OrderCollection[identifier] = value = 0;
        }

        return OrderCollection[identifier] = value + 1;
    }
}
