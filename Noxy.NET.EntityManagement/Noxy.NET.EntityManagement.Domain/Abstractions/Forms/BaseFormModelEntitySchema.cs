using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Attributes;
using Noxy.NET.EntityManagement.Domain.Constants;

namespace Noxy.NET.EntityManagement.Domain.Abstractions.Forms;

public abstract class BaseFormModelEntitySchema(BaseEntitySchema? entity = null) : BaseFormModelEntityTemplate(entity)
{
    [Required]
    [MinLength(3), MaxLength(64)]
    [IdentifierValidation]
    [DisplayName(TextConstants.LabelFormSchemaIdentifier)]
    [Description(TextConstants.HelpFormSchemaIdentifier)]
    public string SchemaIdentifier { get; set; } = entity?.SchemaIdentifier ?? string.Empty;

    [Required]
    public required Guid SchemaID { get; set; }
}
