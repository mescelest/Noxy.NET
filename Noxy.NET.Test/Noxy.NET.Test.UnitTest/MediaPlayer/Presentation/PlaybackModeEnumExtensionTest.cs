using System.ComponentModel;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Presentation.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class PlaybackModeEnumExtensionTest
{
    [Theory]
    [InlineData(ShuffleModeEnum.Off, ShuffleModeEnum.Random)]
    [InlineData(ShuffleModeEnum.Random, ShuffleModeEnum.SpreadArtists)]
    [InlineData(ShuffleModeEnum.SpreadArtists, ShuffleModeEnum.Off)]
    public void ShuffleGetNext_CyclesThroughTheModes(ShuffleModeEnum mode, ShuffleModeEnum expected)
    {
        Assert.Equal(expected, mode.GetNext());
    }

    [Theory]
    [InlineData(RepeatModeEnum.No, RepeatModeEnum.Once)]
    [InlineData(RepeatModeEnum.Once, RepeatModeEnum.Forever)]
    [InlineData(RepeatModeEnum.Forever, RepeatModeEnum.No)]
    public void RepeatGetNext_CyclesThroughTheModes(RepeatModeEnum mode, RepeatModeEnum expected)
    {
        Assert.Equal(expected, mode.GetNext());
    }

    [Fact]
    public void GetNext_OnAValueThatIsNotAMode_Throws()
    {
        Assert.Throws<InvalidEnumArgumentException>(() => ((ShuffleModeEnum)99).GetNext());
        Assert.Throws<InvalidEnumArgumentException>(() => ((RepeatModeEnum)99).GetNext());
    }

    [Fact]
    public void ShuffleGetText_SaysWhatEachModeDoes()
    {
        Assert.Equal(PresentationTexts.ShuffleOff, ShuffleModeEnum.Off.GetText());
        Assert.Equal(PresentationTexts.ShuffleRandom, ShuffleModeEnum.Random.GetText());
        Assert.Equal(PresentationTexts.ShuffleSpreadArtists, ShuffleModeEnum.SpreadArtists.GetText());
    }

    [Fact]
    public void RepeatGetText_SaysWhatEachModeDoes()
    {
        Assert.Equal(PresentationTexts.RepeatOff, RepeatModeEnum.No.GetText());
        Assert.Equal(PresentationTexts.RepeatOnce, RepeatModeEnum.Once.GetText());
        Assert.Equal(PresentationTexts.RepeatForever, RepeatModeEnum.Forever.GetText());
    }

    [Fact]
    public void GetText_GivesEveryModeItsOwnText()
    {
        List<string> textList = [.. Enum.GetValues<ShuffleModeEnum>().Select(x => x.GetText()), .. Enum.GetValues<RepeatModeEnum>().Select(x => x.GetText())];

        Assert.Equal(textList.Count, textList.Distinct().Count());
        Assert.DoesNotContain(textList, string.IsNullOrWhiteSpace);
    }

    [Fact]
    public void GetText_OnAValueThatIsNotAMode_Throws()
    {
        Assert.Throws<InvalidEnumArgumentException>(() => ((ShuffleModeEnum)99).GetText());
        Assert.Throws<InvalidEnumArgumentException>(() => ((RepeatModeEnum)99).GetText());
    }
}
