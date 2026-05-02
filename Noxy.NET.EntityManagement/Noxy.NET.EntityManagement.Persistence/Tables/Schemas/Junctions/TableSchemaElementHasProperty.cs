using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

[Table(nameof(TableSchemaElementHasProperty))]
public class TableSchemaElementHasProperty : BaseTableManyToMany<TableSchemaElement, TableSchemaProperty>
{
    public TableSchemaElementHasProperty Clone(Guid? entityID = null, Guid? relationID = null)
    {
        return new()
        {
            EntityID = entityID ?? EntityID,
            RelationID = relationID ?? RelationID,
        };
    }
}
