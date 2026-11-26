using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Models;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class MediaTagSetMatchesTest
{
    private static MediaTagValues Values()
    {
        return new("Title", ["Main", "Guest"], "Album", ["Album Artist"], "Subtitle", "Comment", ["Rock"], 3, 2, 1999);
    }

    private static MediaTagSet Tags()
    {
        return new("Title", ["Main", "Guest"], "Album", ["Album Artist"], ["Rock"], 3, 2, 1999, "Comment", "Subtitle", TimeSpan.FromSeconds(215), [1, 2, 3]);
    }

    [Fact]
    public void Matches_WhenEveryComparedFieldIsEqual()
    {
        Assert.True(Tags().Matches(Values()));
    }

    [Fact]
    public void Matches_IgnoresPictureAndDuration()
    {
        Assert.True((Tags() with { Picture = null, Duration = null }).Matches(Values()));
    }

    [Fact]
    public void Matches_WithAPicture_RequiresTheSameCoverBytes()
    {
        Assert.True(Tags().Matches(Values(), [1, 2, 3]));
        Assert.False(Tags().Matches(Values(), [1, 2, 4]));
        Assert.False(Tags().Matches(Values(), [1, 2]));
    }

    [Fact]
    public void Matches_WithAPictureAndAFileWithNoCover_IsFalse()
    {
        Assert.False((Tags() with { Picture = null }).Matches(Values(), [1, 2, 3]));
    }

    [Fact]
    public void Matches_WithNoPicture_IgnoresTheCover()
    {
        Assert.True((Tags() with { Picture = [9, 9] }).Matches(Values(), null));
        Assert.True((Tags() with { Picture = null }).Matches(Values(), null));
    }

    [Fact]
    public void Matches_WithAPicture_StillRequiresTheTextTagsToMatch()
    {
        Assert.False((Tags() with { Title = "Other" }).Matches(Values(), [1, 2, 3]));
    }

    [Fact]
    public void Matches_TreatsNullAndEmptyTextAsEqual()
    {
        MediaTagValues values = Values() with { Album = null, Subtitle = "", Comment = "  " };
        MediaTagSet tags = Tags() with { Album = "", Subtitle = null, Comment = null };

        Assert.True(tags.Matches(values));
    }

    [Fact]
    public void Matches_TrimsTheText()
    {
        Assert.True((Tags() with { Title = "  Title " }).Matches(Values()));
    }

    [Fact]
    public void Matches_TreatsNullAndZeroNumbersAsEqual()
    {
        MediaTagValues values = Values() with { TrackNumber = 0, DiscNumber = null, Year = 0 };
        MediaTagSet tags = Tags() with { TrackNumber = null, DiscNumber = 0, Year = null };

        Assert.True(tags.Matches(values));
    }

    [Fact]
    public void Matches_IgnoresEmptyEntriesInTheLists()
    {
        Assert.True((Tags() with { ArtistList = ["Main", "", "Guest"] }).Matches(Values()));
    }

    [Fact]
    public void DoesNotMatch_WhenOneArtistDiffers()
    {
        Assert.False((Tags() with { ArtistList = ["Main", "Other"] }).Matches(Values()));
    }

    [Fact]
    public void DoesNotMatch_WhenTheArtistsAreInAnotherOrder()
    {
        Assert.False((Tags() with { ArtistList = ["Guest", "Main"] }).Matches(Values()));
    }

    [Fact]
    public void DoesNotMatch_WhenOneComparedFieldDiffers()
    {
        Assert.False((Tags() with { Year = 2000 }).Matches(Values()));
        Assert.False((Tags() with { Title = "Other" }).Matches(Values()));
        Assert.False((Tags() with { GenreList = [] }).Matches(Values()));
    }

    [Fact]
    public void DoesNotMatch_TheEmptySet()
    {
        Assert.False(MediaTagSet.Empty.Matches(Values()));
    }
}
