using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class MetadataSyncService(IApiClient clientApi, IMetadataListStore storeMetadataList) : IMetadataSyncService
{
    public event Action? Changed;

    public async Task SyncArtistList(bool force = false)
    {
        string? eTag = force || (await storeMetadataList.GetArtistList()).Count == 0 ? null : await storeMetadataList.GetArtistListETag();

        ApiResponse<List<ResponseArtist>>? response = await Fetch(() => clientApi.GetArtistList(eTag));
        if (response?.Value is null) return;

        await storeMetadataList.ReplaceArtistList(response.Value);
        await storeMetadataList.SetArtistListETag(response.ETag);
        Changed?.Invoke();
    }

    public async Task SyncAlbumList(bool force = false)
    {
        string? eTag = force || (await storeMetadataList.GetAlbumList()).Count == 0 ? null : await storeMetadataList.GetAlbumListETag();

        ApiResponse<List<ResponseAlbum>>? response = await Fetch(() => clientApi.GetAlbumList(eTag));
        if (response?.Value is null) return;

        await storeMetadataList.ReplaceAlbumList(response.Value);
        await storeMetadataList.SetAlbumListETag(response.ETag);
        Changed?.Invoke();
    }

    public async Task SyncGenreList(bool force = false)
    {
        string? eTag = force || (await storeMetadataList.GetGenreList()).Count == 0 ? null : await storeMetadataList.GetGenreListETag();

        ApiResponse<List<ResponseGenre>>? response = await Fetch(() => clientApi.GetGenreList(eTag));
        if (response?.Value is null) return;

        await storeMetadataList.ReplaceGenreList(response.Value);
        await storeMetadataList.SetGenreListETag(response.ETag);
        Changed?.Invoke();
    }

    public async Task ApplyArtistUpdated(ResponseArtist artist)
    {
        ResponseArtist? local = (await storeMetadataList.GetArtistList()).FirstOrDefault(x => x.ID == artist.ID);
        if (local is not null && artist.TimeModified <= local.TimeModified) return;

        await storeMetadataList.SaveArtist(artist);
        Changed?.Invoke();
    }

    public async Task ApplyArtistDeleted(Guid id)
    {
        await storeMetadataList.DeleteArtist(id);
        Changed?.Invoke();
    }

    public async Task ApplyAlbumUpdated(ResponseAlbum album)
    {
        ResponseAlbum? local = (await storeMetadataList.GetAlbumList()).FirstOrDefault(x => x.ID == album.ID);
        if (local is not null && album.TimeModified <= local.TimeModified) return;

        await storeMetadataList.SaveAlbum(album);
        Changed?.Invoke();
    }

    public async Task ApplyAlbumDeleted(Guid id)
    {
        await storeMetadataList.DeleteAlbum(id);
        Changed?.Invoke();
    }

    public async Task ApplyGenreUpdated(ResponseGenre genre)
    {
        ResponseGenre? local = (await storeMetadataList.GetGenreList()).FirstOrDefault(x => x.ID == genre.ID);
        if (local is not null && genre.TimeModified <= local.TimeModified) return;

        await storeMetadataList.SaveGenre(genre);
        Changed?.Invoke();
    }

    public async Task ApplyGenreDeleted(Guid id)
    {
        await storeMetadataList.DeleteGenre(id);
        Changed?.Invoke();
    }

    // Null when the server could not be reached or answered 304. Any other failure propagates.
    private static async Task<ApiResponse<T>?> Fetch<T>(Func<Task<ApiResponse<T>>> call)
    {
        try
        {
            return await call();
        }
        catch (Exception exception) when (exception.IsOfflineFailure)
        {
            return null;
        }
    }
}
