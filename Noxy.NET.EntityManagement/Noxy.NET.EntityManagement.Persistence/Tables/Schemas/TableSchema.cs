using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

[Table(nameof(TableSchema))]
public class TableSchema : BaseTableTemplate
{
    [Required]
    public required bool IsActive { get; set; }
    public required DateTime? TimeActivated { get; set; }

    public ICollection<TableSchemaContext>? ContextList { get; set; }
    public ICollection<TableSchemaParameter>? ParameterList { get; set; }
    public ICollection<TableSchemaElement>? ElementList { get; set; }
    public ICollection<TableSchemaProperty>? PropertyList { get; set; }
}
