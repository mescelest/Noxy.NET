using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaActionInput))]
[Index(nameof(SchemaID), nameof(SchemaIdentifier), IsUnique = true)]
public class TableSchemaActionInput : BaseTableSchemaComponent
{
    public TableSchemaDynamicValueCode? ActiveDynamic { get; set; }
    public Guid? IsActiveDynamicID { get; set; }
    
    public TableSchemaDynamicValueCode? ValidDynamic { get; set; }
    public Guid? IsValidDynamicID { get; set; }
    
    [Required]
    public TableSchemaInput? Input { get; set; }
    public required Guid InputID { get; set; }

    public ICollection<TableAssociationSchemaActionInputHasAttribute>? AttributeList { get; set; }

    public ICollection<TableJunctionSchemaActionStepHasActionInput>? RelationActionStepList { get; set; }
}
