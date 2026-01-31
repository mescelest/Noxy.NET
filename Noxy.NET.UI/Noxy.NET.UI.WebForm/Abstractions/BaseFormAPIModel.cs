using System.Text.Json.Serialization;

namespace Noxy.NET.UI.Abstractions;

public abstract class BaseFormAPIModel : BaseFormModel
{
    [JsonIgnore]
    public abstract string APIEndpoint { get; }

    [JsonIgnore]
    public abstract HttpMethod HttpMethod { get; }
}

public abstract class BaseFormAPIModel<TResponse> : BaseFormAPIModel
{
    [JsonIgnore]
    public Type ResponseType { get; } = typeof(TResponse);
}
