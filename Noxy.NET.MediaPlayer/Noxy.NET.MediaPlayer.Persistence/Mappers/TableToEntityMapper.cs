using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Interfaces.Mappers;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;
using Noxy.NET.MediaPlayer.Persistence.Tables.Metadata;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;
using Noxy.NET.MediaPlayer.Persistence.Tables.Setting;

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
            AlbumID = table.AlbumID,
            Album = table.Album is null ? null : Map(table.Album),
            ArtistLinkList = [.. table.ArtistLinkList.OrderBy(x => x.Position).Select(Map)],
            GenreLinkList = [.. table.GenreLinkList.OrderBy(x => x.Position).Select(Map)],
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
            TitleWasSent = table.TitleWasSent,
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

    #region -- Metadata --

    public EntityArtist Map(TableArtist table)
    {
        return new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            UserID = table.UserID,
            Name = table.Name,
            NameKey = table.NameKey,
            TimeModified = table.TimeModified
        };
    }

    public EntityGenre Map(TableGenre table)
    {
        return new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            UserID = table.UserID,
            Name = table.Name,
            NameKey = table.NameKey,
            TimeModified = table.TimeModified
        };
    }

    public EntityAlbum Map(TableAlbum table)
    {
        return new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            UserID = table.UserID,
            Title = table.Title,
            TitleKey = table.TitleKey,
            IdentityKey = table.IdentityKey,
            TimeModified = table.TimeModified,
            ArtistLinkList = [.. table.ArtistLinkList.OrderBy(x => x.Position).Select(Map)]
        };
    }

    public EntityAlbumArtist Map(TableAlbumArtist table)
    {
        return new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            AlbumID = table.AlbumID,
            ArtistID = table.ArtistID,
            Position = table.Position,
            Artist = table.Artist is null ? null : Map(table.Artist)
        };
    }

    public EntityMediaFileArtist Map(TableMediaFileArtist table)
    {
        return new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            MediaFileID = table.MediaFileID,
            ArtistID = table.ArtistID,
            Position = table.Position,
            Artist = table.Artist is null ? null : Map(table.Artist)
        };
    }

    public EntityMediaFileGenre Map(TableMediaFileGenre table)
    {
        return new()
        {
            ID = table.ID,
            TimeCreated = table.TimeCreated,
            MediaFileID = table.MediaFileID,
            GenreID = table.GenreID,
            Position = table.Position,
            Genre = table.Genre is null ? null : Map(table.Genre)
        };
    }

    public EntityUserSetting Map(TableUserSetting table)
    {
        return table switch
        {
            TableUserSettingBoolean boolean => new EntityUserSettingBoolean { ID = boolean.ID, TimeCreated = boolean.TimeCreated, UserID = boolean.UserID, Key = boolean.Key, TimeModified = boolean.TimeModified, Value = boolean.Value },
            TableUserSettingInteger integer => new EntityUserSettingInteger { ID = integer.ID, TimeCreated = integer.TimeCreated, UserID = integer.UserID, Key = integer.Key, TimeModified = integer.TimeModified, Value = integer.Value },
            TableUserSettingEnum member => new EntityUserSettingEnum { ID = member.ID, TimeCreated = member.TimeCreated, UserID = member.UserID, Key = member.Key, TimeModified = member.TimeModified, Value = member.Value },
            _ => throw new NotSupportedException($"There is no entity for {table.GetType().Name}.")
        };
    }

    #endregion -- Metadata --
}
