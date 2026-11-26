using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Interfaces.Mappers;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;
using Noxy.NET.MediaPlayer.Persistence.Tables.Metadata;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;
using Noxy.NET.MediaPlayer.Persistence.Tables.Setting;

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
            AlbumID = entity.AlbumID,
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
            TitleWasSent = entity.TitleWasSent,
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
            Position = entity.Position,
            TimeModified = entity.TimeModified,
            TimeLastPlayed = entity.TimeLastPlayed,
            ArtHue = entity.ArtHue,
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

    #region -- Metadata --

    public TableArtist Map(EntityArtist entity)
    {
        return new()
        {
            ID = entity.ID,
            TimeCreated = entity.TimeCreated,
            UserID = entity.UserID,
            Name = entity.Name,
            NameKey = entity.NameKey,
            TimeModified = entity.TimeModified
        };
    }

    public TableGenre Map(EntityGenre entity)
    {
        return new()
        {
            ID = entity.ID,
            TimeCreated = entity.TimeCreated,
            UserID = entity.UserID,
            Name = entity.Name,
            NameKey = entity.NameKey,
            TimeModified = entity.TimeModified
        };
    }

    public TableAlbum Map(EntityAlbum entity)
    {
        return new()
        {
            ID = entity.ID,
            TimeCreated = entity.TimeCreated,
            UserID = entity.UserID,
            Title = entity.Title,
            TitleKey = entity.TitleKey,
            IdentityKey = entity.IdentityKey,
            TimeModified = entity.TimeModified,
            TimeArtModified = entity.TimeArtModified,
            ArtistLinkList = [.. entity.ArtistLinkList.Select(Map)]
        };
    }

    public TableAlbumArtist Map(EntityAlbumArtist entity)
    {
        return new()
        {
            ID = entity.ID,
            TimeCreated = entity.TimeCreated,
            AlbumID = entity.AlbumID,
            ArtistID = entity.ArtistID,
            Position = entity.Position
        };
    }

    public TableMediaFileArtist Map(EntityMediaFileArtist entity)
    {
        return new()
        {
            ID = entity.ID,
            TimeCreated = entity.TimeCreated,
            MediaFileID = entity.MediaFileID,
            ArtistID = entity.ArtistID,
            Position = entity.Position
        };
    }

    public TableMediaFileGenre Map(EntityMediaFileGenre entity)
    {
        return new()
        {
            ID = entity.ID,
            TimeCreated = entity.TimeCreated,
            MediaFileID = entity.MediaFileID,
            GenreID = entity.GenreID,
            Position = entity.Position
        };
    }

    public TableUserSetting Map(EntityUserSetting entity)
    {
        return entity switch
        {
            EntityUserSettingBoolean boolean => new TableUserSettingBoolean { ID = boolean.ID, TimeCreated = boolean.TimeCreated, UserID = boolean.UserID, Key = boolean.Key, TimeModified = boolean.TimeModified, Value = boolean.Value },
            EntityUserSettingInteger integer => new TableUserSettingInteger { ID = integer.ID, TimeCreated = integer.TimeCreated, UserID = integer.UserID, Key = integer.Key, TimeModified = integer.TimeModified, Value = integer.Value },
            EntityUserSettingEnum member => new TableUserSettingEnum { ID = member.ID, TimeCreated = member.TimeCreated, UserID = member.UserID, Key = member.Key, TimeModified = member.TimeModified, Value = member.Value },
            _ => throw new NotSupportedException($"There is no table for {entity.GetType().Name}.")
        };
    }

    #endregion -- Metadata --
}
