using System.Text.Json.Serialization;

namespace Noxy.NET.EntityManagement.Domain.Abstractions.Requests;

// ReSharper disable once UnusedTypeParameter
public abstract class BaseRequest<TResponse> : BaseRequest;

public abstract class BaseRequest
{
    [JsonIgnore]
    public abstract string APIEndpoint { get; }
}
