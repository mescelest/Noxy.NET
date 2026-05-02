using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Attributes;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Element;

public class RequestSchemaElementDelete : BaseRequestPost<ResponseSchemaElementDelete>
{
    public override string APIEndpoint => $"Schema/Element/{ID}/Delete";

    [NotEmptyGuid]
    public required Guid ID { get; init; }
}
