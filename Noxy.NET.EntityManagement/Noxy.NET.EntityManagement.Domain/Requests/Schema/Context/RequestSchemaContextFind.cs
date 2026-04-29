using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Element;

public class RequestSchemaContextFind : BaseRequestGet<ResponseSchemaContextFind>
{
    public override string APIEndpoint => $"Schema/Context/{ID}";

    public required Guid ID { get; set; }
}
