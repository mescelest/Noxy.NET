using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Data;

public class TableDataPropertyString : TableDataProperty
{
    public required string? Value { get; set; }
}
