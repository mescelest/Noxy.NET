using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;

namespace Noxy.NET.EntityManagement.Domain.Entities.Authentication;

public class EntityAuthentication : BaseEntity
{
    public required byte[] Salt { get; set; }
    public required byte[] Hash { get; set; }

    public required Guid UserID { get; set; }
    public required EntityUser? User { get; set; }
}
