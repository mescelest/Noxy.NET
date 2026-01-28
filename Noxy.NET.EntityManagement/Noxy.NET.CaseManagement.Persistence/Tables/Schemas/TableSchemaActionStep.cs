using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaActionStep))]
[Index(nameof(SchemaID), nameof(SchemaIdentifier), IsUnique = true)]
public class TableSchemaActionStep : BaseTableSchemaComponent
{
    [Required]
    public bool IsRepeatable { get; set; }

    public ICollection<TableJunctionSchemaActionStepHasActionInput>? ActionInputList { get; set; }
    
    public ICollection<TableJunctionSchemaActionHasActionStep>? RelationActionList { get; set; }
}
