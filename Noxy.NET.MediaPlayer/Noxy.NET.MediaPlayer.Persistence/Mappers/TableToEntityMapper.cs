using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Interfaces.Mappers;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;

namespace Noxy.NET.MediaPlayer.Persistence.Mappers;

public class TableToEntityMapper : ITableToEntityMapper
{
    #region -- Authentication --

    public EntityExternalLogin Map(TableExternalLogin table)
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

    public EntityRefreshToken Map(TableRefreshToken table)
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

    public EntityUser Map(TableUser table)
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

    public EntityMediaFile Map(TableMediaFile table)
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

    public EntityMediaFileMetadata Map(TableMediaFileMetadata table)
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

    public EntityMediaUploadJob Map(TableMediaUploadJob table)
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

    #region -- Playlist --

    public EntityPlaylist Map(TablePlaylist table)
    {
        return new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            UserID = table.UserID,
            Name = table.Name,
            AllowDuplicates = table.AllowDuplicates,
            TimeModified = table.TimeModified,
            EntryList = [.. table.EntryList.Select(Map)]
        };
    }

    public EntityPlaylistEntry Map(TablePlaylistEntry table)
    {
        return new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            PlaylistID = table.PlaylistID,
            MediaFileID = table.MediaFileID,
            Position = table.Position,
            MediaFile = table.MediaFile is null ? null : Map(table.MediaFile)
        };
    }

    #endregion -- Playlist --
}
