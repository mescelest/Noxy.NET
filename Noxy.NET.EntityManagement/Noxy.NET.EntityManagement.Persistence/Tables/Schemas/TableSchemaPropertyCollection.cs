using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

public class TableSchemaPropertyCollection : TableSchemaProperty
{
    public ICollection<TableJunctionSchemaPropertyCollectionHasProperty>? PropertyList { get; set; }
}
