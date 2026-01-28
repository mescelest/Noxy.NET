using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Forms;
using Noxy.NET.CaseManagement.Domain.Constants;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;

namespace Noxy.NET.CaseManagement.Domain.Forms.Schemas.Forms;

public class FormModelSchemaDynamicValueCode(EntitySchemaDynamicValueCode? entity= null) : BaseFormModelEntitySchema(entity)
{
    public override string APIEndpoint => "Schema/DynamicValue/Code";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    [Required]
    [DisplayName(TextConstants.LabelFormCodeSnippet)]
    [Description(TextConstants.HelpFormCodeSnippet)]
    public string CodeSnippet { get; set; } = entity?.Value ?? string.Empty;

    [Required]
    [DisplayName(TextConstants.LabelFormIsAsynchronous)]
    [Description(TextConstants.HelpFormIsAsynchronous)]
    public bool IsAsynchronous { get; set; } = entity?.IsAsynchronous ?? true;

    [JsonConstructor]
    public FormModelSchemaDynamicValueCode() : this(null)
    {
    }
}
