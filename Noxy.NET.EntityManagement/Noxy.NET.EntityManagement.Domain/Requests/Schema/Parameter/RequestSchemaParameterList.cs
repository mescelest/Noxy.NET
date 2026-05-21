using System.ComponentModel;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Parameter;

public class RequestSchemaParameterList : BaseRequestGetList<ResponseSchemaParameterList>
{
    public override string APIEndpoint => "schema/parameter";

    public Guid? SchemaID { get; set; }

    [DisplayName(ParameterTextConstants.LabelFormIsSystemDefined)]
    [Description(ParameterTextConstants.HelpFormIsSystemDefined)]
    public bool? IsSystemDefined { get; set; }

    [DisplayName(ParameterTextConstants.LabelFormIsApprovalRequired)]
    [Description(ParameterTextConstants.HelpFormIsApprovalRequired)]
    public bool? IsApprovalRequired { get; set; }

    [DisplayName(ParameterTextConstants.LabelFormParameterType)]
    [Description(ParameterTextConstants.HelpFormParameterType)]
    public HashSet<string>? ParameterType { get; set; }

    public override Dictionary<string, object?> ToQueryParameters()
    {
        return new()
        {
            [nameof(SchemaID)] = SchemaID,
            [nameof(IsSystemDefined)] = IsSystemDefined,
            [nameof(IsApprovalRequired)] = IsApprovalRequired,
            [nameof(ParameterType)] = ParameterType,
            [nameof(Search)] = Search,
            [nameof(PageSize)] = PageSize,
            [nameof(PageNumber)] = PageNumber,
            [nameof(SortColumn)] = SortColumn,
            [nameof(SortDirection)] = SortDirection,
        };
    }
}
