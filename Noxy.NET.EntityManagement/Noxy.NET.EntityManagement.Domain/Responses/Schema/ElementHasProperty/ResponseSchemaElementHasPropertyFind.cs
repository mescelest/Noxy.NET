using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;

public class ResponseSchemaElementHasPropertyFind : BaseResponse
{
    public required EntitySchemaElementHasProperty Value { get; set; }
}
