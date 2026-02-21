using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Data;

public class TableDataPropertyDateTime : TableDataProperty
{
    public required DateTime? Value { get; set; }
}
