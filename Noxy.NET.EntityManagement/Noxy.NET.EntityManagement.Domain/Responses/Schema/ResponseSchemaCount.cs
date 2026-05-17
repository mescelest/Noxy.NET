using Noxy.NET.EntityManagement.Domain.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema;

public class ResponseSchemaCount(int value) : BaseResponse
{
    public int Value { get; } = value;
}
