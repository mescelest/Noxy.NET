using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Attributes;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Element;

public class RequestSchemaElementClone : BaseRequestPost<ResponseSchemaElementClone>
{
    public override string APIEndpoint => $"schema/element/{ID}/clone";

    [NotEmptyGuid]
    public required Guid ID { get; init; }
}
