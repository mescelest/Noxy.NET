using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Junctions;

[Table(nameof(TableJunctionSchemaPropertyTableHasProperty))]
[Index(nameof(EntityID), nameof(RelationID), IsUnique = true)]
public class TableJunctionSchemaPropertyTableHasProperty : BaseTableManyToMany<TableSchemaPropertyTable, TableSchemaProperty>
{
    [Required]
    public required int Order { get; set; }
}
