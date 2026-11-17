using System.ComponentModel;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class QueueSourceService(IMetadataStore storeMetadata, ISettingService serviceSetting, IShuffleService serviceShuffle, Random random) : IQueueSourceService
{
    private sealed record BuiltSource(List<QueueItem> ItemList, Dictionary<Guid, string?> ArtistMap);

    public async Task<List<QueueItem>?> BuildBase(ResponseQueueSource source)
    {
        return (await Load(source))?.ItemList;
    }

    public async Task<List<QueueItem>?> Build(ResponseQueueSource source, ShuffleModeEnum shuffle, Guid? startID)
    {
        BuiltSource? built = await Load(source);
        if (built is null) return null;

        if (startID is null) return serviceShuffle.Order(built.ItemList, shuffle, ArtistOf(built));

        int index = built.ItemList.FindIndex(x => Key(source, x) == startID);
        if (index < 0) return null;

        if (shuffle == ShuffleModeEnum.Off) return built.ItemList.GetRange(index, built.ItemList.Count - index);

        List<QueueItem> rest = [.. built.ItemList];
        rest.RemoveAt(index);

        return [built.ItemList[index], .. serviceShuffle.Order(rest, shuffle, ArtistOf(built))];
    }

    public async Task<List<QueueItem>?> Regenerate(ResponseQueueSource source, ShuffleModeEnum shuffle, QueueItem current, List<QueueItem> excludeItemList)
    {
        BuiltSource? built = await Load(source);
        if (built is null) return null;

        Guid currentKey = Key(source, current);
        HashSet<Guid> excludeKeySet = excludeItemList.Select(x => Key(source, x)).ToHashSet();
        excludeKeySet.Add(currentKey);

        List<QueueItem> candidateList = built.ItemList;
        if (shuffle == ShuffleModeEnum.Off)
        {
            int index = built.ItemList.FindIndex(x => Key(source, x) == currentKey);
            if (index >= 0) candidateList = built.ItemList.GetRange(index + 1, built.ItemList.Count - index - 1);
        }

        List<QueueItem> remaining = candidateList.Where(x => !excludeKeySet.Contains(Key(source, x))).ToList();

        return serviceShuffle.Order(remaining, shuffle, ArtistOf(built));
    }

    public async Task<QueuePlacement?> PlaceNew(ResponseQueueSource source, ShuffleModeEnum shuffle, LiveQueue queue, Guid mediaFileID, Guid? entryID = null)
    {
        ResponseQueue current = queue.ToResponse();
        if (current.Current is null) return null;

        BuiltSource? built = await Load(source);
        if (built is null) return null;

        HashSet<Guid> queuedItemIDSet = [.. current.HistoryList.Select(x => x.ItemID), .. current.UserQueueList.Select(x => x.ItemID), .. current.SourceList.Select(x => x.ItemID), current.Current.ItemID];
        int newIndex = built.ItemList.FindIndex(x => IsNew(source, x, mediaFileID, entryID, queuedItemIDSet));
        if (newIndex < 0) return null;

        QueueItem item = built.ItemList[newIndex];
        if (shuffle != ShuffleModeEnum.Off)
        {
            int place = random.Next(current.SourceList.Count + 1);
            return new(item, place == 0 ? null : current.SourceList[place - 1].ItemID);
        }

        // In the source's own order the item belongs after the nearest earlier item that is still ahead (or the current one); when every earlier item is already played it sorts before the current item.
        Guid currentKey = Key(source, current.Current);
        for (int index = newIndex - 1; index >= 0; index--)
        {
            Guid key = Key(source, built.ItemList[index]);
            if (key == currentKey) return new(item, null);

            QueueItem? ahead = current.SourceList.Find(x => Key(source, x) == key);
            if (ahead is not null) return new(item, ahead.ItemID);
        }

        return null;
    }

    public async Task<ShuffleModeEnum> GetShuffle(ResponseQueueSource? source)
    {
        return (await FindPlaylist(source))?.Shuffle ?? await serviceSetting.GetShuffle();
    }

    public async Task<RepeatModeEnum> GetRepeat(ResponseQueueSource? source)
    {
        return (await FindPlaylist(source))?.Repeat ?? await serviceSetting.GetRepeat();
    }

    private async Task<ResponsePlaylist?> FindPlaylist(ResponseQueueSource? source)
    {
        if (source is not { Kind: QueueSourceKindEnum.Playlist, PlaylistID: { } playlistID }) return null;

        return (await storeMetadata.GetPlaylistList()).Find(x => x.ID == playlistID);
    }

    private static bool IsNew(ResponseQueueSource source, QueueItem item, Guid mediaFileID, Guid? entryID, HashSet<Guid> queuedItemIDSet)
    {
        if (source.Kind == QueueSourceKindEnum.Library) return item.MediaFileID == mediaFileID;

        return entryID is { } id ? item.ItemID == id : item.MediaFileID == mediaFileID && !queuedItemIDSet.Contains(item.ItemID);
    }

    // The thing that tells two items of this source apart: a Library item is its media file (each build gives new ItemIDs), a playlist item its entry.
    private static Guid Key(ResponseQueueSource source, QueueItem item)
    {
        return source.Kind switch
        {
            QueueSourceKindEnum.Library => item.MediaFileID,
            QueueSourceKindEnum.Playlist => item.ItemID,
            _ => throw new InvalidEnumArgumentException(nameof(source), (int)source.Kind, typeof(QueueSourceKindEnum))
        };
    }

    private static Func<Guid, string?> ArtistOf(BuiltSource built)
    {
        return mediaFileID => built.ArtistMap.GetValueOrDefault(mediaFileID);
    }

    private async Task<BuiltSource?> Load(ResponseQueueSource source)
    {
        return source.Kind switch
        {
            QueueSourceKindEnum.Library => await LoadLibrary(source),
            QueueSourceKindEnum.Playlist => await LoadPlaylist(source),
            _ => throw new InvalidEnumArgumentException(nameof(source), (int)source.Kind, typeof(QueueSourceKindEnum))
        };
    }

    // Sorting by ID first makes the order of equal keys the same on every device, whatever order the store holds.
    private async Task<BuiltSource> LoadLibrary(ResponseQueueSource source)
    {
        List<ResponseMediaFile> mediaFileList = (await storeMetadata.GetMediaFileList()).OrderBy(x => x.ID).ToList().Filter(source.Search).Sort(source.Sort, source.Descending);

        return new(mediaFileList.Select(x => new QueueItem(Guid.NewGuid(), x.ID)).ToList(), mediaFileList.ToDictionary(x => x.ID, x => x.Artist));
    }

    private async Task<BuiltSource?> LoadPlaylist(ResponseQueueSource source)
    {
        if (source.PlaylistID is not { } playlistID) return null;
        if (!(await storeMetadata.GetPlaylistList()).Any(x => x.ID == playlistID)) return null;

        ResponsePlaylistDetail? detail = await storeMetadata.GetPlaylistDetail(playlistID);
        if (detail is null) return null;

        List<ResponsePlaylistEntry> entryList = detail.EntryList.OrderBy(x => x.Position).ToList();
        Dictionary<Guid, string?> artistMap = [];
        foreach (ResponsePlaylistEntry entry in entryList) artistMap.TryAdd(entry.MediaFileID, entry.MediaFile.Artist);

        return new(entryList.Select(x => new QueueItem(x.ID, x.MediaFileID)).ToList(), artistMap);
    }
}
