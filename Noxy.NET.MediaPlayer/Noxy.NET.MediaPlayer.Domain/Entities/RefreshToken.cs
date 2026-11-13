using Noxy.NET.MediaPlayer.Domain.Abstractions.Entities;

namespace Noxy.NET.MediaPlayer.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public required string TokenHash { get; set; }
    public required DateTime TimeExpires { get; set; }
    public DateTime? TimeRevoked { get; set; }

    public required Guid UserID { get; set; }
}
