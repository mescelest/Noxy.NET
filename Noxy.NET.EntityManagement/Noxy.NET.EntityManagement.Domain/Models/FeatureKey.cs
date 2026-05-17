namespace Noxy.NET.EntityManagement.Domain.Models;

public readonly record struct FeatureKey<TKind>(string Scope, TKind Kind) where TKind : struct, Enum;
