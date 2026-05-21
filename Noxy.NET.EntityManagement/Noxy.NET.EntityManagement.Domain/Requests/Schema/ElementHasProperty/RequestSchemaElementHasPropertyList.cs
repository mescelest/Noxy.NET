using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Attributes;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.ElementHasProperty;

public class RequestSchemaElementHasPropertyList : BaseRequestGet<ResponseSchemaElementHasPropertyList>
{
    public override string APIEndpoint => "schema/element/property";

    [RequiredIfNot(nameof(SchemaPropertyID))]
    public Guid? SchemaElementID { get; set; }

    [RequiredIfNot(nameof(SchemaElementID))]
    public Guid? SchemaPropertyID { get; set; }

    public override Dictionary<string, object?> ToQueryParameters()
    {
        return new()
        {
            [nameof(SchemaElementID)] = SchemaElementID,
            [nameof(SchemaPropertyID)] = SchemaPropertyID,
        };
    }
}
