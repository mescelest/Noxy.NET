using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

[Table(nameof(TableJunctionSchemaContextHasElement))]
public class TableJunctionSchemaContextHasElement : BaseTableManyToMany<TableSchemaContext, TableSchemaElement>
{
    public required FeatureOrdering Ordering { get; set; }
}
