using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

[PrimaryKey(nameof(ID))]
[Index(nameof(TimeCreated))]
public abstract class BaseTableDataParameter : BaseTableData
{
    [Required]
    public required string Value { get; set; }

    public required DateTime? TimeApproved { get; set; }

    [Required]
    public required DateTime TimeEffective { get; set; }
}
