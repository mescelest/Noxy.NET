using System.Collections.Concurrent;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class ArtService : IArtService
{
    private const string DataUriPrefix = "data:image/jpeg;base64,";

    private readonly IApiClient _clientApi;
    private readonly IArtCacheStore _storeArtCache;
    private readonly IMetadataListStore _storeMetadataList;

    // What is ready to show, so a list of rows does not read the disk for every row at every render.
    private readonly ConcurrentDictionary<Guid, (DateTime Version, string Uri)> _readyMap = [];

    // The version of an album the server said it has no picture for: not asked again until the album's art time changes.
    private readonly ConcurrentDictionary<Guid, DateTime> _missingMap = [];

    // The art time each album had the last time it was asked about (null for none), to tell what changed in a sync.
    private readonly ConcurrentDictionary<Guid, DateTime?> _knownMap = [];

    private readonly ConcurrentDictionary<(Guid AlbumID, DateTime Version), Lazy<Task<string?>>> _loadingMap = [];

    public ArtService(IApiClient clientApi, IArtCacheStore storeArtCache, IMetadataListStore storeMetadataList, IMetadataSyncService serviceMetadataSync)
    {
        _clientApi = clientApi;
        _storeArtCache = storeArtCache;
        _storeMetadataList = storeMetadataList;

        serviceMetadataSync.Changed += OnMetadataChanged;
    }

    public event Action? Changed;

    public async Task<string?> GetAlbumArtUri(Guid albumID)
    {
        List<ResponseAlbum> albumList = await _storeMetadataList.Album.GetList();
        ResponseAlbum? album = albumList.FirstOrDefault(x => x.ID == albumID);
        DateTime? version = album?.TimeArtModified;
        _knownMap[albumID] = version;

        if (version is not { } current) return null;
        if (_readyMap.TryGetValue(albumID, out (DateTime Version, string Uri) ready) && ready.Version == current) return ready.Uri;
        if (_missingMap.TryGetValue(albumID, out DateTime missing) && missing == current) return null;

        // Calls for the same picture share the one request.
        Lazy<Task<string?>> loading = _loadingMap.GetOrAdd((albumID, current), key => new(() => Load(key.AlbumID, key.Version)));
        try
        {
            return await loading.Value;
        }
        finally
        {
            _loadingMap.TryRemove(new KeyValuePair<(Guid AlbumID, DateTime Version), Lazy<Task<string?>>>((albumID, current), loading));
        }
    }

    // Never throws for a picture that cannot be had: the caller shows the default tile.
    private async Task<string?> Load(Guid albumID, DateTime version)
    {
        try
        {
            byte[]? bytes = await _storeArtCache.Get(albumID, version);
            if (bytes is null)
            {
                bytes = await _clientApi.GetAlbumArt(albumID);
                if (bytes is null)
                {
                    _missingMap[albumID] = version;
                    return null;
                }

                await _storeArtCache.Save(albumID, version, bytes);
            }

            string uri = DataUriPrefix + Convert.ToBase64String(bytes);
            _readyMap[albumID] = (version, uri);
            return uri;
        }
        catch (Exception)
        {
            // The network or the disk failed: the old copy stays, and the next call tries again.
            return null;
        }
    }

    private void OnMetadataChanged()
    {
        // The sync raises this from wherever it ran, so the work goes on its own and a failure there never reaches it.
        _ = RefreshAfterSync();
    }

    private async Task RefreshAfterSync()
    {
        try
        {
            List<ResponseAlbum> albumList = await _storeMetadataList.Album.GetList();
            bool changed = false;

            foreach ((Guid albumID, DateTime? known) in _knownMap.ToList())
            {
                ResponseAlbum? album = albumList.FirstOrDefault(x => x.ID == albumID);
                if (album is null)
                {
                    await Forget(albumID);
                    changed = true;
                }
                else if (album.TimeArtModified != known)
                {
                    _knownMap[albumID] = album.TimeArtModified;
                    _readyMap.TryRemove(albumID, out _);
                    _missingMap.TryRemove(albumID, out _);
                    changed = true;
                }
            }

            if (changed) Changed?.Invoke();
        }
        catch (Exception)
        {
            // What shows a picture asks again at its next render, so a missed refresh costs nothing that stays.
        }
    }

    private async Task Forget(Guid albumID)
    {
        _knownMap.TryRemove(albumID, out _);
        _readyMap.TryRemove(albumID, out _);
        _missingMap.TryRemove(albumID, out _);
        await _storeArtCache.Delete(albumID);
    }
}
