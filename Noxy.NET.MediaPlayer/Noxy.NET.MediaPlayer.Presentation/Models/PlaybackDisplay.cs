using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

// What the player bar shows. A controller shows what the active device reports; any other device shows its own player.
public sealed record PlaybackDisplay(
    bool IsController,
    bool IsPlaying,
    TimeSpan Position,
    TimeSpan Duration,
    int VolumePercent,
    ShuffleModeEnum Shuffle,
    RepeatModeEnum Repeat,
    string Title,
    string Artist,
    string Status,
    Guid? AlbumID = null)
{
    // remote is the active device's state when this device is a controller, otherwise null. position is read by the caller, since it differs between the two and moves on every tick.
    public static PlaybackDisplay Of(PlayerSnapshot snapshot, ResponsePlayerState? remote, TimeSpan position, Func<Guid, ResponseMediaFile?> mediaOf, string? activeDeviceName, bool isOffline)
    {
        if (remote is not null)
        {
            ResponseMediaFile? remoteMedia = remote.CurrentItemID is { } remoteID ? mediaOf(remoteID) : null;
            string status = string.Format(PresentationTexts.PlayerStatusPlayingOn, activeDeviceName ?? PresentationTexts.PlayerStatusAnotherDevice);

            return new(true, remote.IsPlaying, position, remoteMedia?.Duration ?? TimeSpan.Zero, remote.Volume, remote.Shuffle, remote.Repeat, remote.Title ?? string.Empty, remote.Artist ?? string.Empty, status, remoteMedia?.AlbumID);
        }

        ResponseMediaFile? media = snapshot.Current is { } current ? mediaOf(current.MediaFileID) : null;

        return new(false, snapshot.IsPlaying, position, snapshot.Duration, (int)Math.Round(snapshot.Volume * 100), snapshot.Shuffle, snapshot.Repeat, media?.Title ?? string.Empty, media?.Artist ?? string.Empty, LocalStatus(snapshot, isOffline),
            media?.AlbumID);
    }

    private static string LocalStatus(PlayerSnapshot snapshot, bool isOffline)
    {
        if (snapshot.IsWaiting) return PresentationTexts.PlayerStatusWaiting;

        return isOffline ? PresentationTexts.PlayerStatusOffline : string.Empty;
    }
}
