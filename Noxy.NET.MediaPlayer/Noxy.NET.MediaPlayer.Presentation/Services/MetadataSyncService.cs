using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class MetadataSyncService : IMetadataSyncService
{
    private readonly SyncedList<ResponseArtist, Guid> _artistList;
    private readonly SyncedList<ResponseAlbum, Guid> _albumList;
    private readonly SyncedList<ResponseGenre, Guid> _genreList;

    public MetadataSyncService(IApiClient clientApi, IMetadataListStore storeMetadataList)
    {
        _artistList = new(storeMetadataList.Artist, clientApi.GetArtistList, x => x.ID, x => x.TimeModified);
        _albumList = new(storeMetadataList.Album, clientApi.GetAlbumList, x => x.ID, x => x.TimeModified);
        _genreList = new(storeMetadataList.Genre, clientApi.GetGenreList, x => x.ID, x => x.TimeModified);

        _artistList.Changed += RaiseChanged;
        _albumList.Changed += RaiseChanged;
        _genreList.Changed += RaiseChanged;
    }

    public event Action? Changed;

    public async Task SyncArtistList(bool force = false)
    {
        await _artistList.Sync(force);
    }

    public async Task SyncAlbumList(bool force = false)
    {
        await _albumList.Sync(force);
    }

    public async Task SyncGenreList(bool force = false)
    {
        await _genreList.Sync(force);
    }

    public async Task ApplyArtistUpdated(ResponseArtist artist)
    {
        await _artistList.ApplyUpdated(artist);
    }

    public async Task ApplyArtistDeleted(Guid id)
    {
        await _artistList.ApplyDeleted(id);
    }

    public async Task ApplyAlbumUpdated(ResponseAlbum album)
    {
        await _albumList.ApplyUpdated(album);
    }

    public async Task ApplyAlbumDeleted(Guid id)
    {
        await _albumList.ApplyDeleted(id);
    }

    public async Task ApplyGenreUpdated(ResponseGenre genre)
    {
        await _genreList.ApplyUpdated(genre);
    }

    public async Task ApplyGenreDeleted(Guid id)
    {
        await _genreList.ApplyDeleted(id);
    }

    private void RaiseChanged()
    {
        Changed?.Invoke();
    }
}
