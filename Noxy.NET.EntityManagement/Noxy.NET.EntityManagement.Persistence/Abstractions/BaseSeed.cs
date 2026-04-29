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
            Name = name,
            Note = note,
            IsActive = isActive,
            TimeActivated = timeActivated ?? Now,
            TimeCreated = timeCreated ?? Now
        };
    }

    protected void RegisterText(string constant, string value, string culture = "en", ParameterTextTypeEnum type = ParameterTextTypeEnum.Line, bool isApprovalRequired = false)
    {
        TableSchemaParameterText tableParameterText = new()
        {
            ID = constant.ToDeterministicGuid(),
            SchemaIdentifier = constant,
            Name = constant,
            Note = "",
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
