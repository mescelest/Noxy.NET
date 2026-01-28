using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Junctions;

[Table(nameof(TableJunctionSchemaContextHasElement))]
[Index(nameof(EntityID), nameof(RelationID), IsUnique = true)]
public class TableJunctionSchemaContextHasElement : BaseTableManyToMany<TableSchemaContext, TableSchemaElement>
{
    [Required]
    public required int Order { get; set; }
}
