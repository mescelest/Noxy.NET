using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Interfaces.Services;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;

namespace Noxy.NET.MediaPlayer.Persistence.Services;

public class EntityToTableMapper : IEntityToTableMapper
{
    #region -- Authentication --

    public TableExternalLogin Map(ExternalLogin entity)
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

    public TableRefreshToken Map(RefreshToken entity)
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

    public TableUser Map(User entity)
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

    public TableMediaFile Map(MediaFile entity)
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

    public TableMediaFileMetadata Map(MediaFileMetadata entity)
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

    public TableMediaUploadJob Map(MediaUploadJob entity)
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

    public TablePlaylist Map(Playlist entity)
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

    public TablePlaylistEntry Map(PlaylistEntry entity)
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
