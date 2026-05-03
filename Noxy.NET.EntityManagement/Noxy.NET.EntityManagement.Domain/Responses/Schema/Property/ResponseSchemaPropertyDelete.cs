using Noxy.NET.EntityManagement.Domain.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

public class ResponseSchemaPropertyDelete : BaseResponse
{
    public required Guid Value { get; set; }
}
