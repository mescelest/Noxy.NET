using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;

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
}
