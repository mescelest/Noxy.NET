using Noxy.NET.MediaPlayer.Application.Commands.Media;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class CommandMediaUploadTest
{
    private static string ResolveTitle(string? title, string fileName)
    {
        return new CommandMediaUpload(Guid.CreateVersion7(), fileName, title, Stream.Null).ResolveTitle();
    }

    [Theory]
    [InlineData("media.test.mp3", null, "media.test")]
    [InlineData("media.mp3", "", "media")]
    [InlineData("media.mp3", "   ", "media")]
    [InlineData("media.mp3", "  Given  ", "Given")]
    [InlineData(".mp3", null, "Untitled")]
    public void ResolveTitle_PicksTheTitle(string fileName, string? title, string expected)
    {
        Assert.Equal(expected, ResolveTitle(title, fileName));
    }

    [Fact]
    public void ResolveTitle_CutInTheMiddleOfASurrogatePair_DoesNotSplitIt()
    {
        string title = new string('a', 254) + "😀";

        string resolved = ResolveTitle(title, "media.mp3");

        Assert.Equal(254, resolved.Length);
        Assert.False(char.IsSurrogate(resolved[^1]));
    }

    [Fact]
    public void ResolveTitle_CutLeavingTrailingWhitespace_IsTrimmedAgain()
    {
        string title = new string('a', 254) + " bbb";

        Assert.Equal(new string('a', 254), ResolveTitle(title, "media.mp3"));
    }

    [Fact]
    public void ResolveTitle_TooLongTitle_IsCutTo255()
    {
        Assert.Equal(255, ResolveTitle(new string('a', 400), "media.mp3").Length);
    }
}
