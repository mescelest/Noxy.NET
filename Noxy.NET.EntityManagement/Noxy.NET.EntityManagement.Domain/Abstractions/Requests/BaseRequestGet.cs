namespace Noxy.NET.EntityManagement.Domain.Abstractions.Requests;

public abstract class BaseRequestGet<TResponse> : BaseRequest<TResponse>
{
    public virtual Dictionary<string, object?> ToQueryParameters() => new();
}
