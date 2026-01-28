using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaElement))]
[Index(nameof(SchemaID), nameof(SchemaIdentifier), IsUnique = true)]
public class TableSchemaElement : BaseTableSchemaComponent
{
    public ICollection<TableJunctionSchemaElementHasAction>? ContextList { get; set; }
    public ICollection<TableJunctionSchemaElementHasElement>? ElementList { get; set; }
    public ICollection<TableJunctionSchemaElementHasProperty>? PropertyList { get; set; }
    
    
    public ICollection<TableJunctionSchemaContextHasElement>? RelationContextList { get; set; }
    public ICollection<TableJunctionSchemaElementHasElement>? RelationElementList { get; set; }
}
