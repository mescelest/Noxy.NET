using Noxy.NET.EntityManagement.Domain.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema;

public class ResponseSchemaCreate(Guid value) : BaseResponse
{
    public Guid Value { get; } = value;
}
