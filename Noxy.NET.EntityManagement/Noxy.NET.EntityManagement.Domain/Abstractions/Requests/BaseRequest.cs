using System.Text.Json.Serialization;

namespace Noxy.NET.EntityManagement.Domain.Abstractions.Requests;

public abstract class BaseRequest<TResponse>
{
    [JsonIgnore]
    public abstract string APIEndpoint { get; }

    [JsonIgnore]
    public abstract HttpMethod Method { get; }
}
