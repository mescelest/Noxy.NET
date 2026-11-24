using Noxy.NET.MediaPlayer.Domain.Declarations;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class PlaylistArtHueTest
{
    [Fact]
    public void ChoiceList_IsTwelveHuesThirtyDegreesApart_WithinTheWheel()
    {
        Assert.Equal(12, PlaylistArtHue.ChoiceList.Count);
        Assert.Equal(Enumerable.Range(0, 12).Select(x => x * 30), PlaylistArtHue.ChoiceList);
        Assert.All(PlaylistArtHue.ChoiceList, x => Assert.InRange(x, PlaylistArtHue.Min, PlaylistArtHue.Max));
    }

    [Fact]
    public void PickChoice_AlwaysGivesOneOfTheChoices_AndNotAlwaysTheSame()
    {
        Random random = new(7);

        List<int> pickedList = [.. Enumerable.Range(0, 200).Select(_ => PlaylistArtHue.PickChoice(random))];

        Assert.All(pickedList, x => Assert.Contains(x, PlaylistArtHue.ChoiceList));
        Assert.True(pickedList.Distinct().Count() > 1);
    }

    [Fact]
    public void Wheel_HasThreeHundredAndSixtyPlaces()
    {
        Assert.Equal(360, PlaylistArtHue.Count);
    }
}
