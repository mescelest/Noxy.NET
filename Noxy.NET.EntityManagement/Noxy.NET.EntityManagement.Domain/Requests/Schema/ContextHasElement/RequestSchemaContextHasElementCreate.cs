using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Attributes;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.ContextHasElement;

public class RequestSchemaContextHasElementCreate : BaseRequestPost<ResponseSchemaContextCreate>
{
    public override string APIEndpoint => "Schema/Context/Element";

    [NotEmptyGuid]
    public Guid SchemaElementID { get; set; }

    [NotEmptyGuid]
    public Guid SchemaContextID { get; set; }
}
