using Noxy.NET.EntityManagement.Domain.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

public class ResponseSchemaParameterSystemCreate(Guid value) : BaseResponse
{
    public Guid Value { get; } = value;
}
