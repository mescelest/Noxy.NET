using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Attributes;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.ContextHasElement;

public class RequestSchemaContextHasElementList : BaseRequestGet<ResponseSchemaContextHasElementList>
{
    public override string APIEndpoint => "schema/context/element";

    [RequiredIfNot(nameof(SchemaContextID))]
    public Guid? SchemaElementID { get; set; }

    [RequiredIfNot(nameof(SchemaElementID))]
    public Guid? SchemaContextID { get; set; }

    public override Dictionary<string, object?> ToQueryParameters()
    {
        return new()
        {
            [nameof(SchemaElementID)] = SchemaElementID,
            [nameof(SchemaContextID)] = SchemaContextID,
        };
    }
}
