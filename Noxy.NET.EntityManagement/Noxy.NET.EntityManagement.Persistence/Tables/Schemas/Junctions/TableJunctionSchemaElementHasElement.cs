using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

[Table(nameof(TableJunctionSchemaElementHasElement))]
public class TableJunctionSchemaElementHasElement : BaseTableManyToMany<TableSchemaElement, TableSchemaElement>
{
    public required FeatureOrdering Ordering { get; set; }
}
