using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Attributes;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema;

public class RequestSchemaElementCreate : BaseRequestPost<ResponseSchemaElementCreate>
{
    public override string APIEndpoint => "Schema/Context";

    public Guid? SchemaID { get; set; }

    [Required]
    [DisplayName(TextConstants.LabelFormSchemaIdentifier)]
    [Description(TextConstants.HelpFormSchemaIdentifier)]
    public string SchemaIdentifier { get; set; } = string.Empty;

    [Required]
    [DisplayName(TextConstants.LabelFormName)]
    [Description(TextConstants.HelpFormName)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [DisplayName(TextConstants.LabelFormDescription)]
    [Description(TextConstants.HelpFormDescription)]
    public string Note { get; set; } = string.Empty;

    [NotEmpty]
    [DisplayName(TextConstants.LabelFormTitle)]
    [Description(TextConstants.HelpFormTitle)]
    public Guid TitleParameterTextID { get; set; }

    [NotEmpty]
    [DisplayName(TextConstants.LabelFormDescription)]
    [Description(TextConstants.HelpFormDescription)]
    public Guid DescriptionParameterTextID { get; set; }
}
