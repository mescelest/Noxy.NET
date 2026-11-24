using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class PlaybackDisplayTest
{
    private static readonly Guid MediaID = Guid.CreateVersion7();

    private static ResponseMediaFile Media(string title, string? artist, TimeSpan? duration)
    {
        return new()
        {
            ID = MediaID,
            Title = title,
            Artist = artist,
            Duration = duration,
            ContentType = "audio/mpeg",
            SizeBytes = 1,
            ContentHash = "h",
            TimeCreated = DateTime.UtcNow,
            TimeModified = DateTime.UtcNow
        };
    }

    private static PlayerSnapshot Snapshot(bool isPlaying = false, bool isWaiting = false, double volume = 1, ShuffleModeEnum shuffle = ShuffleModeEnum.Off, RepeatModeEnum repeat = RepeatModeEnum.No, bool hasCurrent = true)
    {
        return new(hasCurrent ? new QueueItem(Guid.CreateVersion7(), MediaID) : null, false, isPlaying, isWaiting, TimeSpan.Zero, TimeSpan.FromSeconds(200), volume, shuffle, repeat, null, 1);
    }

    private static ResponsePlayerState Remote(bool isPlaying = true, int volume = 40, ShuffleModeEnum shuffle = ShuffleModeEnum.Random, RepeatModeEnum repeat = RepeatModeEnum.Once, Guid? currentItemID = null, string? title = "Remote title",
        string? artist = "Remote artist")
    {
        return new()
        {
            ActiveDeviceID = Guid.CreateVersion7(),
            StateRevision = 1,
            IsPlaying = isPlaying,
            PositionSeconds = 10,
            ServerTime = DateTime.UtcNow,
            Volume = volume,
            Shuffle = shuffle,
            Repeat = repeat,
            CurrentItemID = currentItemID ?? MediaID,
            Title = title,
            Artist = artist
        };
    }

    private static ResponseMediaFile? Library(Guid id)
    {
        return id == MediaID ? Media("Library title", "Library artist", TimeSpan.FromSeconds(180)) : null;
    }

    [Fact]
    public void Of_OnThisDevice_ShowsItsOwnPlayer()
    {
        PlaybackDisplay display = PlaybackDisplay.Of(Snapshot(isPlaying: true, volume: 0.756, shuffle: ShuffleModeEnum.SpreadArtists, repeat: RepeatModeEnum.Forever), null, TimeSpan.FromSeconds(12), Library, null, false);

        Assert.False(display.IsController);
        Assert.True(display.IsPlaying);
        Assert.Equal(TimeSpan.FromSeconds(12), display.Position);
        Assert.Equal(TimeSpan.FromSeconds(200), display.Duration);
        Assert.Equal(76, display.VolumePercent);
        Assert.Equal(ShuffleModeEnum.SpreadArtists, display.Shuffle);
        Assert.Equal(RepeatModeEnum.Forever, display.Repeat);
        Assert.Equal("Library title", display.Title);
        Assert.Equal("Library artist", display.Artist);
    }

    [Fact]
    public void Of_OnThisDevice_WithNothingLoaded_HasNoTitleAndNoArtist()
    {
        PlaybackDisplay display = PlaybackDisplay.Of(Snapshot(hasCurrent: false), null, TimeSpan.Zero, Library, null, false);

        Assert.Equal(string.Empty, display.Title);
        Assert.Equal(string.Empty, display.Artist);
    }

    [Fact]
    public void Of_AsAController_ShowsTheActiveDevicesState()
    {
        PlaybackDisplay display = PlaybackDisplay.Of(Snapshot(isPlaying: false, volume: 1), Remote(), TimeSpan.FromSeconds(31), Library, "Phone", false);

        Assert.True(display.IsController);
        Assert.True(display.IsPlaying);
        Assert.Equal(TimeSpan.FromSeconds(31), display.Position);
        Assert.Equal(TimeSpan.FromSeconds(180), display.Duration);
        Assert.Equal(40, display.VolumePercent);
        Assert.Equal(ShuffleModeEnum.Random, display.Shuffle);
        Assert.Equal(RepeatModeEnum.Once, display.Repeat);
        Assert.Equal("Remote title", display.Title);
        Assert.Equal("Remote artist", display.Artist);
    }

    [Fact]
    public void Of_AsAController_WithAMediaFileThisDeviceDoesNotKnow_HasNoDuration_AndWithoutTitleOrArtistShowsNone()
    {
        PlaybackDisplay display = PlaybackDisplay.Of(Snapshot(), Remote(currentItemID: Guid.CreateVersion7(), title: null, artist: null), TimeSpan.Zero, _ => null, "Phone", false);

        Assert.Equal(TimeSpan.Zero, display.Duration);
        Assert.Equal(string.Empty, display.Title);
        Assert.Equal(string.Empty, display.Artist);
    }

    [Fact]
    public void Status_AsAController_NamesTheActiveDevice_AndWinsOverTheOthers()
    {
        PlaybackDisplay named = PlaybackDisplay.Of(Snapshot(isWaiting: true), Remote(), TimeSpan.Zero, Library, "Phone", true);
        PlaybackDisplay unnamed = PlaybackDisplay.Of(Snapshot(), Remote(), TimeSpan.Zero, Library, null, false);

        Assert.Equal(string.Format(PresentationTexts.PlayerStatusPlayingOn, "Phone"), named.Status);
        Assert.Equal(string.Format(PresentationTexts.PlayerStatusPlayingOn, PresentationTexts.PlayerStatusAnotherDevice), unnamed.Status);
    }

    [Fact]
    public void Status_OnThisDevice_IsWaitingBeforeOffline_AndEmptyOtherwise()
    {
        Assert.Equal(PresentationTexts.PlayerStatusWaiting, PlaybackDisplay.Of(Snapshot(isWaiting: true), null, TimeSpan.Zero, Library, null, true).Status);
        Assert.Equal(PresentationTexts.PlayerStatusOffline, PlaybackDisplay.Of(Snapshot(), null, TimeSpan.Zero, Library, null, true).Status);
        Assert.Equal(string.Empty, PlaybackDisplay.Of(Snapshot(), null, TimeSpan.Zero, Library, null, false).Status);
    }

    [Fact]
    public void Of_CarriesTheAlbumOfTheCurrentMedia_OnThisDevice_AndOnARemoteOne()
    {
        Guid albumID = Guid.CreateVersion7();

        ResponseMediaFile? WithAlbum(Guid id) => Library(id) is { } media
            ? new() { ID = media.ID, Title = media.Title, ContentType = media.ContentType, SizeBytes = 1, ContentHash = "h", TimeCreated = media.TimeCreated, TimeModified = media.TimeModified, AlbumID = albumID }
            : null;

        PlaybackDisplay local = PlaybackDisplay.Of(Snapshot(), null, TimeSpan.Zero, WithAlbum, null, false);
        PlaybackDisplay remote = PlaybackDisplay.Of(Snapshot(), Remote(), TimeSpan.Zero, WithAlbum, "Phone", false);

        Assert.Equal(albumID, local.AlbumID);
        Assert.Equal(albumID, remote.AlbumID);
    }

    [Fact]
    public void Of_WithNoCurrentMedia_OrOneWithNoAlbum_HasNoAlbum()
    {
        Assert.Null(PlaybackDisplay.Of(Snapshot(hasCurrent: false), null, TimeSpan.Zero, Library, null, false).AlbumID);
        Assert.Null(PlaybackDisplay.Of(Snapshot(), null, TimeSpan.Zero, Library, null, false).AlbumID);
        Assert.Null(PlaybackDisplay.Of(Snapshot(), Remote(currentItemID: Guid.CreateVersion7()), TimeSpan.Zero, Library, "Phone", false).AlbumID);
    }
}
