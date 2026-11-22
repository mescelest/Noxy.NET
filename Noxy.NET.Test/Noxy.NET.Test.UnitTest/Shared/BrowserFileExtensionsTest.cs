using Noxy.NET.Test.Shared.Fakes;
using Noxy.NET.UI.Extensions;

namespace Noxy.NET.Test.Shared;

public class BrowserFileExtensionsTest
{
    [Theory]
    [InlineData("a.mp3", "audio/mpeg", "audio/*", true)]
    [InlineData("a.png", "image/png", "audio/*", false)]
    [InlineData("a.mp3", "AUDIO/MPEG", "audio/*", true)]
    [InlineData("a.flac", "audio/flac", "audio/mpeg", false)]
    [InlineData("a.flac", "audio/flac", "audio/flac", true)]
    [InlineData("a.MP3", "application/octet-stream", ".mp3", true)]
    [InlineData("a.wav", "application/octet-stream", ".mp3", false)]
    [InlineData("a.wav", "audio/wav", "image/*, audio/*", true)]
    [InlineData("a.txt", "text/plain", "image/*, audio/*", false)]
    public void IsAccepted_FollowsTheAcceptListOfAFileInput(string name, string contentType, string accept, bool expected)
    {
        Assert.Equal(expected, new FakeBrowserFile(name, contentType).IsAccepted(accept));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsAccepted_WithNoList_AcceptsEveryFile(string? accept)
    {
        Assert.True(new FakeBrowserFile("a.png", "image/png").IsAccepted(accept));
    }

    [Fact]
    public void IsAccepted_ForAFileWhoseTypeTheBrowserCouldNotTell_AcceptsIt()
    {
        Assert.True(new FakeBrowserFile("a.xyz", string.Empty).IsAccepted("audio/*"));
    }
}
