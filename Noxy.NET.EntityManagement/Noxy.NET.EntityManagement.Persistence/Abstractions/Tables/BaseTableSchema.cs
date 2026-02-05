using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;

namespace Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

[Index(nameof(SchemaID), nameof(SchemaIdentifier), IsUnique = true)]
public abstract class BaseTableSchema : BaseTableTemplate
{
    [Required]
    [StringLength(64)]
    public required string SchemaIdentifier { get; set; }

    [Required]
    public TableSchema? Schema { get; set; }
    public required Guid SchemaID { get; set; }
}
