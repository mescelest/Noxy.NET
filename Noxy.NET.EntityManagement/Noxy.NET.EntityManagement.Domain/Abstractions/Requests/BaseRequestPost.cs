namespace Noxy.NET.EntityManagement.Domain.Abstractions.Requests;

public abstract class BaseRequestPost<TResponse> : BaseRequest<TResponse>
{
    public override HttpMethod Method => HttpMethod.Post;

    public virtual object? ToBody() => this;
}
