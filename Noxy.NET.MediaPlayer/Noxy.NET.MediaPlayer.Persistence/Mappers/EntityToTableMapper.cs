using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Interfaces.Mappers;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;

namespace Noxy.NET.MediaPlayer.Persistence.Mappers;

public class EntityToTableMapper : IEntityToTableMapper
{
    #region -- Authentication --

    public TableExternalLogin Map(EntityExternalLogin entity)
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

    public TableRefreshToken Map(EntityRefreshToken entity)
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

    public TableUser Map(EntityUser entity)
    {
        return new()
        {
            ID = entity.ID,
            TimeCreated = entity.TimeCreated,
            Email = entity.Email,
            IsEmailVerified = entity.IsEmailVerified,
            PasswordHash = entity.PasswordHash,
            ExternalLoginList = [.. entity.ExternalLoginList.Select(Map)]
        };
    }

    #endregion -- Authentication --

    #region -- Media --

    public TableMediaFile Map(EntityMediaFile entity)
    {
        return new()
        {
            ID = entity.ID,
            TimeCreated = entity.TimeCreated,
            FilePath = entity.FilePath,
            ContentType = entity.ContentType,
            SizeBytes = entity.SizeBytes,
            ContentHash = entity.ContentHash,
            TimeModified = entity.TimeModified,
            UserID = entity.UserID,
            Metadata = entity.Metadata is null ? null : Map(entity.Metadata)
        };
    }

    public TableMediaFileMetadata Map(EntityMediaFileMetadata entity)
    {
        return new()
        {
            ID = entity.ID,
            TimeCreated = entity.TimeCreated,
            Title = entity.Title,
            Artist = entity.Artist,
            Album = entity.Album,
            AlbumArtist = entity.AlbumArtist,
            Genre = entity.Genre,
            Subtitle = entity.Subtitle,
            Comment = entity.Comment,
            TrackNumber = entity.TrackNumber,
            DiscNumber = entity.DiscNumber,
            Year = entity.Year,
            Duration = entity.Duration,
            MediaFileID = entity.MediaFileID
        };
    }

    public TableMediaUploadJob Map(EntityMediaUploadJob entity)
    {
        return new()
        {
            ID = entity.ID,
            TimeCreated = entity.TimeCreated,
            UserID = entity.UserID,
            StagedFileName = entity.StagedFileName,
            Title = entity.Title,
            ContentType = entity.ContentType,
            SizeBytes = entity.SizeBytes,
            ContentHash = entity.ContentHash,
            Status = entity.Status
        };
    }

    #endregion -- Media --

    #region -- Playlist --

    public TablePlaylist Map(EntityPlaylist entity)
    {
        return new()
        {
            ID = entity.ID,
            TimeCreated = entity.TimeCreated,
            UserID = entity.UserID,
            Name = entity.Name,
            AllowDuplicates = entity.AllowDuplicates,
            TimeModified = entity.TimeModified,
            EntryList = [.. entity.EntryList.Select(Map)]
        };
    }

    public TablePlaylistEntry Map(EntityPlaylistEntry entity)
    {
        return new()
        {
            ID = entity.ID,
            TimeCreated = entity.TimeCreated,
            PlaylistID = entity.PlaylistID,
            MediaFileID = entity.MediaFileID,
            Position = entity.Position
        };
    }

    #endregion -- Playlist --
}
