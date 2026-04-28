namespace Noxy.NET.EntityManagement.Domain.Abstractions.Requests;

public abstract class BaseRequestPost<TResponse> : BaseRequest<TResponse>
{
    public virtual object? ToBody() => this;
}
