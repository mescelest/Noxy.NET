using Noxy.NET.MediaPlayer.Presentation.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class Int32ExtensionTest
{
    [Fact]
    public void ToCountText_ForExactlyOne_UsesTheSingularText()
    {
        Assert.Equal("1 file", 1.ToCountText("{0} file", "{0} files"));
    }

    [Theory]
    [InlineData(0, "0 files")]
    [InlineData(2, "2 files")]
    [InlineData(26, "26 files")]
    public void ToCountText_ForAnyOtherNumber_UsesThePluralText(int count, string expected)
    {
        Assert.Equal(expected, count.ToCountText("{0} file", "{0} files"));
    }

    [Fact]
    public void ToCountText_PutsFurtherArgumentsAfterTheNumber()
    {
        Assert.Equal("1 file added to Road trip.", 1.ToCountText("{0} file added to {1}.", "{0} files added to {1}.", "Road trip"));
        Assert.Equal("4 files added to Road trip.", 4.ToCountText("{0} file added to {1}.", "{0} files added to {1}.", "Road trip"));
    }

    [Fact]
    public void ToCountText_AllowsATextWithoutThePlaceholder()
    {
        Assert.Equal("Delete song", 1.ToCountText("Delete song", "Delete {0} songs"));
        Assert.Equal("Delete 3 songs", 3.ToCountText("Delete song", "Delete {0} songs"));
    }
}
