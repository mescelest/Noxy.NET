using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;

public class ResponseSchemaElementHasPropertyFind(EntitySchemaElementHasProperty value) : BaseResponse
{
    public EntitySchemaElementHasProperty Value { get; } = value;
}
