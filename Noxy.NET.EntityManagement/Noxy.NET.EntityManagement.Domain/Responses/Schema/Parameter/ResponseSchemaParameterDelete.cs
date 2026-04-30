using Noxy.NET.EntityManagement.Domain.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

public class ResponseSchemaParameterDelete : BaseResponse
{
    public required Guid Value { get; set; }
}
