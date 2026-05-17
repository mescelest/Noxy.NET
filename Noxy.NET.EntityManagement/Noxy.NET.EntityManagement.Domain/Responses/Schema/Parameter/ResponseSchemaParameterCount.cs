namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

public class ResponseSchemaParameterCount(int value)
{
    public int Value { get; } = value;
}
