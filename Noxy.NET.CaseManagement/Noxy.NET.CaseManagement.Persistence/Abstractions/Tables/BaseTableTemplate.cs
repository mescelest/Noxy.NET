using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;

public abstract class BaseTableTemplate : BaseTable
{
    [Required]
    [MaxLength(128)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(1024)]
    public required string Note { get; set; }

    [Required]
    public required int Order { get; set; }
}
