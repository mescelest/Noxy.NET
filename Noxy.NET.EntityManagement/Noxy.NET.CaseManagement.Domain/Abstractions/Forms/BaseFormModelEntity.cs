using System.ComponentModel.DataAnnotations;
using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.CaseManagement.Domain.Abstractions.Forms;

public abstract class BaseFormModelEntity(BaseEntity? entity = null) : BaseFormAPIModel
{
    [Required]
    public Guid ID { get; set; } = entity?.ID ?? Guid.Empty;
    
    public abstract class RelationOrdinal
    {
        [Required]
        public Guid ID { get; set; }
        
        [Required]
        public Guid RelationID { get; set; }
        
        [Required]
        public int Order { get; set; }
    }

}
