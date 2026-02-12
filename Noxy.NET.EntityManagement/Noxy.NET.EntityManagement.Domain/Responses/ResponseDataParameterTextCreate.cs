using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Data;

namespace Noxy.NET.EntityManagement.Domain.Responses;

public class ResponseDataParameterTextCreate : BaseResponse
{
    public required EntityDataParameterText Value { get; set; }
}
