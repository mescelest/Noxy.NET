using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Data;

namespace Noxy.NET.EntityManagement.Domain.Responses.Data;

public class ResponseDataParameterStyleCreate(EntityDataParameterStyle value) : BaseResponse
{
    public EntityDataParameterStyle Value { get; } = value;
}
