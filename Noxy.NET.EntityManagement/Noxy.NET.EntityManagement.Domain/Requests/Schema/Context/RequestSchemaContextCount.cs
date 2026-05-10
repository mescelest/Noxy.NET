using System.ComponentModel;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Context;

public class RequestSchemaContextCount : BaseRequestGet<ResponseSchemaContextCount>
{
    public override string APIEndpoint => "Schema/Context/Count";

    public Guid? SchemaID { get; set; }

    [DisplayName(ParameterTextConstants.LabelFormSearch)]
    [Description(ParameterTextConstants.HelpFormSearch)]
    public string? Search { get; set; }

    public override Dictionary<string, object?> ToQueryParameters()
    {
        return new()
        {
            [nameof(SchemaID)] = SchemaID,
        };
    }
}
