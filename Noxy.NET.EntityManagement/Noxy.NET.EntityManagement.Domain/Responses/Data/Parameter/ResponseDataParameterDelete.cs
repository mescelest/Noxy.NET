namespace Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

public class ResponseDataParameterDelete(Guid value)
{
    public Guid Value { get; } = value;
}
