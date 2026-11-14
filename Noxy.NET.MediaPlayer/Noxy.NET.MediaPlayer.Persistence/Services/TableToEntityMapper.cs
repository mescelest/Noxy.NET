using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Interfaces.Services;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;

namespace Noxy.NET.MediaPlayer.Persistence.Services;

public class TableToEntityMapper : ITableToEntityMapper
{
    #region -- Authentication --

    public ExternalLogin Map(TableExternalLogin table)
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

    public RefreshToken Map(TableRefreshToken table)
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

    public User Map(TableUser table)
    {
        return new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            Email = table.Email,
            IsEmailVerified = table.IsEmailVerified,
            PasswordHash = table.PasswordHash,
            ExternalLoginList = [.. table.ExternalLoginList.Select(Map)]
        };
    }

    #endregion -- Authentication --

    #region -- Media --

    public MediaFile Map(TableMediaFile table)
    {
        return new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            FilePath = table.FilePath,
            ContentType = table.ContentType,
            SizeBytes = table.SizeBytes,
            ContentHash = table.ContentHash,
            TimeModified = table.TimeModified,
            UserID = table.UserID,
            Metadata = table.Metadata is null ? null : Map(table.Metadata)
        };
    }

    public MediaFileMetadata Map(TableMediaFileMetadata table)
    {
        return new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            Title = table.Title,
            Artist = table.Artist,
            Album = table.Album,
            AlbumArtist = table.AlbumArtist,
            Genre = table.Genre,
            Subtitle = table.Subtitle,
            Comment = table.Comment,
            TrackNumber = table.TrackNumber,
            DiscNumber = table.DiscNumber,
            Year = table.Year,
            Duration = table.Duration,
            MediaFileID = table.MediaFileID
        };
    }

    public MediaUploadJob Map(TableMediaUploadJob table)
    {
        return new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            UserID = table.UserID,
            StagedFileName = table.StagedFileName,
            Title = table.Title,
            ContentType = table.ContentType,
            SizeBytes = table.SizeBytes,
            ContentHash = table.ContentHash,
            Status = table.Status
        };
    }

    #endregion -- Media --
}
