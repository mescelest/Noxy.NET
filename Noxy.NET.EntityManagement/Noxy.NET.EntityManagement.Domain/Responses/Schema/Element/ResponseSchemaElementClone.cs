using Noxy.NET.EntityManagement.Domain.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

public class ResponseSchemaElementClone(Guid value) : BaseResponse
{
    public Guid Value { get; } = value;
}
