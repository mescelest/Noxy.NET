using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;

namespace Noxy.NET.MediaPlayer.Persistence.Interfaces.Services;

public interface ITableToEntityMapper
{
    #region -- Authentication --

    ExternalLogin Map(TableExternalLogin table);
    RefreshToken Map(TableRefreshToken table);
    User Map(TableUser table);

    #endregion -- Authentication --

    #region -- Media --

    MediaFile Map(TableMediaFile table);
    MediaFileMetadata Map(TableMediaFileMetadata table);
    MediaUploadJob Map(TableMediaUploadJob table);

    #endregion -- Media --

    #region -- Playlist --

    Playlist Map(TablePlaylist table);
    PlaylistEntry Map(TablePlaylistEntry table);

    #endregion -- Playlist --
}
