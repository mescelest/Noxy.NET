using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Models.Metadata;

public class MetadataChangeSet
{
    private readonly Dictionary<Guid, EntityMediaFile> _fileMap = [];
    private readonly HashSet<Guid> _touchedSet = [];

    public List<ResponseArtist> ArtistUpdatedList { get; } = [];
    public List<ResponseGenre> GenreUpdatedList { get; } = [];
    public List<ResponseAlbum> AlbumUpdatedList { get; } = [];
    public List<Guid> ArtistDeletedIDList { get; } = [];
    public List<Guid> GenreDeletedIDList { get; } = [];
    public List<Guid> AlbumDeletedIDList { get; } = [];

    // The files whose responses changed, in the order they were first touched.
    public List<EntityMediaFile> TouchedFileList { get; } = [];

    // The copy this change set already holds of the file, or the given one when it is new to it.
    public EntityMediaFile Adopt(EntityMediaFile loaded)
    {
        if (_fileMap.TryGetValue(loaded.ID, out EntityMediaFile? held)) return held;

        _fileMap[loaded.ID] = loaded;
        return loaded;
    }

    public List<EntityMediaFile> Adopt(IEnumerable<EntityMediaFile> loadedList)
    {
        return [.. loadedList.Select(Adopt)];
    }

    // The file's response changed, so it will get a new modified time and be pushed.
    public void Touch(EntityMediaFile file)
    {
        if (_touchedSet.Add(file.ID)) TouchedFileList.Add(file);
    }

    public void AddUpdated(ResponseArtist response)
    {
        ArtistUpdatedList.RemoveAll(x => x.ID == response.ID);
        ArtistUpdatedList.Add(response);
    }

    public void AddUpdated(ResponseGenre response)
    {
        GenreUpdatedList.RemoveAll(x => x.ID == response.ID);
        GenreUpdatedList.Add(response);
    }

    public void AddUpdated(ResponseAlbum response)
    {
        AlbumUpdatedList.RemoveAll(x => x.ID == response.ID);
        AlbumUpdatedList.Add(response);
    }

    // A deleted item is not also reported as updated.
    public void AddDeletedArtist(Guid id)
    {
        ArtistUpdatedList.RemoveAll(x => x.ID == id);
        if (!ArtistDeletedIDList.Contains(id)) ArtistDeletedIDList.Add(id);
    }

    public void AddDeletedGenre(Guid id)
    {
        GenreUpdatedList.RemoveAll(x => x.ID == id);
        if (!GenreDeletedIDList.Contains(id)) GenreDeletedIDList.Add(id);
    }

    public void AddDeletedAlbum(Guid id)
    {
        AlbumUpdatedList.RemoveAll(x => x.ID == id);
        if (!AlbumDeletedIDList.Contains(id)) AlbumDeletedIDList.Add(id);
    }
}
