using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaAction))]
[Index(nameof(SchemaID), nameof(SchemaIdentifier), IsUnique = true)]
public class TableSchemaAction : BaseTableSchemaComponent
{
    public TableSchemaDynamicValueCode? ActiveDynamic { get; set; }
    public Guid? IsActiveDynamicID { get; set; }
    
    public ICollection<TableJunctionSchemaActionHasActionStep>? ActionStepList { get; set; }
    public ICollection<TableJunctionSchemaActionHasDynamicValueCode>? DynamicValueCodeList { get; set; }
    
    public ICollection<TableJunctionSchemaContextHasAction>? RelationContextList { get; set; }
}
