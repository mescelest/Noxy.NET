using Noxy.NET.MediaPlayer.Application.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakePasswordHashService : IPasswordHashService
{
    public List<string?> VerifiedHashes { get; } = [];

    public string Hash(string password)
    {
        return $"hashed:{password}";
    }

    public bool Verify(string password, string? storedHash)
    {
        VerifiedHashes.Add(storedHash);
        return storedHash == $"hashed:{password}";
    }
}
