using Noxy.NET.EntityManagement.Domain.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema;

public class ResponseSchemaActivate(DateTime value) : BaseResponse
{
    public DateTime Value { get; } = value;
}
