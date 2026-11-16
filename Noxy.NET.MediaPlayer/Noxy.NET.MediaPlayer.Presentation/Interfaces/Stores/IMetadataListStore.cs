using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

public interface IMetadataListStore
{
    Task<List<ResponseArtist>> GetArtistList();

    // A Replace is one atomic write that keeps the order of the list; a saved item that exists stays where it is and a new one goes last. The three kinds never touch each other.
    Task ReplaceArtistList(List<ResponseArtist> artistList);

    Task SaveArtist(ResponseArtist artist);

    // An ID that is not held is harmless.
    Task DeleteArtist(Guid id);

    // Only ever one that arrived with a full 200 response the device downloaded. Null clears it.
    Task<string?> GetArtistListETag();

    Task SetArtistListETag(string? eTag);

    Task<List<ResponseAlbum>> GetAlbumList();

    Task ReplaceAlbumList(List<ResponseAlbum> albumList);

    Task SaveAlbum(ResponseAlbum album);

    Task DeleteAlbum(Guid id);

    Task<string?> GetAlbumListETag();

    Task SetAlbumListETag(string? eTag);

    Task<List<ResponseGenre>> GetGenreList();

    Task ReplaceGenreList(List<ResponseGenre> genreList);

    Task SaveGenre(ResponseGenre genre);

    Task DeleteGenre(Guid id);

    Task<string?> GetGenreListETag();

    Task SetGenreListETag(string? eTag);
}
