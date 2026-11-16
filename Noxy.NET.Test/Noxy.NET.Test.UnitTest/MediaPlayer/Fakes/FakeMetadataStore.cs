using System.Collections.Concurrent;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeMetadataStore : IMetadataStore, ISettingStore, IMetadataListStore
{
    private readonly List<ResponseMediaFile> _mediaFileList = [];
    private readonly List<ResponsePlaylist> _playlistList = [];
    private readonly Dictionary<Guid, ResponsePlaylistDetail> _detailMap = [];
    private readonly ConcurrentDictionary<Guid, FileState> _fileStateMap = [];
    private string? _libraryETag;
    private string? _playlistListETag;
    private readonly List<ResponseSetting> _settingList = [];
    private readonly List<ResponseArtist> _artistList = [];
    private readonly List<ResponseAlbum> _albumList = [];
    private readonly List<ResponseGenre> _genreList = [];
    private string? _settingListETag;
    private string? _artistListETag;
    private string? _albumListETag;
    private string? _genreListETag;

    // Everything goes in and out as a copy through JSON, as it does with a real store, so a test can never change what is stored by changing what it got back.
    private static T Copy<T>(T value)
    {
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, JsonSerializerOptions.Web), JsonSerializerOptions.Web) ?? throw new InvalidOperationException($"A {typeof(T).Name} did not survive a copy.");
    }

    public Task<List<ResponseMediaFile>> GetMediaFileList()
    {
        return Task.FromResult(Copy(_mediaFileList));
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

    public Task<List<FileState>> GetFileStateList()
    {
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

    #region -- Settings --

    public Task<List<ResponseSetting>> GetSettingList()
    {
        return Task.FromResult(Copy(_settingList));
    }

    public Task ReplaceSettingList(List<ResponseSetting> settingList)
    {
        List<ResponseSetting> copy = Copy(settingList);
        _settingList.Clear();
        _settingList.AddRange(copy);

        return Task.CompletedTask;
    }

    public Task SaveSetting(ResponseSetting setting)
    {
        Upsert(_settingList, Copy(setting), x => x.Key);

        return Task.CompletedTask;
    }

    public Task<string?> GetSettingListETag()
    {
        return Task.FromResult(_settingListETag);
    }

    public Task SetSettingListETag(string? eTag)
    {
        _settingListETag = eTag;

        return Task.CompletedTask;
    }

    #endregion -- Settings --

    #region -- Artists, albums and genres --

    public Task<List<ResponseArtist>> GetArtistList()
    {
        return Task.FromResult(Copy(_artistList));
    }

    public Task ReplaceArtistList(List<ResponseArtist> artistList)
    {
        List<ResponseArtist> copy = Copy(artistList);
        _artistList.Clear();
        _artistList.AddRange(copy);

        return Task.CompletedTask;
    }

    public Task SaveArtist(ResponseArtist artist)
    {
        Upsert(_artistList, Copy(artist), x => x.ID);

        return Task.CompletedTask;
    }

    public Task DeleteArtist(Guid id)
    {
        _artistList.RemoveAll(x => x.ID == id);

        return Task.CompletedTask;
    }

    public Task<string?> GetArtistListETag()
    {
        return Task.FromResult(_artistListETag);
    }

    public Task SetArtistListETag(string? eTag)
    {
        _artistListETag = eTag;

        return Task.CompletedTask;
    }

    public Task<List<ResponseAlbum>> GetAlbumList()
    {
        return Task.FromResult(Copy(_albumList));
    }

    public Task ReplaceAlbumList(List<ResponseAlbum> albumList)
    {
        List<ResponseAlbum> copy = Copy(albumList);
        _albumList.Clear();
        _albumList.AddRange(copy);

        return Task.CompletedTask;
    }

    public Task SaveAlbum(ResponseAlbum album)
    {
        Upsert(_albumList, Copy(album), x => x.ID);

        return Task.CompletedTask;
    }

    public Task DeleteAlbum(Guid id)
    {
        _albumList.RemoveAll(x => x.ID == id);

        return Task.CompletedTask;
    }

    public Task<string?> GetAlbumListETag()
    {
        return Task.FromResult(_albumListETag);
    }

    public Task SetAlbumListETag(string? eTag)
    {
        _albumListETag = eTag;

        return Task.CompletedTask;
    }

    public Task<List<ResponseGenre>> GetGenreList()
    {
        return Task.FromResult(Copy(_genreList));
    }

    public Task ReplaceGenreList(List<ResponseGenre> genreList)
    {
        List<ResponseGenre> copy = Copy(genreList);
        _genreList.Clear();
        _genreList.AddRange(copy);

        return Task.CompletedTask;
    }

    public Task SaveGenre(ResponseGenre genre)
    {
        Upsert(_genreList, Copy(genre), x => x.ID);

        return Task.CompletedTask;
    }

    public Task DeleteGenre(Guid id)
    {
        _genreList.RemoveAll(x => x.ID == id);

        return Task.CompletedTask;
    }

    public Task<string?> GetGenreListETag()
    {
        return Task.FromResult(_genreListETag);
    }

    public Task SetGenreListETag(string? eTag)
    {
        _genreListETag = eTag;

        return Task.CompletedTask;
    }

    #endregion -- Artists, albums and genres --

    private static void Upsert<T, TKey>(List<T> list, T value, Func<T, TKey> idOf) where TKey : notnull
    {
        int index = list.FindIndex(x => idOf(x).Equals(idOf(value)));
        if (index >= 0) list[index] = value;
        else list.Add(value);
    }
}
