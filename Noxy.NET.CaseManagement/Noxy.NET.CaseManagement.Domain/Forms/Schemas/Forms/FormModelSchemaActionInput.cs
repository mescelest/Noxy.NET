using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Forms;
using Noxy.NET.CaseManagement.Domain.Constants;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Forms.Schemas.AssociationForms;

namespace Noxy.NET.CaseManagement.Domain.Forms.Schemas.Forms;

public class FormModelSchemaActionInput(EntitySchemaActionInput? entity) : BaseFormModelEntitySchemaComponent(entity)
{
    public override string APIEndpoint => "Schema/ActionInput";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    [Required]
    [DisplayName(TextConstants.LabelFormInputID)]
    [Description(TextConstants.HelpFormInputID)]
    public Guid InputID { get; set; } = entity?.InputID ?? Guid.NewGuid();

    public List<FormModelAssociationSchemaActionInputHasAttribute.Discriminator>? AttributeList { get; set; }
    
    [JsonConstructor]
    public FormModelSchemaActionInput() : this(null)
    {
    }
}
