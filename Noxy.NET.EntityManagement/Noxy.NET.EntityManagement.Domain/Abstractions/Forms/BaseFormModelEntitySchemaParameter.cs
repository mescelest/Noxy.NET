using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Abstractions.Forms;

public abstract class BaseFormModelEntitySchemaParameter(EntitySchemaParameter? entity = null) : BaseFormModelEntitySchema(entity)
{
    [Required]
    [DisplayName(TextConstants.LabelFormIsApprovalRequired)]
    [Description(TextConstants.HelpFormIsApprovalRequired)]
    public bool IsApprovalRequired { get; set; } = entity?.IsApprovalRequired ?? false;
}
