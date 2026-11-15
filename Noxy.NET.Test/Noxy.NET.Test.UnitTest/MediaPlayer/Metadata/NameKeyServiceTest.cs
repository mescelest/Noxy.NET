using Noxy.NET.MediaPlayer.Application.Services;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class NameKeyServiceTest
{
    private readonly NameKeyService _service = new();

    [Theory]
    [InlineData("Radiohead")]
    [InlineData("radiohead ")]
    [InlineData("  RADIOHEAD")]
    [InlineData("\tRadioHead\n")]
    public void GetKey_IgnoresCaseAndSurroundingWhitespace(string name)
    {
        Assert.Equal(_service.GetKey("Radiohead"), _service.GetKey(name));
        Assert.Equal("radiohead", _service.GetKey(name));
    }

    [Fact]
    public void GetKey_CollapsesInnerWhitespace()
    {
        Assert.Equal(_service.GetKey("Massive Attack"), _service.GetKey("Massive   Attack"));
        Assert.Equal(_service.GetKey("Massive Attack"), _service.GetKey("Massive\t  Attack"));
    }

    [Fact]
    public void GetKey_TreatsComposedAndDecomposedLettersAsOne()
    {
        string composed = "Beyoncé";
        string decomposed = "Beyoncé";

        Assert.Equal(_service.GetKey(composed), _service.GetKey(decomposed));
    }

    [Fact]
    public void GetKey_MatchesFullWidthLettersToTheirNormalForm()
    {
        Assert.Equal(_service.GetKey("ABBA"), _service.GetKey("ＡＢＢＡ"));
    }

    [Fact]
    public void GetKey_KeepsPunctuationSoDifferentNamesStayApart()
    {
        Assert.NotEqual(_service.GetKey("ACDC"), _service.GetKey("AC/DC"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void GetKey_OfABlankName_IsEmpty(string name)
    {
        Assert.Equal(string.Empty, _service.GetKey(name));
    }
}
