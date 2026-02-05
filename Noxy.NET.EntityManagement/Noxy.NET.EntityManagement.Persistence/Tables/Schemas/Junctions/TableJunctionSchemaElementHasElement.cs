using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

[Table(nameof(TableJunctionSchemaElementHasElement))]
public class TableJunctionSchemaElementHasElement : BaseTableManyToMany<TableSchemaElement, TableSchemaElement>
{
    [Required]
    public required int Order { get; set; }
}
