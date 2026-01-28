using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchema))]
public class TableSchema : BaseTableTemplate
{
    [Required]
    public required bool IsActive { get; set; }
    public required DateTime? TimeActivated { get; set; }

    public ICollection<TableSchemaAction>? ActionList { get; set; }
    public ICollection<TableSchemaActionInput>? ActionInputList { get; set; }
    public ICollection<TableSchemaActionStep>? ActionStepList { get; set; }
    public ICollection<TableSchemaAttribute>? AttributeList { get; set; }
    public ICollection<TableSchemaContext>? ContextList { get; set; }
    public ICollection<TableSchemaDynamicValue>? DynamicValueList { get; set; }
    public ICollection<TableSchemaElement>? ElementList { get; set; }
    public ICollection<TableSchemaInput>? InputList { get; set; }
    public ICollection<TableSchemaProperty>? PropertyList { get; set; }
}
