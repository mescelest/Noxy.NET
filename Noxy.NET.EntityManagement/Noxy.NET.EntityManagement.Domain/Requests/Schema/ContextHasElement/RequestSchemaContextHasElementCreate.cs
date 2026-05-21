using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Attributes;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.ContextHasElement;

public class RequestSchemaContextHasElementCreate : BaseRequestPost<ResponseSchemaContextHasElementCreate>
{
    public override string APIEndpoint => "schema/context/element";

    [NotEmptyGuid]
    public Guid SchemaElementID { get; set; }

    [NotEmptyGuid]
    public Guid SchemaContextID { get; set; }
}
