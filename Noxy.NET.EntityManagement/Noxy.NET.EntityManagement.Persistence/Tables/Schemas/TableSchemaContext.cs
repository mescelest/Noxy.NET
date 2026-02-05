using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaContext))]
public class TableSchemaContext : BaseTableSchemaComponent
{
    public ICollection<TableJunctionSchemaContextHasElement>? ElementList { get; set; }
}
