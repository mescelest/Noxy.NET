using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;

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
}
