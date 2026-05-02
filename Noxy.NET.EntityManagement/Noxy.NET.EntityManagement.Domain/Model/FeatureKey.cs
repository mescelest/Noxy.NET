namespace Noxy.NET.EntityManagement.Domain.Model;

public readonly record struct FeatureKey<TKind>(string Scope, TKind Kind) where TKind : struct, Enum;
