using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

[Index(nameof(SchemaIdentifier))]
public abstract class BaseTableData : BaseTable
{
    [Required]
    [StringLength(64)]
    public required string SchemaIdentifier { get; set; }
}
