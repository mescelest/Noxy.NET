using Noxy.NET.MediaPlayer.Application.Services;

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
    public void Resolve_TooLongTitle_IsCutTo255()
    {
        Assert.Equal(255, MediaTitle.Resolve(new string('a', 400), "song.mp3").Length);
    }
}
