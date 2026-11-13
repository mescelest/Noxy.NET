using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

namespace Noxy.NET.MediaPlayer.Persistence.Mappers;

public static class UserMapper
{
    public static User ToEntity(this TableUser table)
    {
        return new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            Email = table.Email,
            IsEmailVerified = table.IsEmailVerified,
            PasswordHash = table.PasswordHash,
            ExternalLogins = [.. table.ExternalLoginList.Select(x => x.ToEntity())]
        };
    }

    public static TableUser ToTable(this User entity)
    {
        return new()
        {
            ID = entity.ID,
            TimeCreated = entity.TimeCreated,
            Email = entity.Email,
            IsEmailVerified = entity.IsEmailVerified,
            PasswordHash = entity.PasswordHash,
            ExternalLoginList = [.. entity.ExternalLogins.Select(x => x.ToTable())]
        };
    }

    public static ExternalLogin ToEntity(this TableExternalLogin table)
    {
        return new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            Provider = table.Provider,
            ProviderKey = table.ProviderKey,
            UserID = table.UserID
        };
    }

    public static TableExternalLogin ToTable(this ExternalLogin entity)
    {
        return new()
        {
            ID = entity.ID,
            TimeCreated = entity.TimeCreated,
            Provider = entity.Provider,
            ProviderKey = entity.ProviderKey,
            UserID = entity.UserID
        };
    }
}
