using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class ResponsePlaylistExtensionTest
{
    private static ResponsePlaylist Playlist(int? artHue)
    {
        DateTime now = new(2026, 11, 24, 0, 0, 0, DateTimeKind.Utc);
        return new() { ID = Guid.CreateVersion7(), Name = "Mix", AllowDuplicates = true, EntryCount = 0, TimeCreated = now, TimeModified = now, TimeLastPlayed = null, ArtHue = artHue };
    }

    [Fact]
    public void ArtHueCurrent_UsesTheChosenHue()
    {
        Assert.Equal(200, Playlist(200).ArtHueCurrent);
    }

    [Fact]
    public void ArtHueCurrent_WhenNoneIsChosen_IsTheHueFromTheID_AndStaysTheSame()
    {
        ResponsePlaylist playlist = Playlist(null);

        Assert.Equal(playlist.ArtHueCurrent, playlist.ArtHueCurrent);
        Assert.InRange(playlist.ArtHueCurrent, 0, 359);
    }

    [Fact]
    public void ArtHueCurrent_WhenNoneIsChosen_FollowsTheID()
    {
        List<int> hueList = [.. Enumerable.Range(0, 50).Select(_ => Playlist(null).ArtHueCurrent)];

        Assert.True(hueList.Distinct().Count() > 1);
    }

    [Fact]
    public void ArtHueCurrent_AChosenZero_IsZero_NotTheDefault()
    {
        Assert.Equal(0, Playlist(0).ArtHueCurrent);
    }

    [Fact]
    public void ArtHueDefault_IgnoresTheChosenHue_AndIsWhatCurrentFallsBackTo()
    {
        ResponsePlaylist chosen = Playlist(200);
        ResponsePlaylist none = new() { ID = chosen.ID, Name = "Mix", AllowDuplicates = true, EntryCount = 0, TimeCreated = chosen.TimeCreated, TimeModified = chosen.TimeModified, TimeLastPlayed = null };

        Assert.Equal(none.ArtHueCurrent, chosen.ArtHueDefault);
        Assert.Equal(none.ArtHueDefault, none.ArtHueCurrent);
        Assert.InRange(chosen.ArtHueDefault, 0, 359);
    }
}
