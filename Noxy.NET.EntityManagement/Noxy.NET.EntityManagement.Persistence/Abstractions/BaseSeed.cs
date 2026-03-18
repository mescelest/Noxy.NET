using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Enums;
using Noxy.NET.EntityManagement.Persistence.Tables.Data;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;
using Noxy.NET.Extensions;

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
            Description = new(name, note),
            IsActive = isActive,
            TimeActivated = timeActivated ?? Now,
            TimeCreated = timeCreated ?? Now
        };
    }

    protected void RegisterText(string constant, string value, string culture = "en", TextParameterTypeEnum type = TextParameterTypeEnum.Line, bool isApprovalRequired = false)
    {
        TableSchemaParameterText tableParameterText = new()
        {
            ID = constant.ToDeterministicGuid(),
            SchemaIdentifier = constant,
            Description = new(constant, ""),
            Ordering = new(GetNextOrder<TableSchemaParameterText>()),
            Type = type,
            IsSystemDefined = true,
            IsApprovalRequired = isApprovalRequired,
            TimeCreated = Now,
            SchemaID = Schema.ID
        };
        Builder.Entity<TableSchemaParameterText>().HasData(tableParameterText);

        TableDataParameterText tableParameterValue = new()
        {
            ID = $"{constant}:Value".ToDeterministicGuid(),
            SchemaIdentifier = constant,
            Culture = culture,
            Value = value,
            TimeApproved = Now,
            TimeEffective = Now,
            TimeCreated = Now
        };
        Builder.Entity<TableDataParameterText>().HasData(tableParameterValue);
    }

    protected TableSchemaParameterText HasSchemaParameterText(string constant, string name = "", string note = "", TextParameterTypeEnum type = TextParameterTypeEnum.Line, bool isApprovalRequired = false, DateTime? timeCreated = null)
    {
        TableSchemaParameterText table = new()
        {
            ID = constant.ToDeterministicGuid(),
            SchemaIdentifier = constant,
            Description = new(!string.IsNullOrEmpty(name) ? name : constant, note),
            Ordering = new(GetNextOrder<TableSchema>()),
            Type = type,
            IsSystemDefined = true,
            IsApprovalRequired = isApprovalRequired,
            TimeCreated = timeCreated ?? Now,
            SchemaID = Schema.ID
        };
        Builder.Entity<TableSchemaParameterText>().HasData(table);
        return table;
    }

    protected TableDataParameterText AddDataParameterText(string constant, string value, string culture = "en", DateTime? timeApproved = null, DateTime? timeEffective = null, DateTime? timeCreated = null)
    {
        return new()
        {
            ID = Guid.NewGuid(),
            SchemaIdentifier = constant,
            Culture = culture,
            Value = value,
            TimeApproved = timeApproved ?? Now,
            TimeEffective = timeEffective ?? Now,
            TimeCreated = timeCreated ?? Now
        };
    }

    protected TableDataParameterText HasDataParameterText(string constant, string value, string culture = "en", DateTime? timeApproved = null, DateTime? timeEffective = null, DateTime? timeCreated = null)
    {
        TableDataParameterText table = new()
        {
            ID = constant.ToDeterministicGuid(),
            SchemaIdentifier = constant,
            Culture = culture,
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
