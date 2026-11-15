using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Application.Models.Playlist;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakePlaylistRepository(FakeStore store) : IPlaylistRepository
{
    private readonly List<EntityPlaylist> _pendingPlaylistAddList = [];
    private readonly List<EntityPlaylist> _pendingPlaylistUpdateList = [];
    private readonly List<Guid> _pendingPlaylistRemovalList = [];
    private readonly List<EntityPlaylistEntry> _pendingEntryAddList = [];
    private readonly List<EntityPlaylistEntry> _pendingEntryUpdateList = [];
    private readonly List<Guid> _pendingEntryRemovalList = [];

    public void Flush()
    {
        store.PlaylistList.AddRange(_pendingPlaylistAddList);
        _pendingPlaylistAddList.Clear();

        foreach (EntityPlaylistEntry added in _pendingEntryAddList)
        {
            EntityPlaylist? owner = store.PlaylistList.FirstOrDefault(x => x.ID == added.PlaylistID);
            owner?.EntryList.Add(Copy(added));
        }

        _pendingEntryAddList.Clear();

        foreach (EntityPlaylist updated in _pendingPlaylistUpdateList)
        {
            EntityPlaylist? existing = store.PlaylistList.FirstOrDefault(x => x.ID == updated.ID);
            if (existing is null) continue;

            existing.Name = updated.Name;
            existing.AllowDuplicates = updated.AllowDuplicates;
            existing.TimeModified = updated.TimeModified;
        }

        _pendingPlaylistUpdateList.Clear();

        foreach (EntityPlaylistEntry updated in _pendingEntryUpdateList)
        {
            EntityPlaylistEntry? existing = store.PlaylistList.SelectMany(x => x.EntryList).FirstOrDefault(x => x.ID == updated.ID);
            if (existing is null) continue;

            existing.Position = updated.Position;
        }

        _pendingEntryUpdateList.Clear();

        foreach (EntityPlaylist playlist in store.PlaylistList)
        {
            playlist.EntryList.RemoveAll(x => _pendingEntryRemovalList.Contains(x.ID));
        }

        _pendingEntryRemovalList.Clear();

        store.PlaylistList.RemoveAll(x => _pendingPlaylistRemovalList.Contains(x.ID));
        _pendingPlaylistRemovalList.Clear();
    }

    public void AddPlaylist(EntityPlaylist playlist)
    {
        _pendingPlaylistAddList.Add(Copy(playlist, true));
    }

    public Task<EntityPlaylist?> FindPlaylist(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        EntityPlaylist? found = store.PlaylistList.FirstOrDefault(x => x.ID == id && x.UserID == userID);

        return Task.FromResult(found is null ? null : Copy(found, false));
    }

    public Task<EntityPlaylist?> FindPlaylistWithEntries(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        EntityPlaylist? found = store.PlaylistList.FirstOrDefault(x => x.ID == id && x.UserID == userID);

        return Task.FromResult(found is null ? null : WithMediaFileList(Copy(found, true)));
    }

    public Task<List<PlaylistSummary>> GetPlaylistSummaryList(Guid userID, CancellationToken cancellationToken = default)
    {
        List<PlaylistSummary> list = store.PlaylistList
            .Where(x => x.UserID == userID)
            .OrderByDescending(x => x.TimeCreated)
            .ThenByDescending(x => x.ID)
            .Select(x => new PlaylistSummary(Copy(x, false), x.EntryList.Count))
            .ToList();

        return Task.FromResult(list);
    }

    public Task<List<PlaylistStamp>> GetPlaylistStampList(Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.PlaylistList.Where(x => x.UserID == userID).Select(x => new PlaylistStamp(x.ID, x.TimeModified)).ToList());
    }

    public Task<List<PlaylistStamp>> GetEntryMediaFileStampList(Guid playlistID, Guid userID, CancellationToken cancellationToken = default)
    {
        EntityPlaylist? found = store.PlaylistList.FirstOrDefault(x => x.ID == playlistID && x.UserID == userID);
        List<PlaylistStamp> list = found is null ? [] : [.. found.EntryList.OrderBy(x => x.Position).Select(x => new PlaylistStamp(x.MediaFileID, store.MediaFileList.Single(file => file.ID == x.MediaFileID).TimeModified))];

        return Task.FromResult(list);
    }

    public Task<int> CountPlaylists(Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.PlaylistList.Count(x => x.UserID == userID));
    }

    public Task<List<EntityPlaylist>> GetPlaylistListByMediaFileID(Guid userID, Guid mediaFileID, CancellationToken cancellationToken = default)
    {
        List<EntityPlaylist> list = store.PlaylistList
            .Where(x => x.UserID == userID && x.EntryList.Any(e => e.MediaFileID == mediaFileID))
            .OrderBy(x => x.TimeCreated)
            .ThenBy(x => x.ID)
            .Select(x => Copy(x, true))
            .ToList();

        return Task.FromResult(list);
    }

    public void UpdatePlaylist(EntityPlaylist playlist)
    {
        _pendingPlaylistUpdateList.Add(Copy(playlist, false));
    }

    public void RemovePlaylist(EntityPlaylist playlist)
    {
        _pendingPlaylistRemovalList.Add(playlist.ID);
    }

    public void AddPlaylistEntry(EntityPlaylistEntry entry)
    {
        _pendingEntryAddList.Add(Copy(entry));
    }

    public void UpdatePlaylistEntry(EntityPlaylistEntry entry)
    {
        _pendingEntryUpdateList.Add(Copy(entry));
    }

    public void RemovePlaylistEntry(EntityPlaylistEntry entry)
    {
        _pendingEntryRemovalList.Add(entry.ID);
    }

    private EntityPlaylist WithMediaFileList(EntityPlaylist playlist)
    {
        foreach (EntityPlaylistEntry entry in playlist.EntryList)
        {
            EntityMediaFile file = store.MediaFileList.Single(x => x.ID == entry.MediaFileID);
            entry.MediaFile = new()
            {
                ID = file.ID,
                TimeCreated = file.TimeCreated,
                FilePath = file.FilePath,
                ContentType = file.ContentType,
                SizeBytes = file.SizeBytes,
                ContentHash = file.ContentHash,
                TimeModified = file.TimeModified,
                UserID = file.UserID,
                Metadata = file.Metadata
            };
        }

        return playlist;
    }

    private static EntityPlaylist Copy(EntityPlaylist playlist, bool withEntries)
    {
        return new()
        {
            ID = playlist.ID,
            TimeCreated = playlist.TimeCreated,
            UserID = playlist.UserID,
            Name = playlist.Name,
            AllowDuplicates = playlist.AllowDuplicates,
            TimeModified = playlist.TimeModified,
            EntryList = withEntries ? [.. playlist.EntryList.OrderBy(x => x.Position).Select(Copy)] : []
        };
    }

    private static EntityPlaylistEntry Copy(EntityPlaylistEntry entry)
    {
        return new()
        {
            ID = entry.ID,
            TimeCreated = entry.TimeCreated,
            PlaylistID = entry.PlaylistID,
            MediaFileID = entry.MediaFileID,
            Position = entry.Position
        };
    }
}
