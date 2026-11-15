using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Application.Models.Playlist;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakePlaylistRepository(FakeStore store) : IPlaylistRepository
{
    private readonly List<Playlist> _pendingPlaylistAddList = [];
    private readonly List<Playlist> _pendingPlaylistUpdateList = [];
    private readonly List<Guid> _pendingPlaylistRemovalList = [];
    private readonly List<PlaylistEntry> _pendingEntryAddList = [];
    private readonly List<PlaylistEntry> _pendingEntryUpdateList = [];
    private readonly List<Guid> _pendingEntryRemovalList = [];

    public void Flush()
    {
        store.PlaylistList.AddRange(_pendingPlaylistAddList);
        _pendingPlaylistAddList.Clear();

        foreach (PlaylistEntry added in _pendingEntryAddList)
        {
            Playlist? owner = store.PlaylistList.FirstOrDefault(x => x.ID == added.PlaylistID);
            owner?.EntryList.Add(Copy(added));
        }

        _pendingEntryAddList.Clear();

        foreach (Playlist updated in _pendingPlaylistUpdateList)
        {
            Playlist? existing = store.PlaylistList.FirstOrDefault(x => x.ID == updated.ID);
            if (existing is null) continue;

            existing.Name = updated.Name;
            existing.AllowDuplicates = updated.AllowDuplicates;
            existing.TimeModified = updated.TimeModified;
        }

        _pendingPlaylistUpdateList.Clear();

        foreach (PlaylistEntry updated in _pendingEntryUpdateList)
        {
            PlaylistEntry? existing = store.PlaylistList.SelectMany(x => x.EntryList).FirstOrDefault(x => x.ID == updated.ID);
            if (existing is null) continue;

            existing.Position = updated.Position;
        }

        _pendingEntryUpdateList.Clear();

        foreach (Playlist playlist in store.PlaylistList)
        {
            playlist.EntryList.RemoveAll(x => _pendingEntryRemovalList.Contains(x.ID));
        }

        _pendingEntryRemovalList.Clear();

        store.PlaylistList.RemoveAll(x => _pendingPlaylistRemovalList.Contains(x.ID));
        _pendingPlaylistRemovalList.Clear();
    }

    public void AddPlaylist(Playlist playlist)
    {
        _pendingPlaylistAddList.Add(Copy(playlist, true));
    }

    public Task<Playlist?> FindPlaylist(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        Playlist? found = store.PlaylistList.FirstOrDefault(x => x.ID == id && x.UserID == userID);

        return Task.FromResult(found is null ? null : Copy(found, false));
    }

    public Task<Playlist?> FindPlaylistWithEntries(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        Playlist? found = store.PlaylistList.FirstOrDefault(x => x.ID == id && x.UserID == userID);

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
        Playlist? found = store.PlaylistList.FirstOrDefault(x => x.ID == playlistID && x.UserID == userID);
        List<PlaylistStamp> list = found is null ? [] : [.. found.EntryList.OrderBy(x => x.Position).Select(x => new PlaylistStamp(x.MediaFileID, store.MediaFileList.Single(file => file.ID == x.MediaFileID).TimeModified))];

        return Task.FromResult(list);
    }

    public Task<int> CountPlaylists(Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.PlaylistList.Count(x => x.UserID == userID));
    }

    public Task<List<Playlist>> GetPlaylistListByMediaFileID(Guid userID, Guid mediaFileID, CancellationToken cancellationToken = default)
    {
        List<Playlist> list = store.PlaylistList
            .Where(x => x.UserID == userID && x.EntryList.Any(e => e.MediaFileID == mediaFileID))
            .OrderBy(x => x.TimeCreated)
            .ThenBy(x => x.ID)
            .Select(x => Copy(x, true))
            .ToList();

        return Task.FromResult(list);
    }

    public void UpdatePlaylist(Playlist playlist)
    {
        _pendingPlaylistUpdateList.Add(Copy(playlist, false));
    }

    public void RemovePlaylist(Playlist playlist)
    {
        _pendingPlaylistRemovalList.Add(playlist.ID);
    }

    public void AddPlaylistEntry(PlaylistEntry entry)
    {
        _pendingEntryAddList.Add(Copy(entry));
    }

    public void UpdatePlaylistEntry(PlaylistEntry entry)
    {
        _pendingEntryUpdateList.Add(Copy(entry));
    }

    public void RemovePlaylistEntry(PlaylistEntry entry)
    {
        _pendingEntryRemovalList.Add(entry.ID);
    }

    private Playlist WithMediaFileList(Playlist playlist)
    {
        foreach (PlaylistEntry entry in playlist.EntryList)
        {
            MediaFile file = store.MediaFileList.Single(x => x.ID == entry.MediaFileID);
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

    private static Playlist Copy(Playlist playlist, bool withEntries)
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

    private static PlaylistEntry Copy(PlaylistEntry entry)
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
