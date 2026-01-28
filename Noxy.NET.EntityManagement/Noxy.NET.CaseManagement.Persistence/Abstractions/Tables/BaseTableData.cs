using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;

[PrimaryKey(nameof(ID))]
[Index(nameof(TimeCreated))]
public abstract class BaseTableData : BaseTable
{
    [Required]
    [StringLength(64)]
    public required string SchemaIdentifier { get; set; }
}
