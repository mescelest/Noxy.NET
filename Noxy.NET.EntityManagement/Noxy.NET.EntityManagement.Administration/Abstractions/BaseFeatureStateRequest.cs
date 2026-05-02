using Noxy.NET.EntityManagement.Domain.Model;

namespace Noxy.NET.EntityManagement.Administration.Abstractions;

public abstract record BaseFeatureStateRequest<TKind> where TKind : struct, Enum
{
    public Dictionary<FeatureKey<TKind>, bool> Loading { get; init; } = [];
    public Dictionary<FeatureKey<TKind>, string?> Error { get; init; } = [];
}
