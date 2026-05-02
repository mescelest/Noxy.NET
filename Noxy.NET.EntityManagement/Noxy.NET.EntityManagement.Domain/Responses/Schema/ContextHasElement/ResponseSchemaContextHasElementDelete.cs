using Noxy.NET.EntityManagement.Domain.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

public class ResponseSchemaContextHasElementDelete : BaseResponse
{
    public required Guid Value { get; set; }
}
