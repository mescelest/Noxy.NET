using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

public class ResponseSchemaPropertyFind : BaseResponse
{
    public required EntitySchemaProperty.Discriminator Value { get; set; }
}
