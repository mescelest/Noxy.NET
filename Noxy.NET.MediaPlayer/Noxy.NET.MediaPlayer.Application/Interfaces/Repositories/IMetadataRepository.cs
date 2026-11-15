using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

public interface IMetadataRepository
{
    #region -- Artist --

    void AddArtist(EntityArtist artist);

    // Scoped to the owner: another user's artist comes back as null.
    Task<EntityArtist?> FindArtist(Guid id, Guid userID, CancellationToken cancellationToken = default);

    // Includes an artist added earlier in the same unit of work and not yet committed.
    Task<EntityArtist?> FindArtistByKey(Guid userID, string key, CancellationToken cancellationToken = default);

    // Newest first.
    Task<List<ArtistSummary>> GetArtistSummaryList(Guid userID, CancellationToken cancellationToken = default);

    // The ID, modified time and file count of each item: all the list ETag needs.
    Task<List<MetadataStamp>> GetArtistStampList(Guid userID, CancellationToken cancellationToken = default);

    void UpdateArtist(EntityArtist artist);

    // Its links to media files and albums go with it.
    void RemoveArtist(EntityArtist artist);

    // The owner's files that have the artist, each with its album, artists and genres loaded in position order.
    Task<List<EntityMediaFile>> GetMediaFileListByArtist(Guid userID, Guid artistID, CancellationToken cancellationToken = default);

    void AddMediaFileArtist(EntityMediaFileArtist link);
    void RemoveMediaFileArtist(EntityMediaFileArtist link);

    #endregion -- Artist --

    #region -- Genre --

    void AddGenre(EntityGenre genre);

    Task<EntityGenre?> FindGenre(Guid id, Guid userID, CancellationToken cancellationToken = default);

    // Includes a genre added earlier in the same unit of work and not yet committed.
    Task<EntityGenre?> FindGenreByKey(Guid userID, string key, CancellationToken cancellationToken = default);
    Task<List<GenreSummary>> GetGenreSummaryList(Guid userID, CancellationToken cancellationToken = default);
    Task<List<MetadataStamp>> GetGenreStampList(Guid userID, CancellationToken cancellationToken = default);
    void UpdateGenre(EntityGenre genre);

    // Its links to media files go with it.
    void RemoveGenre(EntityGenre genre);

    Task<List<EntityMediaFile>> GetMediaFileListByGenre(Guid userID, Guid genreID, CancellationToken cancellationToken = default);
    void AddMediaFileGenre(EntityMediaFileGenre link);
    void RemoveMediaFileGenre(EntityMediaFileGenre link);

    #endregion -- Genre --

    #region -- Album --

    // Adds the album together with any links in its ArtistLinkList.
    void AddAlbum(EntityAlbum album);

    // The artist links are loaded, in position order, each with its artist.
    Task<EntityAlbum?> FindAlbum(Guid id, Guid userID, CancellationToken cancellationToken = default);

    // Includes an album added earlier in the same unit of work and not yet committed.
    Task<EntityAlbum?> FindAlbumByIdentityKey(Guid userID, string identityKey, CancellationToken cancellationToken = default);

    // Newest first, each album with its artist links.
    Task<List<AlbumSummary>> GetAlbumSummaryList(Guid userID, CancellationToken cancellationToken = default);

    Task<List<MetadataStamp>> GetAlbumStampList(Guid userID, CancellationToken cancellationToken = default);

    // Saves the album row only, never its artist links.
    void UpdateAlbum(EntityAlbum album);

    // The files keep existing and lose the album. Its artist links go with it.
    void RemoveAlbum(EntityAlbum album);

    Task<List<EntityMediaFile>> GetMediaFileListByAlbum(Guid userID, Guid albumID, CancellationToken cancellationToken = default);

    // The owner's albums that list the artist, each with its artist links.
    Task<List<EntityAlbum>> GetAlbumListByArtist(Guid userID, Guid artistID, CancellationToken cancellationToken = default);

    void AddAlbumArtist(EntityAlbumArtist link);
    void RemoveAlbumArtist(EntityAlbumArtist link);

    #endregion -- Album --
}
