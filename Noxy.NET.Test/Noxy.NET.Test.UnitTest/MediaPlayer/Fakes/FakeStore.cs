using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeStore
{
    public List<User> Users { get; } = [];
    public List<ExternalLogin> ExternalLogins { get; } = [];
    public List<RefreshToken> RefreshTokens { get; } = [];

    public int CommitCount { get; set; }
    public int RevokeAllCount { get; set; }
    public bool LoseRevokeRace { get; set; }
}
