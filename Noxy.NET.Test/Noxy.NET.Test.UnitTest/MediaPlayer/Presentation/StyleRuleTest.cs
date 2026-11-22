using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class StyleRuleTest
{
    private static string ReadAll(string path)
    {
        return File.ReadAllText(path);
    }

    private static List<string> InEveryComponentStyle(Func<string, List<string>> find)
    {
        return StyleFileFixture.ComponentStylePathList().SelectMany(path => find(ReadAll(path)).Select(violation => $"{Path.GetFileName(path)} {violation}")).ToList();
    }

    #region -- The rules hold for the App's style files --

    [Fact]
    public void NoComponentStyle_HoldsAColourLiteral()
    {
        Assert.Empty(InEveryComponentStyle(StyleFileFixture.ColourLiteralList));
    }

    [Fact]
    public void NoComponentStyle_SetsAFontFamilyLiteral()
    {
        Assert.Empty(InEveryComponentStyle(StyleFileFixture.FontLiteralList));
    }

    [Fact]
    public void NoComponentStyle_HoldsADurationLiteral()
    {
        Assert.Empty(InEveryComponentStyle(StyleFileFixture.DurationLiteralList));
    }

    [Fact]
    public void NoComponentStyle_HoldsASizeLiteral()
    {
        Assert.Empty(InEveryComponentStyle(StyleFileFixture.SizeLiteralList));
    }

    [Fact]
    public void TheStylesheetsExist_SoTheRulesAboveAreNotAskingNothing()
    {
        Assert.NotEmpty(StyleFileFixture.ComponentStylePathList());
        Assert.True(File.Exists(StyleFileFixture.AppStylePath("variables.css")));
        Assert.True(File.Exists(StyleFileFixture.AppStylePath("breakpoints.css")));
    }

    [Fact]
    public void VariablesCss_DefinesEveryVariableTheUiLibrariesAndTheAppRead()
    {
        // A component may declare a custom property for itself in its own style (a value its parts share): that is a definition too.
        List<string> stylePathList = [StyleFileFixture.AppStylePath("variables.css"), .. StyleFileFixture.UiStylePathList(), .. StyleFileFixture.ComponentStylePathList()];
        List<string> definedList = stylePathList.SelectMany(path => StyleFileFixture.VariableDefinitionList(ReadAll(path))).Distinct().ToList();
        List<string> readList = stylePathList.SelectMany(path => StyleFileFixture.VariableReadList(ReadAll(path))).Distinct().ToList();

        List<string> missingList = readList.Where(x => !definedList.Contains(x)).Order().ToList();

        Assert.Empty(missingList);
    }

    #endregion -- The rules hold for the App's style files --

    #region -- The scanner itself bites --

    [Theory]
    [InlineData(".a { color: #fff; }")]
    [InlineData(".a { color: #1b6ec2; }")]
    [InlineData(".a { background: rgb(3, 23, 62); }")]
    [InlineData(".a { background: rgba(0,0,0,.5); }")]
    [InlineData(".a { color: hsl(10 20% 30%); }")]
    [InlineData(".a { color: white; }")]
    [InlineData(".a { border-color: Red; }")]
    public void TheColourScanner_FindsALiteral(string css)
    {
        Assert.NotEmpty(StyleFileFixture.ColourLiteralList(css));
    }

    [Theory]
    [InlineData(".a { color: var(--color-primary); }")]
    [InlineData(".a { color: var(--color-primary, #ffffff); }")]
    [InlineData(".a { background: transparent; color: currentColor; border-color: inherit; }")]
    [InlineData(".a { color: color-mix(in srgb, var(--color-primary) 50%, var(--color-complementary)); }")]
    [InlineData("/* color: #fff; */ .a { color: var(--color-primary); }")]
    [InlineData("#app { color: var(--color-primary); }")]
    public void TheColourScanner_AllowsVariablesAndTheKeywords(string css)
    {
        Assert.Empty(StyleFileFixture.ColourLiteralList(css));
    }

    [Theory]
    [InlineData(".a { font-family: 'Helvetica Neue', Arial; }")]
    [InlineData(".a { font: 12px sans-serif; }")]
    public void TheFontScanner_FindsALiteral(string css)
    {
        Assert.NotEmpty(StyleFileFixture.FontLiteralList(css));
    }

    [Theory]
    [InlineData(".a { font-family: var(--font-family-primary); }")]
    [InlineData(".a { font: var(--font-body); }")]
    [InlineData(".a { font-family: inherit; }")]
    [InlineData("@font-face { font-family: \"Some Face\"; src: url(\"a.woff2\"); } .a { font-family: var(--font-family-primary); }")]
    public void TheFontScanner_AllowsAVariable(string css)
    {
        Assert.Empty(StyleFileFixture.FontLiteralList(css));
    }

    [Theory]
    [InlineData(".a { transition: opacity 200ms ease; }")]
    [InlineData(".a { animation-duration: 0.5s; }")]
    [InlineData(".a { transition-delay: 1s; }")]
    public void TheDurationScanner_FindsALiteral(string css)
    {
        Assert.NotEmpty(StyleFileFixture.DurationLiteralList(css));
    }

    [Theory]
    [InlineData(".a { transition: opacity var(--animation-length-short) ease; }")]
    [InlineData(".a { transition: opacity var(--animation-length-short, 150ms) ease; }")]
    [InlineData(".a { opacity: 1; }")]
    public void TheDurationScanner_AllowsAVariable(string css)
    {
        Assert.Empty(StyleFileFixture.DurationLiteralList(css));
    }

    [Theory]
    [InlineData(".a { padding: 8px; }")]
    [InlineData(".a { margin: 0 1.5rem; }")]
    [InlineData(".a { gap: .5em; }")]
    [InlineData("@media (min-width: 600px) { .a { color: var(--color-primary); } }")]
    public void TheSizeScanner_FindsALiteral(string css)
    {
        Assert.NotEmpty(StyleFileFixture.SizeLiteralList(css));
    }

    [Theory]
    [InlineData(".a { padding: var(--size-gap-sm); }")]
    [InlineData(".a { margin: 0; }")]
    [InlineData(".a { margin: 0px; }")]
    [InlineData(".a { width: 100%; height: 100vh; }")]
    [InlineData(".a { width: calc(100% - var(--size-gap-sm)); }")]
    [InlineData(".a { flex: 1 1 0; z-index: 10; }")]
    public void TheSizeScanner_AllowsVariablesZeroAndRelativeUnits(string css)
    {
        Assert.Empty(StyleFileFixture.SizeLiteralList(css));
    }

    [Fact]
    public void TheVariableScanner_ReadsAndFindsDefinitions()
    {
        const string css = ":root { --color-a: red; --size-b: 1px; } .a { color: var(--color-a); margin: var( --size-c, 2px); }";

        Assert.Equal(["--color-a", "--size-c"], StyleFileFixture.VariableReadList(css));
        Assert.Equal(["--color-a", "--size-b"], StyleFileFixture.VariableDefinitionList(css));
    }

    #endregion -- The scanner itself bites --
}
