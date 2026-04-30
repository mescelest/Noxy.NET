using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

public class ResponseSchemaParameterTextUpdate : BaseResponse
{
    public required EntitySchemaParameterText Value { get; set; }
}
