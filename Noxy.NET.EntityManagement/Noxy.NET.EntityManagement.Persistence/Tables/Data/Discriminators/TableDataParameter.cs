using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

namespace Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;

[Index(nameof(TimeApproved))]
public abstract class TableDataParameter : BaseTableData
{
    [Required]
    public required string Value { get; set; }

    public required DateTime? TimeApproved { get; set; }

    [Required]
    public required DateTime TimeEffective { get; set; }
}
