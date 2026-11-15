using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;
using Noxy.NET.MediaPlayer.Persistence.Tables.Metadata;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playback;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;
using Noxy.NET.MediaPlayer.Persistence.Tables.Setting;

namespace Noxy.NET.MediaPlayer.Persistence.Interfaces.Mappers;

public interface IEntityToTableMapper
{
    #region -- Authentication --

    TableExternalLogin Map(EntityExternalLogin entity);
    TableRefreshToken Map(EntityRefreshToken entity);
    TableUser Map(EntityUser entity);

    #endregion -- Authentication --

    #region -- Media --

    TableMediaFile Map(EntityMediaFile entity);
    TableMediaFileMetadata Map(EntityMediaFileMetadata entity);
    TableMediaUploadJob Map(EntityMediaUploadJob entity);

    #endregion -- Media --

    #region -- Playlist --

    TablePlaylist Map(EntityPlaylist entity);
    TablePlaylistEntry Map(EntityPlaylistEntry entity);

    #endregion -- Playlist --

    #region -- Metadata --

    TableArtist Map(EntityArtist entity);
    TableGenre Map(EntityGenre entity);
    TableAlbum Map(EntityAlbum entity);
    TableAlbumArtist Map(EntityAlbumArtist entity);
    TableMediaFileArtist Map(EntityMediaFileArtist entity);
    TableMediaFileGenre Map(EntityMediaFileGenre entity);
    TableUserSetting Map(EntityUserSetting entity);

    #endregion -- Metadata --

    #region -- Playback --

    TablePlaybackSession Map(EntityPlaybackSession entity);

    #endregion -- Playback --
}
