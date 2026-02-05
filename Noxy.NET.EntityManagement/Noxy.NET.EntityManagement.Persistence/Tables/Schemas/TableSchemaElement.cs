using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaElement))]
public class TableSchemaElement : BaseTableSchemaComponent
{
    public ICollection<TableJunctionSchemaElementHasElement>? ElementList { get; set; }
    public ICollection<TableJunctionSchemaElementHasProperty>? PropertyList { get; set; }


    public ICollection<TableJunctionSchemaContextHasElement>? RelationContextList { get; set; }
    public ICollection<TableJunctionSchemaElementHasElement>? RelationElementList { get; set; }
}
