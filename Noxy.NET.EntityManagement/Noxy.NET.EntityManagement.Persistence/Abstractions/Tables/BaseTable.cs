using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;

namespace Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

[PrimaryKey(nameof(ID))]
[Index(nameof(TimeCreated))]
public abstract class BaseTable
{
    public Guid ID { get; init; } = BaseEntity.CreateID();

    [Required]
    public DateTime TimeCreated { get; set; } = DateTime.UtcNow;

    public DateTime? TimeUpdated { get; set; }

    public const int DefaultNameLength = 255;
    public const int DefaultNoteLength = 255;

    // For free text of any size. It only gives the column a bound, which is far above anything entered.
    public const int DefaultTextLength = 1_000_000;
}
