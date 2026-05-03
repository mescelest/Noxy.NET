using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Attributes;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.ContextHasElement;

public class RequestSchemaContextHasElementDelete : BaseRequestPost<ResponseSchemaContextHasElementDelete>
{
    public override string APIEndpoint => $"Schema/Context/Element/{ID}/Delete";

    [NotEmptyGuid]
    public required Guid ID { get; init; }
}
