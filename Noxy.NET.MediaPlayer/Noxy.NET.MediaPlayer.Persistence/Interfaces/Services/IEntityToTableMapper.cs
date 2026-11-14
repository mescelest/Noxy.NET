using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;

namespace Noxy.NET.MediaPlayer.Persistence.Interfaces.Services;

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

    #endregion -- Media --
}
