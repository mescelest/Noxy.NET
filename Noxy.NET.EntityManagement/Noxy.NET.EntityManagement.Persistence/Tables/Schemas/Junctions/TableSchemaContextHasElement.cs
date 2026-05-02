using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

[Table(nameof(TableSchemaContextHasElement))]
public class TableSchemaContextHasElement : BaseTableManyToMany<TableSchemaContext, TableSchemaElement>
{
    public TableSchemaContextHasElement Clone(Guid? entityID = null, Guid? relationID = null)
    {
        return new()
        {
            EntityID = entityID ?? EntityID,
            RelationID = relationID ?? RelationID,
        };
    }
}
