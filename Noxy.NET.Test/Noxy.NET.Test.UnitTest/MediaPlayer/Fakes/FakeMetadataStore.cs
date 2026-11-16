using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeMetadataStore : IMetadataStore
{
    private readonly List<ResponseMediaFile> _mediaFileList = [];
    private readonly List<ResponsePlaylist> _playlistList = [];
    private readonly Dictionary<Guid, ResponsePlaylistDetail> _detailMap = [];
    private string? _libraryETag;
    private string? _playlistListETag;

    // Everything goes in and out as a copy through JSON, as it does with a real store, so a test can never change what is stored by changing what it got back.
    private static T Copy<T>(T value)
    {
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, JsonSerializerOptions.Web), JsonSerializerOptions.Web) ?? throw new InvalidOperationException($"A {typeof(T).Name} did not survive a copy.");
    }

    public Task<List<ResponseMediaFile>> GetMediaFileList()
    {
        return Task.FromResult(Copy(_mediaFileList));
    }

    public Task ReplaceMediaFileList(List<ResponseMediaFile> mediaFileList)
    {
        List<ResponseMediaFile> copy = Copy(mediaFileList);
        _mediaFileList.Clear();
        _mediaFileList.AddRange(copy);

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

    private static void Upsert<T>(List<T> list, T value, Func<T, Guid> idOf)
    {
        int index = list.FindIndex(x => idOf(x) == idOf(value));
        if (index >= 0) list[index] = value;
        else list.Add(value);
    }
}
