using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Parameter;

public class RequestSchemaParameterFind : BaseRequestGet<ResponseSchemaParameterFind>
{
    public override string APIEndpoint => $"Schema/Parameter/{ID}";

    public required Guid ID { get; set; }
}
