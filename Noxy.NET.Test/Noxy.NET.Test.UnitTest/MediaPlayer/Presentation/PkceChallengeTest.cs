using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public partial class PkceChallengeTest
{
    [GeneratedRegex("^[A-Za-z0-9\\-._~]+$")]
    private static partial Regex AllowedCharacters();

    [Fact]
    public void TheVerifier_IsBetween43And128Characters_OfTheAllowedSet()
    {
        for (int i = 0; i < 50; i++)
        {
            PkceChallenge pkce = PkceChallenge.Create();

            Assert.InRange(pkce.Verifier.Length, 43, 128);
            Assert.Matches(AllowedCharacters(), pkce.Verifier);
        }
    }

    [Fact]
    public void TheChallenge_IsTheBase64UrlSha256OfTheVerifier_WithoutPadding()
    {
        PkceChallenge pkce = PkceChallenge.Create();

        string expected = Base64Url.EncodeToString(SHA256.HashData(Encoding.ASCII.GetBytes(pkce.Verifier)));

        Assert.Equal(expected, pkce.Challenge);
        Assert.DoesNotContain('=', pkce.Challenge);
    }

    [Fact]
    public void TwoChallenges_Differ()
    {
        PkceChallenge first = PkceChallenge.Create();
        PkceChallenge second = PkceChallenge.Create();

        Assert.NotEqual(first.Verifier, second.Verifier);
        Assert.NotEqual(first.Challenge, second.Challenge);
    }

    [Fact]
    public void TheMethod_IsS256()
    {
        Assert.Equal("S256", PkceChallenge.Method);
    }
}
