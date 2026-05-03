using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

public class ResponseSchemaPropertyBooleanCreate : BaseResponse
{
    public required EntitySchemaPropertyBoolean Value { get; set; }
}
