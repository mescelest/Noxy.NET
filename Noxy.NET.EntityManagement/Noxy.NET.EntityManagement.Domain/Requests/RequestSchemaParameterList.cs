using System.ComponentModel;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.Domain.Requests;

public class RequestSchemaParameterList : BaseRequestGet<ResponseSchemaParameterList>
{
    public override string APIEndpoint => "Schema/Parameter";

    [DisplayName(TextConstants.LabelFormSearch)]
    [Description(TextConstants.HelpFormSearch)]
    public string? Search { get; set; }

    [DisplayName(TextConstants.LabelFormIsSystemDefined)]
    [Description(TextConstants.HelpFormIsSystemDefined)]
    public bool? IsSystemDefined { get; set; }

    [DisplayName(TextConstants.LabelFormIsApprovalRequired)]
    [Description(TextConstants.HelpFormIsApprovalRequired)]
    public bool? IsApprovalRequired { get; set; }

    [DisplayName(TextConstants.LabelFormIsApprovalRequired)]
    [Description(TextConstants.HelpFormIsApprovalRequired)]
    public int? PageSize { get; set; }

    [DisplayName(TextConstants.LabelFormIsApprovalRequired)]
    [Description(TextConstants.HelpFormIsApprovalRequired)]
    public int? PageNumber { get; set; }

    public override Dictionary<string, object?> ToQueryParameters()
    {
        return new()
        {
            [nameof(Search)] = Search,
            [nameof(IsSystemDefined)] = IsSystemDefined,
            [nameof(IsApprovalRequired)] = IsApprovalRequired,
            [nameof(PageSize)] = PageSize,
            [nameof(PageNumber)] = PageNumber
        };
    }
}
