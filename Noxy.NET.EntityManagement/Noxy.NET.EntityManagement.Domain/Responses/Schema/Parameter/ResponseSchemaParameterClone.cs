using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

public class ResponseSchemaParameterClone : BaseResponse
{
    public required EntitySchemaElement Value { get; set; }
}
