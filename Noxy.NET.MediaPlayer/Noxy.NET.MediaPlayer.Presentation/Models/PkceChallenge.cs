using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

public record PkceChallenge(string Verifier, string Challenge)
{
    // The code_challenge_method sent with the challenge.
    public const string Method = "S256";

    private const int VerifierByteCount = 32;

    public static PkceChallenge Create()
    {
        string verifier = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(VerifierByteCount));
        string challenge = Base64Url.EncodeToString(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));

        return new(verifier, challenge);
    }
}
