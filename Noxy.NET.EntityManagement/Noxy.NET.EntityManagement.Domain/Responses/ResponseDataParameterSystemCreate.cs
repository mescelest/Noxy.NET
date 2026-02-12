using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Data;

namespace Noxy.NET.EntityManagement.Domain.Responses;

public class ResponseDataParameterSystemCreate : BaseResponse
{
    public required EntityDataParameterSystem Value { get; set; }
}
