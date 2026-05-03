using System.ComponentModel;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Element;

public class RequestSchemaElementCount : BaseRequestGet<ResponseSchemaElementCount>
{
    public override string APIEndpoint => "Schema/Element/Count";

    public Guid? SchemaID { get; set; }

    [DisplayName(TextConstants.LabelFormSearch)]
    [Description(TextConstants.HelpFormSearch)]
    public string? Search { get; set; }

    public override Dictionary<string, object?> ToQueryParameters()
    {
        return new()
        {
            [nameof(SchemaID)] = SchemaID,
        };
    }
}
