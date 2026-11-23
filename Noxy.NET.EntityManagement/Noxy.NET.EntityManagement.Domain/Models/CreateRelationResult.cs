using Noxy.NET.EntityManagement.Domain.Entities;
using Noxy.NET.EntityManagement.Domain.Enums;

namespace Noxy.NET.EntityManagement.Domain.Models;

// The relation is only there when it was created.
public sealed record CreateRelationResult(RelationCreateStatusEnum Status, DataRelation? Relation);
