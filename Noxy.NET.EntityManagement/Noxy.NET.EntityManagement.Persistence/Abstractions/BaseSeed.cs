using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.EntityManagement.Persistence.Abstractions;

public class BaseSeed(ModelBuilder builder, TableSchema refSchema)
{
    public static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    protected ModelBuilder Builder { get; set; } = builder;
    protected TableSchema Schema { get; set; } = refSchema;
}
