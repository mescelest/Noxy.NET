using Noxy.NET.EntityManagement.Domain.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

public class ResponseSchemaContextDelete(Guid value) : BaseResponse
{
    public Guid Value { get; } = value;
}
