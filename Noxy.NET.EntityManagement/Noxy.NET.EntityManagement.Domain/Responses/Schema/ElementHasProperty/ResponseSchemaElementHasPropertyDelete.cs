using Noxy.NET.EntityManagement.Domain.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;

public class ResponseSchemaElementHasPropertyDelete(Guid value) : BaseResponse
{
    public Guid Value { get; } = value;
}
