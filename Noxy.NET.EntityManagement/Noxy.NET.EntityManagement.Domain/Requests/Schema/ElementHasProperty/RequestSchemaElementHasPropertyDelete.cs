using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Attributes;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.ElementHasProperty;

public class RequestSchemaElementHasPropertyDelete : BaseRequestPost<ResponseSchemaElementHasPropertyDelete>
{
    public override string APIEndpoint => $"schema/element/property/{ID}/delete";

    [NotEmptyGuid]
    public required Guid ID { get; init; }
}
