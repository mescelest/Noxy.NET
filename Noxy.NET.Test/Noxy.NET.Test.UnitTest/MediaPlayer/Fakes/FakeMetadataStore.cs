using System.Collections.Concurrent;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeMetadataStore : IMetadataStore, IMetadataListStore, IVolumeStore
{
    private readonly List<ResponseMediaFile> _mediaFileList = [];
    private readonly List<ResponsePlaylist> _playlistList = [];
    private readonly Dictionary<Guid, ResponsePlaylistDetail> _detailMap = [];
    private readonly ConcurrentDictionary<Guid, FileState> _fileStateMap = [];
    private string? _libraryETag;
    private string? _playlistListETag;
    private int _volume = 100;

    // Everything goes in and out as a copy through JSON, as it does with a real store, so a test can never change what is stored by changing what it got back.
    private static T Copy<T>(T value)
    {
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, JsonSerializerOptions.Web), JsonSerializerOptions.Web) ?? throw new InvalidOperationException($"A {typeof(T).Name} did not survive a copy.");
    }

    // When set, GetMediaFileList waits for it, so a test can hold a read of the library in flight.
    public Task? MediaFileListGate { get; set; }

    public int MediaFileListReadCount { get; private set; }

    public int FileStateListReadCount { get; private set; }

    public async Task<List<ResponseMediaFile>> GetMediaFileList()
    {
        MediaFileListReadCount++;
        if (MediaFileListGate is not null) await MediaFileListGate;

        return Copy(_mediaFileList);
    }

    public Task<ResponseMediaFile?> GetMediaFile(Guid id)
    {
        ResponseMediaFile? mediaFile = _mediaFileList.FirstOrDefault(x => x.ID == id);

        return Task.FromResult(mediaFile is null ? null : Copy(mediaFile));
    }

    public Task ReplaceMediaFileList(List<ResponseMediaFile> mediaFileList)
    {
        List<ResponseMediaFile> copy = Copy(mediaFileList);
        _mediaFileList.Clear();
        _mediaFileList.AddRange(copy);

        HashSet<Guid> idSet = [.. copy.Select(x => x.ID)];
        foreach (Guid id in _fileStateMap.Keys.Where(x => !idSet.Contains(x)).ToList())
        {
            _fileStateMap.TryRemove(id, out _);
        }

        return Task.CompletedTask;
    }

    public Task SaveMediaFile(ResponseMediaFile mediaFile)
    {
        Upsert(_mediaFileList, Copy(mediaFile), x => x.ID);

        return Task.CompletedTask;
    }

    public Task DeleteMediaFile(Guid id)
    {
        _mediaFileList.RemoveAll(x => x.ID == id);
        _fileStateMap.TryRemove(id, out _);

        return Task.CompletedTask;
    }

    public Task<List<ResponsePlaylist>> GetPlaylistList()
    {
        return Task.FromResult(Copy(_playlistList));
    }

    public Task ReplacePlaylistList(List<ResponsePlaylist> playlistList)
    {
        List<ResponsePlaylist> copy = Copy(playlistList);
        _playlistList.Clear();
        _playlistList.AddRange(copy);

        HashSet<Guid> idSet = [.. copy.Select(x => x.ID)];
        foreach (Guid id in _detailMap.Keys.Where(x => !idSet.Contains(x)).ToList())
        {
            _detailMap.Remove(id);
        }

        return Task.CompletedTask;
    }

    public Task SavePlaylist(ResponsePlaylist playlist)
    {
        Upsert(_playlistList, Copy(playlist), x => x.ID);

        return Task.CompletedTask;
    }

    public Task DeletePlaylist(Guid id)
    {
        _playlistList.RemoveAll(x => x.ID == id);
        _detailMap.Remove(id);

        return Task.CompletedTask;
    }

    public Task<ResponsePlaylistDetail?> GetPlaylistDetail(Guid id)
    {
        return Task.FromResult(_detailMap.TryGetValue(id, out ResponsePlaylistDetail? detail) ? Copy(detail) : null);
    }

    public Task SavePlaylistDetail(ResponsePlaylistDetail detail)
    {
        _detailMap[detail.ID] = Copy(detail);

        return Task.CompletedTask;
    }

    public Task<List<Guid>> GetHeldPlaylistIDList()
    {
        return Task.FromResult(_detailMap.Keys.ToList());
    }

    public Task<string?> GetLibraryETag()
    {
        return Task.FromResult(_libraryETag);
    }

    public Task SetLibraryETag(string? eTag)
    {
        _libraryETag = eTag;

        return Task.CompletedTask;
    }

    public Task<string?> GetPlaylistListETag()
    {
        return Task.FromResult(_playlistListETag);
    }

    public Task SetPlaylistListETag(string? eTag)
    {
        _playlistListETag = eTag;

        return Task.CompletedTask;
    }

    private Guid? _ownerUserID;

    public Task<Guid?> GetOwnerUserID()
    {
        return Task.FromResult(_ownerUserID);
    }

    public Task SetOwnerUserID(Guid? id)
    {
        _ownerUserID = id;
        return Task.CompletedTask;
    }

    public Task<List<FileState>> GetFileStateList()
    {
        FileStateListReadCount++;
        return Task.FromResult(_fileStateMap.Values.ToList());
    }

    // Every state saved, in order, so a test can check the sequence a download went through.
    public List<FileState> FileStateHistoryList { get; } = [];

    private readonly Lock _historyLock = new();

    public Task SaveFileState(FileState fileState)
    {
        _fileStateMap[fileState.MediaFileID] = fileState;
        lock (_historyLock) FileStateHistoryList.Add(fileState);

        return Task.CompletedTask;
    }

    public Task DeleteFileState(Guid mediaFileID)
    {
        _fileStateMap.TryRemove(mediaFileID, out _);

        return Task.CompletedTask;
    }

    public FakeListStore<ResponseSetting, string> Setting { get; } = new(x => x.Key);

    public FakeListStore<ResponseArtist, Guid> Artist { get; } = new(x => x.ID);

    public FakeListStore<ResponseAlbum, Guid> Album { get; } = new(x => x.ID);

    public FakeListStore<ResponseGenre, Guid> Genre { get; } = new(x => x.ID);

    IListStore<ResponseArtist, Guid> IMetadataListStore.Artist => Artist;

    IListStore<ResponseAlbum, Guid> IMetadataListStore.Album => Album;

    IListStore<ResponseGenre, Guid> IMetadataListStore.Genre => Genre;

    #region -- Volume --

    // Thrown by GetVolume or SetVolume when set, as a store that cannot be read or written would.
    public Exception? VolumeReadFailure { get; set; }

    public Exception? VolumeWriteFailure { get; set; }

    // Every volume saved, in order.
    public List<int> VolumeWriteList { get; } = [];

    public Task<int> GetVolume()
    {
        if (VolumeReadFailure is not null) throw VolumeReadFailure;

        return Task.FromResult(_volume);
    }

    public Task SetVolume(int volume)
    {
        if (VolumeWriteFailure is not null) throw VolumeWriteFailure;

        _volume = volume;
        VolumeWriteList.Add(volume);

        return Task.CompletedTask;
    }

    #endregion -- Volume --

    private static void Upsert<T, TKey>(List<T> list, T value, Func<T, TKey> idOf) where TKey : notnull
    {
        int index = list.FindIndex(x => idOf(x).Equals(idOf(value)));
        if (index >= 0) list[index] = value;
        else list.Add(value);
    }
}
