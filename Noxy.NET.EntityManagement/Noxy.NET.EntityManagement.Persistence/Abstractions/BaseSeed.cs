using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Enums;
using Noxy.NET.EntityManagement.Persistence.Tables.Data;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.EntityManagement.Persistence.Abstractions;

public class BaseSeed(ModelBuilder builder, TableSchema refSchema)
{
    public static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly Dictionary<Type, int> CollectionOrder = [];

    protected ModelBuilder Builder { get; set; } = builder;
    protected TableSchema Schema { get; set; } = refSchema;

    public static TableSchema CreateSchema(string id, string name, string note = "", bool isActive = false, DateTime? timeActivated = null, DateTime? timeCreated = null)
    {
        return new()
        {
            ID = Guid.Parse(id),
            Name = name,
            Note = note,
            Order = GetNextOrder<TableSchema>(),
            IsActive = isActive,
            TimeActivated = timeActivated ?? Now,
            TimeCreated = timeCreated ?? Now
        };
    }

    protected TableSchemaParameterText HasSchemaParameterText(string id, string identifier, string name = "", string note = "", TextParameterTypeEnum type = TextParameterTypeEnum.Line, bool isApprovalRequired = false,
        DateTime? timeCreated = null)
    {
        TableSchemaParameterText table = new()
        {
            ID = Guid.Parse(id),
            SchemaIdentifier = identifier,
            Name = !string.IsNullOrEmpty(name) ? name : identifier,
            Note = note,
            Order = GetNextOrder<TableSchemaParameterText>(),
            Type = type,
            IsSystemDefined = true,
            IsApprovalRequired = isApprovalRequired,
            TimeCreated = timeCreated ?? Now,
            SchemaID = Schema.ID
        };
        Builder.Entity<TableSchemaParameterText>().HasData(table);
        return table;
    }

    protected TableDataParameterText HasDataParameterText(string id, string identifier, string value, DateTime? timeApproved = null, DateTime? timeEffective = null, DateTime? timeCreated = null)
    {
        TableDataParameterText table = new()
        {
            ID = Guid.Parse(id),
            SchemaIdentifier = identifier,
            Value = value,
            TimeApproved = timeApproved ?? Now,
            TimeEffective = timeEffective ?? Now,
            TimeCreated = timeCreated ?? Now
        };
        Builder.Entity<TableDataParameterText>().HasData(table);
        return table;
    }

    protected static int GetNextOrder<T>()
    {
        Type type = typeof(T);
        if (!CollectionOrder.TryGetValue(type, out int value))
        {
            CollectionOrder[type] = value = 0;
        }

        return CollectionOrder[type] = value + 1;
    }
}
