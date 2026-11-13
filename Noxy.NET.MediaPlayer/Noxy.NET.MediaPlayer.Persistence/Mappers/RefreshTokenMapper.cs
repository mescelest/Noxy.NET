using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

namespace Noxy.NET.MediaPlayer.Persistence.Mappers;

public static class RefreshTokenMapper
{
    public static RefreshToken ToEntity(this TableRefreshToken table)
    {
        return new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            TokenHash = table.TokenHash,
            TimeExpires = table.TimeExpires,
            TimeRevoked = table.TimeRevoked,
            UserID = table.UserID
        };
    }

    public static TableRefreshToken ToTable(this RefreshToken entity)
    {
        return new()
        {
            ID = entity.ID,
            TimeCreated = entity.TimeCreated,
            TokenHash = entity.TokenHash,
            TimeExpires = entity.TimeExpires,
            TimeRevoked = entity.TimeRevoked,
            UserID = entity.UserID
        };
    }
}
