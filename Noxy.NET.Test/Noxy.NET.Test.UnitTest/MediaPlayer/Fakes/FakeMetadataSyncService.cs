using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeMetadataSyncService : IMetadataSyncService
{
    // Every call in the order it was made, as the method name.
    public List<string> CallList { get; } = [];

    public List<ResponseArtist> ArtistUpdatedList { get; } = [];

    public List<Guid> ArtistDeletedList { get; } = [];

    public List<ResponseAlbum> AlbumUpdatedList { get; } = [];

    public List<Guid> AlbumDeletedList { get; } = [];

    public List<ResponseGenre> GenreUpdatedList { get; } = [];

    public List<Guid> GenreDeletedList { get; } = [];

    // Thrown by the artist sync, after it is recorded, when set.
    public Exception? SyncArtistListException { get; set; }

    public event Action? Changed;

    public void RaiseChanged()
    {
        Changed?.Invoke();
    }

    public Task SyncArtistList(bool force = false)
    {
        CallList.Add(nameof(SyncArtistList));
        if (SyncArtistListException is not null) throw SyncArtistListException;

        return Task.CompletedTask;
    }

    public Task SyncAlbumList(bool force = false)
    {
        CallList.Add(nameof(SyncAlbumList));
        return Task.CompletedTask;
    }

    public Task SyncGenreList(bool force = false)
    {
        CallList.Add(nameof(SyncGenreList));
        return Task.CompletedTask;
    }

    public Task ApplyArtistUpdated(ResponseArtist artist)
    {
        CallList.Add(nameof(ApplyArtistUpdated));
        ArtistUpdatedList.Add(artist);
        return Task.CompletedTask;
    }

    public Task ApplyArtistDeleted(Guid id)
    {
        CallList.Add(nameof(ApplyArtistDeleted));
        ArtistDeletedList.Add(id);
        return Task.CompletedTask;
    }

    public Task ApplyAlbumUpdated(ResponseAlbum album)
    {
        CallList.Add(nameof(ApplyAlbumUpdated));
        AlbumUpdatedList.Add(album);
        return Task.CompletedTask;
    }

    public Task ApplyAlbumDeleted(Guid id)
    {
        CallList.Add(nameof(ApplyAlbumDeleted));
        AlbumDeletedList.Add(id);
        return Task.CompletedTask;
    }

    public Task ApplyGenreUpdated(ResponseGenre genre)
    {
        CallList.Add(nameof(ApplyGenreUpdated));
        GenreUpdatedList.Add(genre);
        return Task.CompletedTask;
    }

    public Task ApplyGenreDeleted(Guid id)
    {
        CallList.Add(nameof(ApplyGenreDeleted));
        GenreDeletedList.Add(id);
        return Task.CompletedTask;
    }
}
