using Noxy.NET.EntityManagement.Domain.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

public class ResponseSchemaPropertyStringUpdate(Guid value) : BaseResponse
{
    public Guid Value { get; } = value;
}
