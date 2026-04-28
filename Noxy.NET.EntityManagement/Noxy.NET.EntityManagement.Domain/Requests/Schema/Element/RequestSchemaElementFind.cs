using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Element;

public class RequestSchemaElementFind : BaseRequestGet<ResponseSchemaElementFind>
{
    public override string APIEndpoint => $"Schema/Element/{ID}";

    public required Guid ID { get; set; }
}
