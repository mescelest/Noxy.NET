namespace Noxy.NET.EntityManagement.Domain.Abstractions.Requests;

public abstract class BaseRequest<TResponse>
{
    public abstract string APIEndpoint { get; }
    public abstract HttpMethod Method { get; }
}
