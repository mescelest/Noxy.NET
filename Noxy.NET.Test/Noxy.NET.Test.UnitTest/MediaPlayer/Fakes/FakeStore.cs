using System.Diagnostics.CodeAnalysis;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeStore
{
    public List<EntityUser> UserList { get; } = [];
    public List<EntityExternalLogin> ExternalLoginList { get; } = [];
    public List<EntityRefreshToken> RefreshTokenList { get; } = [];
    public List<EntityMediaFile> MediaFileList { get; } = [];
    public List<EntityMediaUploadJob> MediaUploadJobList { get; } = [];
    public List<EntityPlaylist> PlaylistList { get; } = [];
    public List<EntityArtist> ArtistList { get; } = [];
    public List<EntityGenre> GenreList { get; } = [];
    public List<EntityAlbum> AlbumList { get; } = [];
    public List<EntityUserSetting> UserSettingList { get; } = [];

    public int CommitCount { get; set; }
    public bool FailCommit { get; set; }
    public int FailCommitCount { get; set; }
    public int KeyConflictCommitCount { get; set; }
    public Action? OnCommit { get; set; }
    public bool LoseJobClaim { get; set; }
    public int RevokeAllCount { get; set; }
    public bool LoseRevokeRace { get; set; }

    // A copy shaped like the database hands a media file back for a response: links in position order, each with its item, and the album with its artists.
    public EntityMediaFile CopyMediaFile(EntityMediaFile file)
    {
        EntityMediaFile copy = new()
        {
            ID = file.ID,
            TimeCreated = file.TimeCreated,
            TimeModified = file.TimeModified,
            FilePath = file.FilePath,
            ContentType = file.ContentType,
            SizeBytes = file.SizeBytes,
            ContentHash = file.ContentHash,
            UserID = file.UserID,
            AlbumID = file.AlbumID,
            Album = file.AlbumID is null ? null : CopyAlbum(AlbumList.FirstOrDefault(x => x.ID == file.AlbumID)),
            ArtistLinkList =
            [
                .. file.ArtistLinkList.OrderBy(x => x.Position).Select(x => new EntityMediaFileArtist
                    { ID = x.ID, TimeCreated = x.TimeCreated, MediaFileID = x.MediaFileID, ArtistID = x.ArtistID, Position = x.Position, Artist = CopyArtist(ArtistList.FirstOrDefault(a => a.ID == x.ArtistID)) })
            ],
            GenreLinkList =
            [
                .. file.GenreLinkList.OrderBy(x => x.Position).Select(x => new EntityMediaFileGenre
                    { ID = x.ID, TimeCreated = x.TimeCreated, MediaFileID = x.MediaFileID, GenreID = x.GenreID, Position = x.Position, Genre = CopyGenre(GenreList.FirstOrDefault(g => g.ID == x.GenreID)) })
            ]
        };

        if (file.Metadata is not null)
        {
            copy.Metadata = new()
            {
                ID = file.Metadata.ID,
                TimeCreated = file.Metadata.TimeCreated,
                Title = file.Metadata.Title,
                Artist = file.Metadata.Artist,
                Album = file.Metadata.Album,
                AlbumArtist = file.Metadata.AlbumArtist,
                Subtitle = file.Metadata.Subtitle,
                Comment = file.Metadata.Comment,
                Genre = file.Metadata.Genre,
                TrackNumber = file.Metadata.TrackNumber,
                DiscNumber = file.Metadata.DiscNumber,
                Year = file.Metadata.Year,
                Duration = file.Metadata.Duration,
                MediaFileID = file.Metadata.MediaFileID
            };
        }

        return copy;
    }

    [return: NotNullIfNotNull(nameof(album))]
    public EntityAlbum? CopyAlbum(EntityAlbum? album)
    {
        if (album is null) return null;

        return new()
        {
            ID = album.ID,
            TimeCreated = album.TimeCreated,
            UserID = album.UserID,
            Title = album.Title,
            TitleKey = album.TitleKey,
            IdentityKey = album.IdentityKey,
            TimeModified = album.TimeModified,
            ArtistLinkList =
            [
                .. album.ArtistLinkList.OrderBy(x => x.Position).Select(x => new EntityAlbumArtist
                    { ID = x.ID, TimeCreated = x.TimeCreated, AlbumID = x.AlbumID, ArtistID = x.ArtistID, Position = x.Position, Artist = CopyArtist(ArtistList.FirstOrDefault(a => a.ID == x.ArtistID)) })
            ]
        };
    }

    [return: NotNullIfNotNull(nameof(artist))]
    public static EntityArtist? CopyArtist(EntityArtist? artist)
    {
        return artist is null ? null : new() { ID = artist.ID, TimeCreated = artist.TimeCreated, UserID = artist.UserID, Name = artist.Name, NameKey = artist.NameKey, TimeModified = artist.TimeModified };
    }

    [return: NotNullIfNotNull(nameof(genre))]
    public static EntityGenre? CopyGenre(EntityGenre? genre)
    {
        return genre is null ? null : new() { ID = genre.ID, TimeCreated = genre.TimeCreated, UserID = genre.UserID, Name = genre.Name, NameKey = genre.NameKey, TimeModified = genre.TimeModified };
    }
}
