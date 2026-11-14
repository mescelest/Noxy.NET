using Noxy.NET.MediaPlayer.Application.Helpers;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class MediaTitleTest
{
    [Theory]
    [InlineData("song.test.mp3", null, "song.test")]
    [InlineData("song.mp3", "", "song")]
    [InlineData("song.mp3", "   ", "song")]
    [InlineData("song.mp3", "  Given  ", "Given")]
    [InlineData(".mp3", null, "Untitled")]
    public void Resolve_PicksTheTitle(string fileName, string? title, string expected)
    {
        Assert.Equal(expected, MediaTitle.Resolve(title, fileName));
    }

    [Fact]
    public void Resolve_CutInTheMiddleOfASurrogatePair_DoesNotSplitIt()
    {
        string title = new string('a', 254) + "😀";

        string resolved = MediaTitle.Resolve(title, "song.mp3");

        Assert.Equal(254, resolved.Length);
        Assert.False(char.IsSurrogate(resolved[^1]));
    }

    [Fact]
    public void Resolve_CutLeavingTrailingWhitespace_IsTrimmedAgain()
    {
        string title = new string('a', 254) + " bbb";

        Assert.Equal(new string('a', 254), MediaTitle.Resolve(title, "song.mp3"));
    }

    [Fact]
    public void Resolve_TooLongTitle_IsCutTo255()
    {
        Assert.Equal(255, MediaTitle.Resolve(new string('a', 400), "song.mp3").Length);
    }
}
