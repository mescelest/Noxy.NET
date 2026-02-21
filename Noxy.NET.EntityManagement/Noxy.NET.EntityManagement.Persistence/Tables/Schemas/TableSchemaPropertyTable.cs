using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

public class TableSchemaPropertyTable : TableSchemaProperty
{
    public ICollection<TableJunctionSchemaPropertyTableHasProperty>? PropertyList { get; set; }
}
