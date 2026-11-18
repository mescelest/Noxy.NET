using System.Text;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class StringExtentionsTest
{
    [Fact]
    public void GetAccessTokenSubject_ReturnsTheSubClaimAsAUserID()
    {
        Guid userID = Guid.NewGuid();

        Assert.Equal(userID, AccessTokenFactory.Create(DateTime.UtcNow.AddHours(1), userID.ToString()).GetAccessTokenSubject());
    }

    [Theory]
    [InlineData("")]
    [InlineData("garbage")]
    [InlineData("two.segments")]
    [InlineData("a.b.c")]
    [InlineData("header.!!!notbase64!!!.signature")]
    public void GetAccessTokenSubject_ForATokenItCannotRead_IsNull(string token)
    {
        Assert.Null(token.GetAccessTokenSubject());
    }

    [Fact]
    public void GetAccessTokenSubject_WhenTheSubIsNotAUserID_IsNull()
    {
        Assert.Null(AccessTokenFactory.Create(null, "someone").GetAccessTokenSubject());
    }

    [Fact]
    public void GetAccessTokenSubject_WithNoSubClaim_IsNull()
    {
        const string payload = "{\"exp\":1}";
        string token = $"e30.{Convert.ToBase64String(Encoding.UTF8.GetBytes(payload)).TrimEnd('=')}.sig";

        Assert.Null(token.GetAccessTokenSubject());
    }

    [Fact]
    public void GetAccessTokenExpires_ReturnsTheExpClaimAsUtc()
    {
        DateTime expires = new(2026, 11, 15, 12, 30, 45, DateTimeKind.Utc);

        DateTime? read = AccessTokenFactory.Create(expires).GetAccessTokenExpires();

        Assert.Equal(expires, read);
        Assert.Equal(DateTimeKind.Utc, read?.Kind);
    }

    [Theory]
    [InlineData("")]
    [InlineData("garbage")]
    [InlineData("two.segments")]
    [InlineData("a.b.c")]
    [InlineData("header.!!!notbase64!!!.signature")]
    public void GetAccessTokenExpires_ForATokenItCannotRead_IsNull(string token)
    {
        Assert.Null(token.GetAccessTokenExpires());
    }

    [Fact]
    public void GetAccessTokenExpires_WithNoExpClaim_IsNull()
    {
        Assert.Null(AccessTokenFactory.Create(null).GetAccessTokenExpires());
    }

    [Theory]
    [InlineData("https://server.example", "https://server.example")]
    [InlineData("https://server.example/", "https://server.example")]
    [InlineData("  HTTP://Server.Example:8080//  ", "http://server.example:8080")]
    [InlineData("https://192.168.1.20:7182", "https://192.168.1.20:7182")]
    [InlineData("https://server.example/api/", "https://server.example/api")]
    public void NormalizeServerAddress_TrimsLowersAndDropsTheTrailingSlash(string address, string expected)
    {
        Assert.Equal(expected, address.NormalizeServerAddress());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("server.example")]
    [InlineData("ftp://server.example")]
    [InlineData("https://server.example?x=1")]
    [InlineData("https://server.example#top")]
    [InlineData("https://user:secret@server.example")]
    [InlineData("not a url")]
    public void NormalizeServerAddress_ForAnythingButAPlainHttpOrHttpsAddress_ThrowsArgumentException(string address)
    {
        Assert.Throws<ArgumentException>(() => address.NormalizeServerAddress());
    }

    private static string TokenWith(string payload)
    {
        return $"e30.{Convert.ToBase64String(Encoding.UTF8.GetBytes(payload)).TrimEnd('=')}.sig";
    }

    [Fact]
    public void GetAccessTokenEmail_ReadsTheEmailClaim()
    {
        Assert.Equal("me@example.com", TokenWith("{\"sub\":\"x\",\"email\":\"me@example.com\"}").GetAccessTokenEmail());
    }

    [Theory]
    [InlineData("")]
    [InlineData("garbage")]
    [InlineData("a.b.c")]
    [InlineData("header.!!!notbase64!!!.signature")]
    public void GetAccessTokenEmail_ForATokenItCannotRead_IsNull(string token)
    {
        Assert.Null(token.GetAccessTokenEmail());
    }

    [Theory]
    [InlineData("{\"exp\":1}")]
    [InlineData("{\"email\":5}")]
    [InlineData("{\"email\":\" \"}")]
    [InlineData("[1]")]
    public void GetAccessTokenEmail_WithNoUsableEmailClaim_IsNull(string payload)
    {
        Assert.Null(TokenWith(payload).GetAccessTokenEmail());
    }
}
