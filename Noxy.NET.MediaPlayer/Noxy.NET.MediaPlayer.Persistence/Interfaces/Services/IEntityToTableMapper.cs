using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

namespace Noxy.NET.MediaPlayer.Persistence.Interfaces.Services;

public interface IEntityToTableMapper
{
    #region -- Authentication --

    TableExternalLogin Map(ExternalLogin entity);
    TableRefreshToken Map(RefreshToken entity);
    TableUser Map(User entity);

    #endregion -- Authentication --
}
