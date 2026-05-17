using Noxy.NET.EntityManagement.Domain.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

public class ResponseSchemaContextHasElementDelete(Guid value) : BaseResponse
{
    public Guid Value { get; } = value;
}
