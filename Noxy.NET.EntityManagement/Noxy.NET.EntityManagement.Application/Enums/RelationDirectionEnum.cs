namespace Noxy.NET.EntityManagement.Application.Enums;

public enum RelationDirectionEnum
{
    // The starting element is the one the relation reads from: the group holds the elements it reads to.
    Forward,

    // The starting element is the one the relation reads to: the group holds the elements it reads from.
    Inverse
}
