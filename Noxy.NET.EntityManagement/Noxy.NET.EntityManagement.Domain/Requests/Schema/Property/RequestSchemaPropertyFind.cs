using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Property;

public class RequestSchemaPropertyFind : BaseRequestGet<ResponseSchemaPropertyFind>
{
    public override string APIEndpoint => $"Schema/Parameter/{ID}";

    public required Guid ID { get; set; }
}
