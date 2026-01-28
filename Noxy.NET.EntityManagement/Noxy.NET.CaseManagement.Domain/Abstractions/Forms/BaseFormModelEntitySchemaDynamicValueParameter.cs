using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.CaseManagement.Domain.Constants;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.CaseManagement.Domain.Abstractions.Forms;

public abstract class BaseFormModelEntitySchemaDynamicValueParameter(EntitySchemaDynamicValueParameter? entity = null) : BaseFormModelEntitySchema(entity)
{
    [Required]
    [DisplayName(TextConstants.LabelFormIsApprovalRequired)]
    [Description(TextConstants.HelpFormIsApprovalRequired)]
    public bool IsApprovalRequired { get; set; } = entity?.IsApprovalRequired ?? false;
}
