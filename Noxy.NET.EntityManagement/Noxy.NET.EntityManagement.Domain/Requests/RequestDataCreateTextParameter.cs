using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.Domain.Requests;

public class RequestDataCreateTextParameter : BaseRequestGet<ResponseDataCreateTextParameter>
{
    public override string APIEndpoint => "/Data/Parameter/Text";

    [Required]
    [DisplayName(TextConstants.LabelFormSchemaIdentifier)]
    [Description(TextConstants.HelpFormSchemaIdentifier)]
    public required string SchemaIdentifier { get; set; }

    [Required]
    [DisplayName(TextConstants.LabelFormSchemaIdentifier)]
    [Description(TextConstants.HelpFormSchemaIdentifier)]
    public string Value { get; set; } = string.Empty;

    [Required]
    public DateTime? DateEffective { get; set; }
}
