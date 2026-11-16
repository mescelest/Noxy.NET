using System.Security.Cryptography;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class HashService : IHashService
{
    public async Task<string> Sha256Hex(Stream content, CancellationToken cancellationToken = default)
    {
        return Convert.ToHexStringLower(await SHA256.HashDataAsync(content, cancellationToken));
    }
}
