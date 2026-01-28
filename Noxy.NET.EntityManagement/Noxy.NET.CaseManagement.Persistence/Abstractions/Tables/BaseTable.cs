using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;

namespace Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;

[PrimaryKey(nameof(ID))]
[Index(nameof(TimeCreated))]
public abstract class BaseTable
{
    public Guid ID { get; init; } = BaseEntity.CreateID();

    [Required]
    public DateTime TimeCreated { get; set; } = DateTime.UtcNow;

    public DateTime? TimeUpdated { get; set; }
}
