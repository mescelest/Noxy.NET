namespace Noxy.NET.EntityManagement.Domain.Enums;

// How creating a relation went. Everything except Created means nothing was stored.
public enum RelationCreateStatusEnum
{
    Created,

    // The element it reads from, or the one it reads to, does not exist (or has ended).
    FromElementNotFound,
    ToElementNotFound,

    // The two elements already have a current relation of this kind.
    AlreadyExists,

    // The element it reads from, or the one it reads to, already has as many relations of this kind as are allowed.
    FromLimitReached,
    ToLimitReached
}
