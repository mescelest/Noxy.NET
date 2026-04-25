using System.ComponentModel;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Element;

public class RequestSchemaElementList : BaseRequestGet<ResponseSchemaElementList>
{
    public override string APIEndpoint => "/Schema/Element";

    public Guid? SchemaID { get; init; }

    [DisplayName(TextConstants.LabelFormSearch)]
    [Description(TextConstants.HelpFormSearch)]
    public string? Search { get; set; }

    [DisplayName(TextConstants.LabelFormPageSize)]
    [Description(TextConstants.HelpFormPageSize)]
    public int? PageSize { get; set; }

    [DisplayName(TextConstants.LabelFormPageNumber)]
    [Description(TextConstants.HelpFormPageNumber)]
    public int? PageNumber { get; set; }

    public override Dictionary<string, object?> ToQueryParameters()
    {
        return new()
        {
            [nameof(SchemaID)] = SchemaID,
            [nameof(Search)] = Search,
            [nameof(PageSize)] = PageSize,
            [nameof(PageNumber)] = PageNumber
        };
    }
}
