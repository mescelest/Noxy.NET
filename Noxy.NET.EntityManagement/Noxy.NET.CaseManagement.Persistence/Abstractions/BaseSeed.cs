using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Domain.Enums;
using Noxy.NET.CaseManagement.Persistence.Tables.Data;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.CaseManagement.Persistence.Abstractions;

public class BaseSeed(ModelBuilder builder, TableSchema refSchema)
{
    public static readonly DateTime Now = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    
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

    protected TableSchemaDynamicValueTextParameter HasSchemaDynamicValueTextParameter(string id, string identifier, string name = "", string note = "", TextParameterTypeEnum type = TextParameterTypeEnum.Line, bool isApprovalRequired = false, DateTime? timeCreated = null)
    {
        TableSchemaDynamicValueTextParameter table = new()
        {
            ID = Guid.Parse(id),
            SchemaIdentifier = identifier,
            Name = !string.IsNullOrEmpty(name) ? name : identifier,
            Note = note,
            Order = GetNextOrder<TableSchemaDynamicValueTextParameter>(),
            Type = type,
            IsApprovalRequired = isApprovalRequired,
            TimeCreated = timeCreated ?? Now,
            SchemaID = Schema.ID
        };
        Builder.Entity<TableSchemaDynamicValueTextParameter>().HasData(table);
        return table;
    }

    protected TableDataTextParameter HasTableDataTextParameter(string id, string identifier, string value, DateTime? timeApproved = null, DateTime? timeEffective = null, DateTime? timeCreated = null)
    {
        TableDataTextParameter table = new()
        {
            ID = Guid.Parse(id),
            SchemaIdentifier = identifier,
            Value = value,
            TimeApproved = timeApproved ?? Now,
            TimeEffective = timeEffective ?? Now,
            TimeCreated = timeCreated ?? Now
        };
        Builder.Entity<TableDataTextParameter>().HasData(table);
        return table;
    }

    protected TableSchemaInput HasTableSchemaInput(string id, string identifier, string name = "", string note = "", DateTime? timeCreated = null)
    {
        TableSchemaInput table = new()
        {
            ID = Guid.Parse(id),
            SchemaIdentifier = identifier,
            Name = !string.IsNullOrWhiteSpace(name) ? name : identifier,
            Note = note,
            Order = GetNextOrder<TableSchemaInput>(),
            TimeCreated = timeCreated ?? Now,
            SchemaID = Schema.ID
        };
        Builder.Entity<TableSchemaInput>().HasData(table);
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
