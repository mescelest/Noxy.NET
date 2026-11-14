using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;

namespace Noxy.NET.MediaPlayer.Persistence.Interfaces.Mappers;

public interface IEntityToTableMapper
{
    #region -- Authentication --

    TableExternalLogin Map(ExternalLogin entity);
    TableRefreshToken Map(RefreshToken entity);
    TableUser Map(User entity);

    #endregion -- Authentication --

    #region -- Media --

    TableMediaFile Map(MediaFile entity);
    TableMediaFileMetadata Map(MediaFileMetadata entity);
    TableMediaUploadJob Map(MediaUploadJob entity);

    #endregion -- Media --

    #region -- Playlist --

    TablePlaylist Map(Playlist entity);
    TablePlaylistEntry Map(PlaylistEntry entity);

    #endregion -- Playlist --
}
