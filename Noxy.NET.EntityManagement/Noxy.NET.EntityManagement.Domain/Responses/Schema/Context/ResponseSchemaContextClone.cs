using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

public class ResponseSchemaContextClone : BaseResponse
{
    public required EntitySchemaContext Value { get; set; }
}
