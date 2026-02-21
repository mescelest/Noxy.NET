using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

public abstract class TableSchemaProperty : BaseTableSchemaComponent
{
    public ICollection<TableJunctionSchemaElementHasProperty>? RelationElementList { get; set; }
    public ICollection<TableJunctionSchemaPropertyCollectionHasProperty>? RelationPropertyCollectionList { get; set; }
    public ICollection<TableJunctionSchemaPropertyTableHasProperty>? RelationPropertyTableList { get; set; }

    public abstract class Primitive : TableSchemaProperty;
}
