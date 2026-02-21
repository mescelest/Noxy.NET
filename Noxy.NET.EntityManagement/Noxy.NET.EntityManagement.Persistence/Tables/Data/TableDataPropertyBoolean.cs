using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Data;

public class TableDataPropertyBoolean : TableDataProperty
{
    public required bool? Value { get; set; }
}
