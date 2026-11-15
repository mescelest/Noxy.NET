using Noxy.NET.MediaPlayer.Application.Services;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class CreditSplittingServiceTest
{
    private readonly CreditSplittingService _service = new(new NameKeyService());

    [Theory]
    [InlineData("A; B")]
    [InlineData("A;B")]
    [InlineData("A ; B")]
    [InlineData("A feat. B")]
    [InlineData("A Feat. B")]
    [InlineData("A (feat. B)")]
    [InlineData("A [feat. B]")]
    [InlineData("A ft. B")]
    [InlineData("A (ft. B)")]
    [InlineData("A FEATURING B")]
    [InlineData("A featuring B")]
    [InlineData("A (featuring B)")]
    public void Split_SplitsOnEachSeparator(string credit)
    {
        Assert.Equal(["A", "B"], _service.Split(credit));
    }

    [Theory]
    [InlineData("Earth, Wind & Fire")]
    [InlineData("AC/DC")]
    [InlineData("Simon & Garfunkel")]
    [InlineData("Tyler, The Creator")]
    [InlineData("Crosby, Stills, Nash & Young")]
    [InlineData("Hall & Oates")]
    [InlineData("Florence + The Machine")]
    public void Split_KeepsARealNameWithItsPunctuationAsOneValue(string credit)
    {
        Assert.Equal([credit], _service.Split(credit));
    }

    [Theory]
    [InlineData("Feathers")]
    [InlineData("Left Banke")]
    [InlineData("The Left Banke")]
    [InlineData("Soft Machine")]
    [InlineData("Aftermath")]
    public void Split_DoesNotSplitAWordThatMerelyContainsAMarker(string credit)
    {
        Assert.Equal([credit], _service.Split(credit));
    }

    [Fact]
    public void Split_SplitsSeveralMarkersInOneCredit()
    {
        Assert.Equal(["A", "B", "C", "D"], _service.Split("A feat. B; C (ft. D)"));
    }

    [Fact]
    public void Split_KeepsOtherBracketsInTheName()
    {
        Assert.Equal(["Artist (Live)", "B"], _service.Split("Artist (Live) feat. B"));
    }

    [Fact]
    public void Split_TrimsEachValue()
    {
        Assert.Equal(["A", "B"], _service.Split("  A  ;  B  "));
    }

    [Fact]
    public void Split_RemovesRepeatsByKeyKeepingTheFirstSpellingAndOrder()
    {
        Assert.Equal(["Radiohead", "Bjork"], _service.Split("Radiohead; radiohead ; Bjork; BJORK"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(";")]
    [InlineData(" ; ")]
    [InlineData(";;")]
    [InlineData("   ")]
    [InlineData("feat.")]
    public void Split_OfNothingUsable_IsEmpty(string? credit)
    {
        Assert.Empty(_service.Split(credit));
    }

    [Fact]
    public void Split_OfAValueList_FlattensAndRemovesRepeatsAcrossValues()
    {
        List<string> result = _service.Split(["A; B", "b", "C feat. A", "Earth, Wind & Fire"]);

        Assert.Equal(["A", "B", "C", "Earth, Wind & Fire"], result);
    }

    [Fact]
    public void Split_OfAnEmptyValueList_IsEmpty()
    {
        Assert.Empty(_service.Split(Array.Empty<string>()));
    }
}
