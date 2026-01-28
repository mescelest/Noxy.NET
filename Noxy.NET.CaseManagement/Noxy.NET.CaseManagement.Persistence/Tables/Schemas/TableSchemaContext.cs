using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchemaContext))]
[Index(nameof(SchemaID), nameof(SchemaIdentifier), IsUnique = true)]
public class TableSchemaContext : BaseTableSchemaComponent
{
    public ICollection<TableJunctionSchemaContextHasAction>? ActionList { get; set; }
    public ICollection<TableJunctionSchemaContextHasElement>? ElementList { get; set; }
}
