using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas;

public class FormModelSchemaPropertyInteger(EntitySchemaPropertyInteger? entity) : BaseFormModelEntitySchemaComponent(entity)
{
    [JsonConstructor]
    public FormModelSchemaPropertyInteger() : this(null)
    {
    }

    public override string APIEndpoint => "Schema/Property/Integer";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    [Required]
    [DisplayName(TextConstants.LabelFormIsUnsigned)]
    [Description(TextConstants.HelpFormIsUnsigned)]
    public bool IsUnsigned { get; set; } = entity?.IsUnsigned ?? false;
}
