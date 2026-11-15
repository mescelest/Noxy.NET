using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;
using Noxy.NET.MediaPlayer.Persistence.Tables.Metadata;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playback;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;
using Noxy.NET.MediaPlayer.Persistence.Tables.Setting;

namespace Noxy.NET.MediaPlayer.Persistence.Interfaces.Mappers;

public interface ITableToEntityMapper
{
    #region -- Authentication --

    EntityExternalLogin Map(TableExternalLogin table);
    EntityRefreshToken Map(TableRefreshToken table);
    EntityUser Map(TableUser table);

    #endregion -- Authentication --

    #region -- Media --

    EntityMediaFile Map(TableMediaFile table);
    EntityMediaFileMetadata Map(TableMediaFileMetadata table);
    EntityMediaUploadJob Map(TableMediaUploadJob table);

    #endregion -- Media --

    #region -- Playlist --

    EntityPlaylist Map(TablePlaylist table);
    EntityPlaylistEntry Map(TablePlaylistEntry table);

    #endregion -- Playlist --

    #region -- Metadata --

    EntityArtist Map(TableArtist table);
    EntityGenre Map(TableGenre table);
    EntityAlbum Map(TableAlbum table);
    EntityAlbumArtist Map(TableAlbumArtist table);
    EntityMediaFileArtist Map(TableMediaFileArtist table);
    EntityMediaFileGenre Map(TableMediaFileGenre table);
    EntityUserSetting Map(TableUserSetting table);

    #endregion -- Metadata --

    #region -- Playback --

    EntityPlaybackSession Map(TablePlaybackSession table);

    #endregion -- Playback --
}
