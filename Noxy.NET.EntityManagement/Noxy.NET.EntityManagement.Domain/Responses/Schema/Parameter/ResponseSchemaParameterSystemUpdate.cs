using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

public class ResponseSchemaParameterSystemUpdate : BaseResponse
{
    public required EntitySchemaParameterSystem Value { get; set; }
}
