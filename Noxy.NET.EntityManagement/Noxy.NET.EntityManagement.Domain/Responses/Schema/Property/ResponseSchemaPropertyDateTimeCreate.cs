using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

public class ResponseSchemaPropertyDateTimeCreate : BaseResponse
{
    public required EntitySchemaPropertyDateTime Value { get; set; }
}
