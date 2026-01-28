using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;

namespace Noxy.NET.CaseManagement.Domain.Entities.Authentication;

public class EntityIdentity : BaseEntity
{
    public required string Handle { get; set; }
    public required string Username { get; set; }
    public required int Order { get; set; }

    public required DateTime TimeSignIn { get; set; }

    public required EntityUser? User { get; set; }
    public required Guid UserID { get; set; }
}