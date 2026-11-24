namespace Noxy.NET.MediaPlayer.Domain.Enums;

public enum MediaHubEventEnum
{
    UploadSucceeded,
    UploadFailed,
    MediaUpdated,
    MediaDeleted,
    PlaylistUpdated,
    PlaylistDeleted,
    PlaylistOrderChanged,
    ArtistUpdated,
    ArtistDeleted,
    AlbumUpdated,
    AlbumDeleted,
    GenreUpdated,
    GenreDeleted,
    SettingUpdated,
    DeviceListChanged,
    PlayerStateChanged,
    QueueChanged,
    QueueSnapshot,
    SessionEnded,
    TransferFailed,
    PlayerCommandReceived,
    PlayerTransferRequested,
    PlayerStartRequested
}
