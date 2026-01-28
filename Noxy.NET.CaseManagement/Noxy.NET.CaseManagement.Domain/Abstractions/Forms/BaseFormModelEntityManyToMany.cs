using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Attributes;

namespace Noxy.NET.CaseManagement.Domain.Abstractions.Forms;

public abstract class BaseFormModelEntityManyToMany<TEntity, TRelation> : BaseFormModelEntity  where TEntity : BaseEntity where TRelation : BaseEntity
{
    [Required]
    [NotEmpty]
    public required Guid RelationID { get; set; }

    [Required]
    [NotEmpty]
    public Guid EntityID { get; set; }

    protected BaseFormModelEntityManyToMany(TEntity? entity = null, TRelation? relation = null) 
    {
        EntityID = entity?.ID ?? Guid.Empty;
        RelationID = relation?.ID ?? Guid.Empty;
    }
    
    [SetsRequiredMembers]
    protected BaseFormModelEntityManyToMany(BaseEntityManyToMany<TEntity, TRelation>? entity = null) : base(entity)
    {
        EntityID = entity?.EntityID ?? Guid.Empty;
        RelationID = entity?.RelationID ?? Guid.Empty;
    }
    
    [JsonConstructor]
    protected BaseFormModelEntityManyToMany()
    {
        
    }
}
