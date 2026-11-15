using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Application.Models.Playlist;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Interfaces.Mappers;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;

namespace Noxy.NET.MediaPlayer.Persistence.Repositories;

public class PlaylistRepository(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : IPlaylistRepository
{
    public void AddPlaylist(EntityPlaylist playlist)
    {
        context.Playlist.Add(mapperE2T.Map(playlist));
    }

    public async Task<EntityPlaylist?> FindPlaylist(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        TablePlaylist? table = await context.Playlist.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ID == id && x.UserID == userID, cancellationToken);

        return table is null ? null : mapperT2E.Map(table);
    }

    public async Task<EntityPlaylist?> FindPlaylistWithEntries(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        TablePlaylist? table = await context.Playlist.AsNoTracking()
            .Include(x => x.EntryList.OrderBy(e => e.Position)).ThenInclude(e => e.MediaFile!.Metadata)
            .Include(x => x.EntryList.OrderBy(e => e.Position)).ThenInclude(e => e.MediaFile!).ThenInclude(f => f.ArtistLinkList.OrderBy(l => l.Position)).ThenInclude(l => l.Artist)
            .Include(x => x.EntryList.OrderBy(e => e.Position)).ThenInclude(e => e.MediaFile!).ThenInclude(f => f.GenreLinkList.OrderBy(l => l.Position)).ThenInclude(l => l.Genre)
            .Include(x => x.EntryList.OrderBy(e => e.Position)).ThenInclude(e => e.MediaFile!).ThenInclude(f => f.Album).ThenInclude(a => a!.ArtistLinkList.OrderBy(l => l.Position)).ThenInclude(l => l.Artist)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.ID == id && x.UserID == userID, cancellationToken);

        return table is null ? null : mapperT2E.Map(table);
    }

    public async Task<PlaylistSummary?> FindPlaylistSummary(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        var found = await context.Playlist.AsNoTracking()
            .Where(x => x.ID == id && x.UserID == userID)
            .Select(x => new { Table = x, EntryCount = x.EntryList.Count })
            .FirstOrDefaultAsync(cancellationToken);

        return found is null ? null : new PlaylistSummary(mapperT2E.Map(found.Table), found.EntryCount);
    }

    public async Task<List<PlaylistSummary>> GetPlaylistSummaryList(Guid userID, CancellationToken cancellationToken = default)
    {
        var list = await context.Playlist.AsNoTracking()
            .Where(x => x.UserID == userID)
            .OrderByDescending(x => x.TimeCreated)
            .ThenByDescending(x => x.ID)
            .Select(x => new { Table = x, EntryCount = x.EntryList.Count })
            .ToListAsync(cancellationToken);

        return [.. list.Select(x => new PlaylistSummary(mapperT2E.Map(x.Table), x.EntryCount))];
    }

    public async Task<List<PlaylistStamp>> GetPlaylistStampList(Guid userID, CancellationToken cancellationToken = default)
    {
        return await context.Playlist.AsNoTracking()
            .Where(x => x.UserID == userID)
            .Select(x => new PlaylistStamp(x.ID, x.TimeModified, x.TimeLastPlayed))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PlaylistStamp>> GetEntryMediaFileStampList(Guid playlistID, Guid userID, CancellationToken cancellationToken = default)
    {
        return await context.PlaylistEntry.AsNoTracking()
            .Where(x => x.PlaylistID == playlistID && x.Playlist!.UserID == userID)
            .Select(x => new PlaylistStamp(x.MediaFileID, x.MediaFile!.TimeModified))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountPlaylists(Guid userID, CancellationToken cancellationToken = default)
    {
        return await context.Playlist.CountAsync(x => x.UserID == userID, cancellationToken);
    }

    public async Task<List<EntityPlaylist>> GetPlaylistListByMediaFileID(Guid userID, Guid mediaFileID, CancellationToken cancellationToken = default)
    {
        List<TablePlaylist> list = await context.Playlist.AsNoTracking()
            .Include(x => x.EntryList.OrderBy(e => e.Position))
            .Where(x => x.UserID == userID && x.EntryList.Any(e => e.MediaFileID == mediaFileID))
            .OrderBy(x => x.TimeCreated)
            .ThenBy(x => x.ID)
            .ToListAsync(cancellationToken);

        return [.. list.Select(mapperT2E.Map)];
    }

    public void UpdatePlaylist(EntityPlaylist playlist)
    {
        TablePlaylist table = mapperE2T.Map(playlist);

        // Only the playlist row is saved: attaching the entries as well would rewrite every one of them.
        table.EntryList = [];
        context.Playlist.Update(table);
    }

    public void RemovePlaylist(EntityPlaylist playlist)
    {
        TablePlaylist table = mapperE2T.Map(playlist);

        // The database cascade removes the entries.
        table.EntryList = [];
        context.Playlist.Remove(table);
    }

    public void AddPlaylistEntry(EntityPlaylistEntry entry)
    {
        context.PlaylistEntry.Add(mapperE2T.Map(entry));
    }

    public void UpdatePlaylistEntry(EntityPlaylistEntry entry)
    {
        context.PlaylistEntry.Update(mapperE2T.Map(entry));
    }

    public void RemovePlaylistEntry(EntityPlaylistEntry entry)
    {
        context.PlaylistEntry.Remove(mapperE2T.Map(entry));
    }
}
