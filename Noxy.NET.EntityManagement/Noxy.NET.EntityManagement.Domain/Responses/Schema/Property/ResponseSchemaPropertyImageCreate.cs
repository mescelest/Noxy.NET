using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

public class ResponseSchemaPropertyImageCreate : BaseResponse
{
    public required EntitySchemaPropertyImage Value { get; set; }
}
