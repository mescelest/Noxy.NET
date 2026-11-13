using Noxy.NET.MediaPlayer.Domain.Abstractions.Entities;

namespace Noxy.NET.MediaPlayer.Domain.Entities;

/// <summary>
///     A person who can sign in. A user either has a local password, or signs in through an external provider (Google).
/// </summary>
public class User : BaseEntity
{
    public required string Email { get; set; }
    public bool IsEmailVerified { get; set; }

    /// <summary>Hash produced by the password hasher. Null for users who only sign in externally.</summary>
    public string? PasswordHash { get; set; }

    public List<ExternalLogin> ExternalLogins { get; set; } = [];
}
