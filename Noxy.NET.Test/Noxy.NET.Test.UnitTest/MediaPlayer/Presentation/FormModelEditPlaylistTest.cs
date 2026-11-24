using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Presentation.Models.FormModel;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FormModelEditPlaylistTest
{
    [Fact]
    public void HueList_IsTwelveHuesThirtyDegreesApart_WithinTheWheel()
    {
        Assert.Equal(12, FormModelEditPlaylist.HueList.Count);
        Assert.Equal(Enumerable.Range(0, 12).Select(x => x * 30), FormModelEditPlaylist.HueList);
        Assert.All(FormModelEditPlaylist.HueList, x => Assert.InRange(x, 0, 359));
    }

    [Fact]
    public void GetArtHueChange_WhenTheChoiceIsTheOneItStartedWith_IsLeftOut()
    {
        Assert.False(new FormModelEditPlaylist { ArtHue = 90 }.GetArtHueChange(90).HasValue);
        Assert.False(new FormModelEditPlaylist { ArtHue = null }.GetArtHueChange(null).HasValue);
    }

    [Fact]
    public void GetArtHueChange_ANewHue_IsSentAsThatHue()
    {
        Assert.Equal(200, new FormModelEditPlaylist { ArtHue = 200 }.GetArtHueChange(null).Value);
        Assert.Equal(200, new FormModelEditPlaylist { ArtHue = 200 }.GetArtHueChange(90).Value);
    }

    [Fact]
    public void GetArtHueChange_TheDefaultChoiceAfterAHue_IsSentAsANullToClearIt()
    {
        Optional<int?> change = new FormModelEditPlaylist { ArtHue = null }.GetArtHueChange(90);

        Assert.True(change.HasValue);
        Assert.Null(change.Value);
    }
}
