using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Data;

namespace Noxy.NET.EntityManagement.Domain.Responses.Data;

public class ResponseDataParameterSystemCreate(EntityDataParameterSystem value) : BaseResponse
{
    public EntityDataParameterSystem Value { get; } = value;
}
