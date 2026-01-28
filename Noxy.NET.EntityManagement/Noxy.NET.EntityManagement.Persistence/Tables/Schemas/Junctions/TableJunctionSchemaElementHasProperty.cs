using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

[Table(nameof(TableJunctionSchemaElementHasProperty))]
[Index(nameof(EntityID), nameof(RelationID), IsUnique = true)]
public class TableJunctionSchemaElementHasProperty : BaseTableManyToMany<TableSchemaElement, TableSchemaProperty>
{
    [Required]
    public required int Order { get; set; }
}
