using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Interfaces.Mappers;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;

namespace Noxy.NET.MediaPlayer.Persistence.Repositories;

public class PlaylistRepository(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : IPlaylistRepository
{
    public void AddPlaylist(Playlist playlist)
    {
        context.Playlist.Add(mapperE2T.Map(playlist));
    }

    public async Task<Playlist?> FindPlaylist(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        TablePlaylist? table = await context.Playlist.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ID == id && x.UserID == userID, cancellationToken);

        return table is null ? null : mapperT2E.Map(table);
    }

    public async Task<Playlist?> FindPlaylistWithEntries(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        TablePlaylist? table = await context.Playlist.AsNoTracking()
            .Include(x => x.EntryList.OrderBy(e => e.Position))
            .FirstOrDefaultAsync(x => x.ID == id && x.UserID == userID, cancellationToken);

        return table is null ? null : mapperT2E.Map(table);
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
            .Select(x => new PlaylistStamp(x.ID, x.TimeModified))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountPlaylists(Guid userID, CancellationToken cancellationToken = default)
    {
        return await context.Playlist.CountAsync(x => x.UserID == userID, cancellationToken);
    }

    public async Task<List<Playlist>> GetPlaylistListByMediaFileID(Guid userID, Guid mediaFileID, CancellationToken cancellationToken = default)
    {
        List<TablePlaylist> list = await context.Playlist.AsNoTracking()
            .Include(x => x.EntryList.OrderBy(e => e.Position))
            .Where(x => x.UserID == userID && x.EntryList.Any(e => e.MediaFileID == mediaFileID))
            .OrderBy(x => x.TimeCreated)
            .ThenBy(x => x.ID)
            .ToListAsync(cancellationToken);

        return [.. list.Select(mapperT2E.Map)];
    }

    public void UpdatePlaylist(Playlist playlist)
    {
        TablePlaylist table = mapperE2T.Map(playlist);

        // Only the playlist row is saved: attaching the entries as well would rewrite every one of them.
        table.EntryList = [];
        context.Playlist.Update(table);
    }

    public void RemovePlaylist(Playlist playlist)
    {
        TablePlaylist table = mapperE2T.Map(playlist);

        // The database cascade removes the entries.
        table.EntryList = [];
        context.Playlist.Remove(table);
    }

    public void AddPlaylistEntry(PlaylistEntry entry)
    {
        context.PlaylistEntry.Add(mapperE2T.Map(entry));
    }

    public void UpdatePlaylistEntry(PlaylistEntry entry)
    {
        context.PlaylistEntry.Update(mapperE2T.Map(entry));
    }

    public void RemovePlaylistEntry(PlaylistEntry entry)
    {
        context.PlaylistEntry.Remove(mapperE2T.Map(entry));
    }
}
