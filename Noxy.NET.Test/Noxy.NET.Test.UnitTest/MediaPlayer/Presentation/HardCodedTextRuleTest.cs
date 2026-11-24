using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class HardCodedTextRuleTest
{
    [Fact]
    public void NoRazorMarkup_HoldsLiteralText_ItIsInAppTexts()
    {
        List<string> violationList = [.. MarkupFileFixture.RazorPathList().SelectMany(path => MarkupFileFixture.TextLiteralList(File.ReadAllText(path)).Select(violation => $"{Path.GetFileName(path)} {violation}"))];

        Assert.Empty(violationList);
    }

    [Theory]
    [InlineData("<span>Home</span>", 1)]
    [InlineData("<Button aria-label=\"Save\" title=\"Save\">", 2)]
    [InlineData("<MediaFileHeader Name=\"Library\"/>", 1)]
    [InlineData("<InputControlText Name=\"server\" placeholder=\"https://\"/>", 0)]
    [InlineData("<span>@AppTexts.Home</span>", 0)]
    [InlineData("<MaterialIcon Icon=\"queue_music\"/>", 0)]
    [InlineData("<span class=\"title\">@Title</span>", 0)]
    public void TheRule_FindsLiteralTextInMarkup_AndLeavesAppTextsAndNamesAlone(string markup, int count)
    {
        Assert.Equal(count, MarkupFileFixture.TextLiteralList(markup).Count);
    }

    [Fact]
    public void TheRule_IgnoresTheCodeBlock()
    {
        Assert.Empty(MarkupFileFixture.TextLiteralList("<span>@Name</span>\n@code {\n    private string Label => \"Not found\";\n    <span>Home</span>\n}"));
    }
}
