using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema;

public class RequestSchemaUpdate : BaseRequestPost<ResponseSchemaUpdate>
{
    public override string APIEndpoint => $"Schema/{ID}";

    [Required]
    public required Guid ID { get; init; }

    [Required]
    [DisplayName(TextConstants.LabelFormName)]
    [Description(TextConstants.HelpFormName)]
    public string Name { get; set; } = string.Empty;

    [DisplayName(TextConstants.LabelFormDescription)]
    [Description(TextConstants.HelpFormDescription)]
    public string Note { get; set; } = string.Empty;
}
