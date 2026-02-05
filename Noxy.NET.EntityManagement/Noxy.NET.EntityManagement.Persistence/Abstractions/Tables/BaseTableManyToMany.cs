using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Noxy.NET.EntityManagement.Persistence.Abstractions.Tables;

[Index(nameof(EntityID), nameof(RelationID), IsUnique = true)]
public abstract class BaseTableManyToMany<TEntity, TRelation> : BaseTable
{
    [Required]
    public TEntity? Entity { get; set; }
    public Guid EntityID { get; set; }

    [Required]
    public TRelation? Relation { get; set; }
    public Guid RelationID { get; set; }
}
