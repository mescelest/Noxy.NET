using Noxy.NET.MediaPlayer.Application.Services;

namespace Noxy.NET.Test.MediaPlayer.Authentication;

public class PasswordHashServiceTest
{
    private readonly PasswordHashService _service = new();

    [Fact]
    public void Hash_VerifiesWithTheSamePassword()
    {
        string hash = _service.Hash("correct horse battery staple");

        Assert.True(_service.Verify("correct horse battery staple", hash));
    }

    [Fact]
    public void Hash_DoesNotVerifyWithAnotherPassword()
    {
        string hash = _service.Hash("correct horse battery staple");

        Assert.False(_service.Verify("Correct horse battery staple", hash));
    }

    [Fact]
    public void Hash_IsSaltedAndNeverContainsThePassword()
    {
        string first = _service.Hash("same-password-twice");
        string second = _service.Hash("same-password-twice");

        Assert.NotEqual(first, second);
        Assert.DoesNotContain("same-password-twice", first);
        Assert.StartsWith("PBKDF2-SHA512$", first);
    }

    [Fact]
    public void Verify_NoStoredHash_IsFalse()
    {
        Assert.False(_service.Verify("anything", null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-hash")]
    [InlineData("PBKDF2-SHA512$abc$AAAA$AAAA")]
    [InlineData("PBKDF2-SHA512$0$AAAA$AAAA")]
    [InlineData("PBKDF2-SHA512$1000$not base64$AAAA")]
    [InlineData("OTHER$1000$AAAA$AAAA")]
    public void Verify_MalformedHash_IsFalseAndDoesNotThrow(string stored)
    {
        Assert.False(_service.Verify("anything", stored));
    }
}
