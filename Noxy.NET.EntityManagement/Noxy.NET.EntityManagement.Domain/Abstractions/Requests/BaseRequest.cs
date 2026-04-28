using System.Text.Json.Serialization;

namespace Noxy.NET.EntityManagement.Domain.Abstractions.Requests;

// ReSharper disable once UnusedTypeParameter
public abstract class BaseRequest<TResponse>
{
    [JsonIgnore]
    public abstract string APIEndpoint { get; }
}
