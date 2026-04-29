using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Context;

public class RequestSchemaContextFind : BaseRequestGet<ResponseSchemaContextFind>
{
    public override string APIEndpoint => $"Schema/Context/{ID}";

    public required Guid ID { get; set; }
}
