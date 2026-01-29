using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Attributes;
using Noxy.NET.EntityManagement.Domain.Constants;

namespace Noxy.NET.EntityManagement.Domain.Abstractions.Forms;

public abstract class BaseFormModelEntitySchemaComponent(BaseEntitySchemaComponent? entity) : BaseFormModelEntitySchema(entity)
{
    [Required]
    [NotEmpty]
    [DisplayName(TextConstants.LabelFormTitle)]
    [Description(TextConstants.HelpFormTitle)]
    public Guid TitleTextParameterID { get; set; } = entity?.TitleTextParameterID ?? Guid.NewGuid();

    [Required]
    [NotEmpty]
    [DisplayName(TextConstants.LabelFormDescription)]
    [Description(TextConstants.HelpFormDescription)]
    public Guid DescriptionTextParameterID { get; set; } = entity?.DescriptionTextParameterID ?? Guid.NewGuid();
}
