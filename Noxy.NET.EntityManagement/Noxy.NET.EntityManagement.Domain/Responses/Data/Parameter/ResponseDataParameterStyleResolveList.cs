namespace Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

public class ResponseDataParameterStyleResolveList(Dictionary<string, string?> value)
{
    public Dictionary<string, string?> Value { get; } = value;
}
