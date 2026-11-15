using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Notifiers;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Requests.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class PlaylistService(
    IUnitOfWorkFactory factoryUnitOfWork,
    IOptions<ConfigPlaylist> configPlaylist,
    IPlaylistNotifier notifierPlaylist,
    IETagService serviceETag,
    TimeProvider time,
    ILogger<PlaylistService> logger) : IPlaylistService
{
    public async Task<PlaylistLibrary> GetPlaylistLibrary(Guid userID, string? ifNoneMatch, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        if (!string.IsNullOrWhiteSpace(ifNoneMatch))
        {
            List<PlaylistStamp> stampList = await uow.Playlist.GetPlaylistStampList(userID, cancellationToken);
            string current = serviceETag.Compute(stampList.Select(x => (x.ID, x.TimeModified)));
            if (serviceETag.Matches(ifNoneMatch, current)) return new(current, null);
        }

        // The ETag comes from the list that is returned, so the two can never disagree.
        List<PlaylistSummary> summaryList = await uow.Playlist.GetPlaylistSummaryList(userID, cancellationToken);
        return new(serviceETag.Compute(summaryList.Select(x => (x.Playlist.ID, x.Playlist.TimeModified))), [.. summaryList.Select(x => x.Playlist.ToResponse(x.EntryCount))]);
    }

    public async Task<PlaylistView> GetPlaylist(Guid userID, Guid id, string? ifNoneMatch, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        if (!string.IsNullOrWhiteSpace(ifNoneMatch))
        {
            Playlist header = await uow.Playlist.FindPlaylist(id, userID, cancellationToken) ?? throw new PlaylistNotFoundException();
            List<PlaylistStamp> mediaFileStampList = await uow.Playlist.GetEntryMediaFileStampList(id, userID, cancellationToken);
            string current = DetailETag(header.ID, header.TimeModified, mediaFileStampList.Select(x => (x.ID, x.TimeModified)));
            if (serviceETag.Matches(ifNoneMatch, current)) return new(current, null);
        }

        // The ETag comes from the entries that are returned, so the two can never disagree.
        Playlist playlist = await Load(uow, userID, id, cancellationToken);
        string eTag = DetailETag(playlist.ID, playlist.TimeModified, playlist.EntryList.Select(x => (x.MediaFileID, x.GetLoadedMediaFile().TimeModified)));
        return new(eTag, playlist.ToDetailResponse());
    }

    public async Task<ResponsePlaylist> Create(Guid userID, RequestCreatePlaylist request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name, nameof(request));

        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        if (await uow.Playlist.CountPlaylists(userID, cancellationToken) >= configPlaylist.Value.MaxPlaylistsPerUser) throw new PlaylistLimitReachedException();

        DateTime now = time.GetUtcNow().UtcDateTime;
        Playlist playlist = new()
        {
            UserID = userID,
            Name = request.Name.Trim(),
            AllowDuplicates = request.AllowDuplicates,
            TimeCreated = now,
            TimeModified = now
        };

        uow.Playlist.AddPlaylist(playlist);
        await uow.Commit(cancellationToken);

        ResponsePlaylist response = playlist.ToResponse(0);
        await NotifyQuietly(() => notifierPlaylist.Updated(userID, response, cancellationToken), playlist.ID);
        return response;
    }

    public async Task<ResponsePlaylist> Update(Guid userID, Guid id, RequestUpdatePlaylist request, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        Playlist playlist = await Load(uow, userID, id, cancellationToken);

        bool changed = false;
        if (request.Name.HasValue)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Name.Value, nameof(request));

            string name = request.Name.Value.Trim();
            if (name != playlist.Name)
            {
                playlist.Name = name;
                changed = true;
            }
        }

        if (request.AllowDuplicates.HasValue && request.AllowDuplicates.Value != playlist.AllowDuplicates)
        {
            // Duplicates can be turned off only once there are none, so a playlist never breaks its own rule.
            if (!request.AllowDuplicates.Value && playlist.HasDuplicates()) throw new PlaylistHasDuplicatesException();

            playlist.AllowDuplicates = request.AllowDuplicates.Value;
            changed = true;
        }

        // An edit that changes nothing leaves the modified time alone, so it can't change the ETag or push anything.
        if (!changed) return playlist.ToResponse(playlist.EntryList.Count);

        await Save(uow, userID, playlist, cancellationToken);
        return playlist.ToResponse(playlist.EntryList.Count);
    }

    public async Task Delete(Guid userID, Guid id, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        Playlist playlist = await uow.Playlist.FindPlaylist(id, userID, cancellationToken) ?? throw new PlaylistNotFoundException();

        uow.Playlist.RemovePlaylist(playlist);
        await uow.Commit(cancellationToken);

        await NotifyQuietly(() => notifierPlaylist.Deleted(userID, id, cancellationToken), id);
    }

    public async Task<ResponsePlaylistDetail> AddEntryList(Guid userID, Guid id, RequestAddPlaylistEntry request, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        Playlist playlist = await Load(uow, userID, id, cancellationToken);
        List<PlaylistEntry> entryList = playlist.EntryList;

        int position = request.Position ?? entryList.Count;
        if (position < 0 || position > entryList.Count) throw new InvalidPlaylistPositionException();

        // Every media file must be the user's before anything changes.
        Dictionary<Guid, MediaFile> mediaFileMap = [];
        foreach (Guid mediaFileID in request.MediaFileIDList.Distinct())
        {
            mediaFileMap[mediaFileID] = await uow.Media.FindMediaFile(mediaFileID, userID, cancellationToken) ?? throw new MediaNotFoundException();
        }

        HashSet<Guid> present = [.. entryList.Select(x => x.MediaFileID)];
        List<PlaylistEntry> addedList = [];
        foreach (Guid mediaFileID in request.MediaFileIDList)
        {
            // With duplicates off a media file already there, or repeated in this request, is skipped.
            if (!playlist.AllowDuplicates && !present.Add(mediaFileID)) continue;

            addedList.Add(new() { PlaylistID = playlist.ID, MediaFileID = mediaFileID, Position = 0, MediaFile = mediaFileMap[mediaFileID] });
        }

        if (addedList.Count == 0) return playlist.ToDetailResponse();

        if (entryList.Count + addedList.Count > configPlaylist.Value.MaxEntriesPerPlaylist) throw new PlaylistEntryLimitReachedException();

        entryList.InsertRange(position, addedList);
        List<PlaylistEntry> changedList = playlist.RenumberEntryList();

        HashSet<Guid> addedIDSet = [.. addedList.Select(x => x.ID)];
        foreach (PlaylistEntry entry in addedList)
        {
            uow.Playlist.AddPlaylistEntry(entry);
        }

        foreach (PlaylistEntry entry in changedList.Where(x => !addedIDSet.Contains(x.ID)))
        {
            uow.Playlist.UpdatePlaylistEntry(entry);
        }

        return await Save(uow, userID, playlist, cancellationToken);
    }

    public async Task<ResponsePlaylistDetail> RemoveEntryList(Guid userID, Guid id, RequestRemovePlaylistEntry request, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        Playlist playlist = await Load(uow, userID, id, cancellationToken);

        HashSet<Guid> removeIDSet = [.. request.EntryIDList];
        List<PlaylistEntry> removedList = [.. playlist.EntryList.Where(x => removeIDSet.Contains(x.ID))];
        if (removedList.Count != removeIDSet.Count) throw new PlaylistEntryNotFoundException();

        playlist.EntryList.RemoveAll(x => removeIDSet.Contains(x.ID));
        List<PlaylistEntry> changedList = playlist.RenumberEntryList();

        foreach (PlaylistEntry entry in removedList)
        {
            uow.Playlist.RemovePlaylistEntry(entry);
        }

        foreach (PlaylistEntry entry in changedList)
        {
            uow.Playlist.UpdatePlaylistEntry(entry);
        }

        return await Save(uow, userID, playlist, cancellationToken);
    }

    public async Task<ResponsePlaylistDetail> MoveEntry(Guid userID, Guid id, Guid entryID, RequestMovePlaylistEntry request, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        Playlist playlist = await Load(uow, userID, id, cancellationToken);

        PlaylistEntry entry = playlist.EntryList.FirstOrDefault(x => x.ID == entryID) ?? throw new PlaylistEntryNotFoundException();
        if (request.Position < 0 || request.Position >= playlist.EntryList.Count) throw new InvalidPlaylistPositionException();

        // A move to where the entry already is changes nothing, so it leaves the modified time and the ETag alone.
        if (playlist.EntryList.IndexOf(entry) == request.Position) return playlist.ToDetailResponse();

        playlist.EntryList.Remove(entry);
        playlist.EntryList.Insert(request.Position, entry);

        foreach (PlaylistEntry changed in playlist.RenumberEntryList())
        {
            uow.Playlist.UpdatePlaylistEntry(changed);
        }

        return await Save(uow, userID, playlist, cancellationToken);
    }

    // The ETag of a playlist with its entries moves with the playlist and with every media file in it, so a tag edit is never answered with a 304.
    private string DetailETag(Guid playlistID, DateTime playlistModified, IEnumerable<(Guid ID, DateTime TimeModified)> mediaFileStampList)
    {
        return serviceETag.Compute([(playlistID, playlistModified), .. mediaFileStampList]);
    }

    // The entries are in position order, which is the order every change below works on.
    private static async Task<Playlist> Load(IUnitOfWork uow, Guid userID, Guid id, CancellationToken cancellationToken)
    {
        Playlist playlist = await uow.Playlist.FindPlaylistWithEntries(id, userID, cancellationToken) ?? throw new PlaylistNotFoundException();
        playlist.EntryList = [.. playlist.EntryList.OrderBy(x => x.Position)];
        return playlist;
    }

    // Every change bumps the modified time in the same commit, which is what keeps the ETags correct.
    private async Task<ResponsePlaylistDetail> Save(IUnitOfWork uow, Guid userID, Playlist playlist, CancellationToken cancellationToken)
    {
        playlist.TimeModified = time.GetUtcNow().UtcDateTime;
        uow.Playlist.UpdatePlaylist(playlist);
        await uow.Commit(cancellationToken);

        ResponsePlaylist pushed = playlist.ToResponse(playlist.EntryList.Count);
        await NotifyQuietly(() => notifierPlaylist.Updated(userID, pushed, cancellationToken), playlist.ID);
        return playlist.ToDetailResponse();
    }

    // A failed notification is only logged: the change itself has already happened.
    private async Task NotifyQuietly(Func<Task> notify, Guid id)
    {
        try
        {
            await notify();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not notify about playlist {PlaylistID}", id);
        }
    }
}
