using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Attributes;
using Noxy.NET.CaseManagement.Domain.Constants;

namespace Noxy.NET.CaseManagement.Domain.Abstractions.Forms;

public abstract class BaseFormModelEntitySchemaComponent(BaseEntitySchemaComponent? entity) : BaseFormModelEntitySchema(entity)
{
    [Required]
    [NotEmpty]
    [DisplayName(TextConstants.LabelFormTitle)]
    [Description(TextConstants.HelpFormTitle)]
    public Guid TitleDynamicID { get; set; } = entity?.TitleDynamicID ?? Guid.NewGuid();

    [Required]
    [NotEmpty]
    [DisplayName(TextConstants.LabelFormDescription)]
    [Description(TextConstants.HelpFormDescription)]
    public Guid DescriptionDynamicID { get; set; } = entity?.TitleDynamicID ?? Guid.NewGuid();
}
