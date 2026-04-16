using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Domain.Interfaces;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

[Table(nameof(TableJunctionSchemaContextHasElement))]
public class TableJunctionSchemaContextHasElement : BaseTableManyToMany<TableSchemaContext, TableSchemaElement>, ISchemaOrdering
{
    [Required]
    public required int Order { get; set; }
}
