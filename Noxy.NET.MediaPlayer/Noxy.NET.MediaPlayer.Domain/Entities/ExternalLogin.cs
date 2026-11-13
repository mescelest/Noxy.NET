using Noxy.NET.MediaPlayer.Domain.Abstractions.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Domain.Entities;

/// <summary>
///     Links a user to an account at an external provider. <see cref="ProviderKey" /> is the provider's stable user id
///     (for Google, the "sub" claim), never the email.
/// </summary>
public class ExternalLogin : BaseEntity
{
    public required ExternalLoginProvider Provider { get; set; }
    public required string ProviderKey { get; set; }

    public required Guid UserID { get; set; }
}
