using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.CaseManagement.Persistence.Abstractions.Tables;

public abstract class BaseTableManyToMany<TEntity, TRelation> : BaseTable
{
    [Required]
    public TEntity? Entity { get; set; }
    public Guid EntityID { get; set; }
    
    [Required]
    public TRelation? Relation { get; set; }
    public  Guid RelationID { get; set; }
}
