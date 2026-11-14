using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;

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

    #endregion -- Media --
}
