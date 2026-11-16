using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class StringExtentionsTest
{
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
}
