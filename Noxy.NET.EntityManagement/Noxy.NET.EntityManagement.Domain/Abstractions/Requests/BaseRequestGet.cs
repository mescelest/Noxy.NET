namespace Noxy.NET.EntityManagement.Domain.Abstractions.Requests;

public abstract class BaseRequestGet<TResponse> : BaseRequest<TResponse>
{
    public override HttpMethod Method => HttpMethod.Get;

    public virtual Dictionary<string, object?> ToQueryParameters() => new();
}
