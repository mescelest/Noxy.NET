using System.ComponentModel;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Property;

public class RequestSchemaPropertyCount : BaseRequestGet<ResponseSchemaPropertyCount>
{
    public override string APIEndpoint => "Schema/Property/Count";

    public Guid? SchemaID { get; set; }

    [DisplayName(ParameterTextConstants.LabelFormSearch)]
    [Description(ParameterTextConstants.HelpFormSearch)]
    public string? Search { get; set; }

    [DisplayName(ParameterTextConstants.LabelFormPropertyType)]
    [Description(ParameterTextConstants.HelpFormPropertyType)]
    public HashSet<string>? PropertyType { get; set; }

    public override Dictionary<string, object?> ToQueryParameters()
    {
        return new()
        {
            [nameof(SchemaID)] = SchemaID,
        };
    }
}
