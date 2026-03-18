using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

[Table(nameof(TableJunctionSchemaElementHasProperty))]
public class TableJunctionSchemaElementHasProperty : BaseTableManyToMany<TableSchemaElement, TableSchemaProperty>
{
    public required FeatureOrdering Ordering { get; set; }
}
