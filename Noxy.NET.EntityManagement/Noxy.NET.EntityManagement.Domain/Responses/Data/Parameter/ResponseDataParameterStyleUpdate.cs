using Noxy.NET.EntityManagement.Domain.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

public class ResponseDataParameterStyleUpdate(Guid value) : BaseResponse
{
    public Guid Value { get; } = value;
}
