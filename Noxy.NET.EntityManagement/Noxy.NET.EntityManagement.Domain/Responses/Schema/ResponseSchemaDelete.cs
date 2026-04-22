using Noxy.NET.EntityManagement.Domain.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema;

public class ResponseSchemaDelete : BaseResponse
{
    public required Guid Value { get; set; }
}
