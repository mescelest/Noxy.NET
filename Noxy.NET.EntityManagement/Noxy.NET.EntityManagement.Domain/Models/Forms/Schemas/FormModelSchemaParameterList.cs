using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas;

public class FormModelSchemaParameterList : BaseFormModel
{
    [DisplayName(TextConstants.LabelFormSearch)]
    [Description(TextConstants.HelpFormSearch)]
    public string? Search { get; set; }

    [Required]
    [DisplayName(TextConstants.LabelFormIsSystemDefined)]
    [Description(TextConstants.HelpFormIsSystemDefined)]
    public bool IsSystemDefined { get; set; }

    [Required]
    [DisplayName(TextConstants.LabelFormIsApprovalRequired)]
    [Description(TextConstants.HelpFormIsApprovalRequired)]
    public bool IsApprovalRequired { get; set; }

    public IDictionary<string, object?> AsQueryParameters() => new Dictionary<string, object?>
    {
        [nameof(Search)] = Search,
        [nameof(IsSystemDefined)] = IsSystemDefined,
        [nameof(IsApprovalRequired)] = IsApprovalRequired
    };
}
