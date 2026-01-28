using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Constants;

namespace Noxy.NET.EntityManagement.Domain.Abstractions.Forms;

public abstract class BaseFormModelEntityTemplate(BaseEntityTemplate? entity) : BaseFormModelEntity(entity)
{
    [Required]
    [MinLength(3), MaxLength(64)]
    [DisplayName(TextConstants.LabelFormName)]
    [Description(TextConstants.HelpFormName)]
    public string Name { get; set; } = entity?.Name ?? string.Empty;

    [MaxLength(1024)]
    [DisplayName(TextConstants.LabelFormNote)]
    [Description(TextConstants.HelpFormNote)]
    public string Note { get; set; } = entity?.Note ?? string.Empty;

    [Range(0, int.MaxValue)]
    [DisplayName(TextConstants.LabelFormOrder)]
    [Description(TextConstants.HelpFormOrder)]
    public int Order { get; set; } = entity?.Order ?? 0;
}
