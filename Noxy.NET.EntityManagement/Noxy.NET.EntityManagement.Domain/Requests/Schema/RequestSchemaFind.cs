using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema;

public class RequestSchemaFind : BaseRequestGet<ResponseSchemaFind>
{
    public override string APIEndpoint => $"Schema/{ID}";

    public required Guid ID { get; set; }
}
