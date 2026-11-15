using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Extensions;
using Noxy.NET.MediaPlayer.Persistence.Interfaces.Mappers;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;
using Noxy.NET.MediaPlayer.Persistence.Tables.Metadata;

namespace Noxy.NET.MediaPlayer.Persistence.Repositories;

public class MetadataRepository(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : IMetadataRepository
{
    #region -- Artist --

    public void AddArtist(EntityArtist artist)
    {
        context.Artist.Add(mapperE2T.Map(artist));
    }

    public async Task<EntityArtist?> FindArtist(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        TableArtist? table = await context.Artist.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ID == id && x.UserID == userID, cancellationToken);

        return table is null ? null : mapperT2E.Map(table);
    }

    public async Task<EntityArtist?> FindArtistByKey(Guid userID, string key, CancellationToken cancellationToken = default)
    {
        // An artist added earlier in this unit of work counts too, so one batch never adds the same artist twice.
        TableArtist? table = context.Artist.Local.FirstOrDefault(x => x.UserID == userID && x.NameKey == key)
                             ?? await context.Artist.AsNoTracking().FirstOrDefaultAsync(x => x.UserID == userID && x.NameKey == key, cancellationToken);

        return table is null ? null : mapperT2E.Map(table);
    }

    public async Task<List<ArtistSummary>> GetArtistSummaryList(Guid userID, CancellationToken cancellationToken = default)
    {
        List<TableArtist> tableList = await context.Artist.AsNoTracking()
            .Where(x => x.UserID == userID)
            .OrderByDescending(x => x.TimeCreated)
            .ThenByDescending(x => x.ID)
            .ToListAsync(cancellationToken);

        Dictionary<Guid, int> countList = await context.MediaFileArtist.AsNoTracking()
            .Where(x => x.Artist!.UserID == userID)
            .GroupBy(x => x.ArtistID)
            .Select(x => new { ID = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.ID, x => x.Count, cancellationToken);

        return [.. tableList.Select(x => new ArtistSummary(mapperT2E.Map(x), countList.GetValueOrDefault(x.ID)))];
    }

    public async Task<List<MetadataStamp>> GetArtistStampList(Guid userID, CancellationToken cancellationToken = default)
    {
        return await context.Artist.AsNoTracking()
            .Where(x => x.UserID == userID)
            .Select(x => new MetadataStamp(x.ID, x.TimeModified, context.MediaFileArtist.Count(l => l.ArtistID == x.ID)))
            .ToListAsync(cancellationToken);
    }

    public void UpdateArtist(EntityArtist artist)
    {
        context.Artist.Update(mapperE2T.Map(artist));
    }

    public void RemoveArtist(EntityArtist artist)
    {
        // The database cascade removes its links.
        context.Artist.Remove(mapperE2T.Map(artist));
    }

    public async Task<List<EntityMediaFile>> GetMediaFileListByArtist(Guid userID, Guid artistID, CancellationToken cancellationToken = default)
    {
        List<TableMediaFile> list = await context.MediaFile.AsNoTracking()
            .IncludeLinks()
            .Where(x => x.UserID == userID && x.ArtistLinkList.Any(l => l.ArtistID == artistID))
            .OrderByDescending(x => x.TimeCreated)
            .ThenByDescending(x => x.ID)
            .ToListAsync(cancellationToken);

        return [.. list.Select(mapperT2E.Map)];
    }

    public void AddMediaFileArtist(EntityMediaFileArtist link)
    {
        context.MediaFileArtist.Add(mapperE2T.Map(link));
    }

    public void RemoveMediaFileArtist(EntityMediaFileArtist link)
    {
        context.MediaFileArtist.Remove(mapperE2T.Map(link));
    }

    #endregion -- Artist --

    #region -- Genre --

    public void AddGenre(EntityGenre genre)
    {
        context.Genre.Add(mapperE2T.Map(genre));
    }

    public async Task<EntityGenre?> FindGenre(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        TableGenre? table = await context.Genre.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ID == id && x.UserID == userID, cancellationToken);

        return table is null ? null : mapperT2E.Map(table);
    }

    public async Task<EntityGenre?> FindGenreByKey(Guid userID, string key, CancellationToken cancellationToken = default)
    {
        TableGenre? table = context.Genre.Local.FirstOrDefault(x => x.UserID == userID && x.NameKey == key)
                            ?? await context.Genre.AsNoTracking().FirstOrDefaultAsync(x => x.UserID == userID && x.NameKey == key, cancellationToken);

        return table is null ? null : mapperT2E.Map(table);
    }

    public async Task<List<GenreSummary>> GetGenreSummaryList(Guid userID, CancellationToken cancellationToken = default)
    {
        List<TableGenre> tableList = await context.Genre.AsNoTracking()
            .Where(x => x.UserID == userID)
            .OrderByDescending(x => x.TimeCreated)
            .ThenByDescending(x => x.ID)
            .ToListAsync(cancellationToken);

        Dictionary<Guid, int> countList = await context.MediaFileGenre.AsNoTracking()
            .Where(x => x.Genre!.UserID == userID)
            .GroupBy(x => x.GenreID)
            .Select(x => new { ID = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.ID, x => x.Count, cancellationToken);

        return [.. tableList.Select(x => new GenreSummary(mapperT2E.Map(x), countList.GetValueOrDefault(x.ID)))];
    }

    public async Task<List<MetadataStamp>> GetGenreStampList(Guid userID, CancellationToken cancellationToken = default)
    {
        return await context.Genre.AsNoTracking()
            .Where(x => x.UserID == userID)
            .Select(x => new MetadataStamp(x.ID, x.TimeModified, context.MediaFileGenre.Count(l => l.GenreID == x.ID)))
            .ToListAsync(cancellationToken);
    }

    public void UpdateGenre(EntityGenre genre)
    {
        context.Genre.Update(mapperE2T.Map(genre));
    }

    public void RemoveGenre(EntityGenre genre)
    {
        // The database cascade removes its links.
        context.Genre.Remove(mapperE2T.Map(genre));
    }

    public async Task<List<EntityMediaFile>> GetMediaFileListByGenre(Guid userID, Guid genreID, CancellationToken cancellationToken = default)
    {
        List<TableMediaFile> list = await context.MediaFile.AsNoTracking()
            .IncludeLinks()
            .Where(x => x.UserID == userID && x.GenreLinkList.Any(l => l.GenreID == genreID))
            .OrderByDescending(x => x.TimeCreated)
            .ThenByDescending(x => x.ID)
            .ToListAsync(cancellationToken);

        return [.. list.Select(mapperT2E.Map)];
    }

    public void AddMediaFileGenre(EntityMediaFileGenre link)
    {
        context.MediaFileGenre.Add(mapperE2T.Map(link));
    }

    public void RemoveMediaFileGenre(EntityMediaFileGenre link)
    {
        context.MediaFileGenre.Remove(mapperE2T.Map(link));
    }

    #endregion -- Genre --

    #region -- Album --

    public void AddAlbum(EntityAlbum album)
    {
        context.Album.Add(mapperE2T.Map(album));
    }

    public async Task<EntityAlbum?> FindAlbum(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        TableAlbum? table = await context.Album.AsNoTracking()
            .Include(x => x.ArtistLinkList.OrderBy(l => l.Position)).ThenInclude(l => l.Artist)
            .FirstOrDefaultAsync(x => x.ID == id && x.UserID == userID, cancellationToken);

        return table is null ? null : mapperT2E.Map(table);
    }

    public async Task<EntityAlbum?> FindAlbumByIdentityKey(Guid userID, string identityKey, CancellationToken cancellationToken = default)
    {
        TableAlbum? table = context.Album.Local.FirstOrDefault(x => x.UserID == userID && x.IdentityKey == identityKey)
                            ?? await context.Album.AsNoTracking()
                                .Include(x => x.ArtistLinkList.OrderBy(l => l.Position)).ThenInclude(l => l.Artist)
                                .FirstOrDefaultAsync(x => x.UserID == userID && x.IdentityKey == identityKey, cancellationToken);

        return table is null ? null : mapperT2E.Map(table);
    }

    public async Task<List<AlbumSummary>> GetAlbumSummaryList(Guid userID, CancellationToken cancellationToken = default)
    {
        List<TableAlbum> tableList = await context.Album.AsNoTracking()
            .Include(x => x.ArtistLinkList.OrderBy(l => l.Position)).ThenInclude(l => l.Artist)
            .Where(x => x.UserID == userID)
            .OrderByDescending(x => x.TimeCreated)
            .ThenByDescending(x => x.ID)
            .ToListAsync(cancellationToken);

        Dictionary<Guid, int> countList = await context.MediaFile.AsNoTracking()
            .Where(x => x.UserID == userID && x.AlbumID != null)
            .GroupBy(x => x.AlbumID!.Value)
            .Select(x => new { ID = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.ID, x => x.Count, cancellationToken);

        return [.. tableList.Select(x => new AlbumSummary(mapperT2E.Map(x), countList.GetValueOrDefault(x.ID)))];
    }

    public async Task<List<MetadataStamp>> GetAlbumStampList(Guid userID, CancellationToken cancellationToken = default)
    {
        return await context.Album.AsNoTracking()
            .Where(x => x.UserID == userID)
            .Select(x => new MetadataStamp(x.ID, x.TimeModified, context.MediaFile.Count(f => f.AlbumID == x.ID)))
            .ToListAsync(cancellationToken);
    }

    public void UpdateAlbum(EntityAlbum album)
    {
        TableAlbum table = mapperE2T.Map(album);

        // Only the album row is saved: attaching the links as well would rewrite every one of them.
        table.ArtistLinkList = [];
        context.Album.Update(table);
    }

    public void RemoveAlbum(EntityAlbum album)
    {
        TableAlbum table = mapperE2T.Map(album);

        // The database cascade removes the artist links, and the files lose the album.
        table.ArtistLinkList = [];
        context.Album.Remove(table);
    }

    public async Task<List<EntityMediaFile>> GetMediaFileListByAlbum(Guid userID, Guid albumID, CancellationToken cancellationToken = default)
    {
        List<TableMediaFile> list = await context.MediaFile.AsNoTracking()
            .IncludeLinks()
            .Where(x => x.UserID == userID && x.AlbumID == albumID)
            .OrderByDescending(x => x.TimeCreated)
            .ThenByDescending(x => x.ID)
            .ToListAsync(cancellationToken);

        return [.. list.Select(mapperT2E.Map)];
    }

    public async Task<List<EntityAlbum>> GetAlbumListByArtist(Guid userID, Guid artistID, CancellationToken cancellationToken = default)
    {
        List<TableAlbum> list = await context.Album.AsNoTracking()
            .Include(x => x.ArtistLinkList.OrderBy(l => l.Position)).ThenInclude(l => l.Artist)
            .Where(x => x.UserID == userID && x.ArtistLinkList.Any(l => l.ArtistID == artistID))
            .OrderBy(x => x.TimeCreated)
            .ThenBy(x => x.ID)
            .ToListAsync(cancellationToken);

        return [.. list.Select(mapperT2E.Map)];
    }

    public void AddAlbumArtist(EntityAlbumArtist link)
    {
        context.AlbumArtist.Add(mapperE2T.Map(link));
    }

    public void RemoveAlbumArtist(EntityAlbumArtist link)
    {
        context.AlbumArtist.Remove(mapperE2T.Map(link));
    }

    #endregion -- Album --
}
