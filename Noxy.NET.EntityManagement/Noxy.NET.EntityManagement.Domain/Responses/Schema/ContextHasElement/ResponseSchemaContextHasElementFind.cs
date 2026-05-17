using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

public class ResponseSchemaContextHasElementFind(EntitySchemaContextHasElement value) : BaseResponse
{
    public EntitySchemaContextHasElement Value { get; } = value;
}
