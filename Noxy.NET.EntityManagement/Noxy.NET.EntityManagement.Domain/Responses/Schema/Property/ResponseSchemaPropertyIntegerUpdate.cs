using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

public class ResponseSchemaPropertyIntegerUpdate : BaseResponse
{
    public required EntitySchemaPropertyInteger Value { get; set; }
}
