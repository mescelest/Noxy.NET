using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeMetadataRepository(FakeStore store) : IMetadataRepository
{
    private readonly List<EntityArtist> _artistAddList = [];
    private readonly List<EntityArtist> _artistUpdateList = [];
    private readonly List<Guid> _artistRemovalList = [];
    private readonly List<EntityGenre> _genreAddList = [];
    private readonly List<EntityGenre> _genreUpdateList = [];
    private readonly List<Guid> _genreRemovalList = [];
    private readonly List<EntityAlbum> _albumAddList = [];
    private readonly List<EntityAlbum> _albumUpdateList = [];
    private readonly List<Guid> _albumRemovalList = [];
    private readonly List<EntityMediaFileArtist> _fileArtistAddList = [];
    private readonly List<Guid> _fileArtistRemovalList = [];
    private readonly List<EntityMediaFileGenre> _fileGenreAddList = [];
    private readonly List<Guid> _fileGenreRemovalList = [];
    private readonly List<EntityAlbumArtist> _albumArtistAddList = [];
    private readonly List<Guid> _albumArtistRemovalList = [];

    public void Flush()
    {
        // The unique indexes: a second item with the same per-user key is rejected, like the database does.
        foreach (EntityArtist added in _artistAddList)
        {
            if (store.ArtistList.Any(x => x.UserID == added.UserID && x.NameKey == added.NameKey)) ThrowConflict();
            store.ArtistList.Add(added);
        }

        _artistAddList.Clear();

        foreach (EntityGenre added in _genreAddList)
        {
            if (store.GenreList.Any(x => x.UserID == added.UserID && x.NameKey == added.NameKey)) ThrowConflict();
            store.GenreList.Add(added);
        }

        _genreAddList.Clear();

        foreach (EntityAlbum added in _albumAddList)
        {
            if (store.AlbumList.Any(x => x.UserID == added.UserID && x.IdentityKey == added.IdentityKey)) ThrowConflict();
            store.AlbumList.Add(added);
        }

        _albumAddList.Clear();

        foreach (EntityArtist updated in _artistUpdateList)
        {
            if (store.ArtistList.Any(x => x.ID != updated.ID && x.UserID == updated.UserID && x.NameKey == updated.NameKey)) ThrowConflict();

            int index = store.ArtistList.FindIndex(x => x.ID == updated.ID);
            if (index >= 0) store.ArtistList[index] = updated;
        }

        _artistUpdateList.Clear();

        foreach (EntityGenre updated in _genreUpdateList)
        {
            if (store.GenreList.Any(x => x.ID != updated.ID && x.UserID == updated.UserID && x.NameKey == updated.NameKey)) ThrowConflict();

            int index = store.GenreList.FindIndex(x => x.ID == updated.ID);
            if (index >= 0) store.GenreList[index] = updated;
        }

        _genreUpdateList.Clear();

        foreach (EntityAlbum updated in _albumUpdateList)
        {
            if (store.AlbumList.Any(x => x.ID != updated.ID && x.UserID == updated.UserID && x.IdentityKey == updated.IdentityKey)) ThrowConflict();

            EntityAlbum? existing = store.AlbumList.FirstOrDefault(x => x.ID == updated.ID);
            if (existing is null) continue;

            // The row only: the artist links are saved on their own.
            existing.Title = updated.Title;
            existing.TitleKey = updated.TitleKey;
            existing.IdentityKey = updated.IdentityKey;
            existing.TimeModified = updated.TimeModified;
            existing.TimeArtModified = updated.TimeArtModified;
        }

        _albumUpdateList.Clear();

        foreach (EntityMediaFileArtist link in _fileArtistAddList)
        {
            store.MediaFileList.FirstOrDefault(x => x.ID == link.MediaFileID)?.ArtistLinkList.Add(link);
        }

        _fileArtistAddList.Clear();

        foreach (EntityMediaFileGenre link in _fileGenreAddList)
        {
            store.MediaFileList.FirstOrDefault(x => x.ID == link.MediaFileID)?.GenreLinkList.Add(link);
        }

        _fileGenreAddList.Clear();

        foreach (EntityAlbumArtist link in _albumArtistAddList)
        {
            store.AlbumList.FirstOrDefault(x => x.ID == link.AlbumID)?.ArtistLinkList.Add(link);
        }

        _albumArtistAddList.Clear();

        foreach (EntityMediaFile file in store.MediaFileList)
        {
            file.ArtistLinkList.RemoveAll(x => _fileArtistRemovalList.Contains(x.ID));
            file.GenreLinkList.RemoveAll(x => _fileGenreRemovalList.Contains(x.ID));
        }

        _fileArtistRemovalList.Clear();
        _fileGenreRemovalList.Clear();

        foreach (EntityAlbum album in store.AlbumList)
        {
            album.ArtistLinkList.RemoveAll(x => _albumArtistRemovalList.Contains(x.ID));
        }

        _albumArtistRemovalList.Clear();

        // The database cascades: a removed item leaves every link that points at it.
        foreach (EntityMediaFile file in store.MediaFileList)
        {
            file.ArtistLinkList.RemoveAll(x => _artistRemovalList.Contains(x.ArtistID));
            file.GenreLinkList.RemoveAll(x => _genreRemovalList.Contains(x.GenreID));
            if (file.AlbumID is { } albumID && _albumRemovalList.Contains(albumID)) file.AlbumID = null;
        }

        foreach (EntityAlbum album in store.AlbumList)
        {
            album.ArtistLinkList.RemoveAll(x => _artistRemovalList.Contains(x.ArtistID));
        }

        store.ArtistList.RemoveAll(x => _artistRemovalList.Contains(x.ID));
        store.GenreList.RemoveAll(x => _genreRemovalList.Contains(x.ID));
        store.AlbumList.RemoveAll(x => _albumRemovalList.Contains(x.ID));
        _artistRemovalList.Clear();
        _genreRemovalList.Clear();
        _albumRemovalList.Clear();
    }

    private static void ThrowConflict()
    {
        throw new MetadataKeyConflictException();
    }

    #region -- Artist --

    public void AddArtist(EntityArtist artist)
    {
        _artistAddList.Add(FakeStore.CopyArtist(artist));
    }

    public Task<EntityArtist?> FindArtist(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FakeStore.CopyArtist(store.ArtistList.FirstOrDefault(x => x.ID == id && x.UserID == userID)));
    }

    public Task<EntityArtist?> FindArtistByKey(Guid userID, string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FakeStore.CopyArtist(_artistAddList.Concat(store.ArtistList).FirstOrDefault(x => x.UserID == userID && x.NameKey == key)));
    }

    public Task<List<ArtistSummary>> GetArtistSummaryList(Guid userID, CancellationToken cancellationToken = default)
    {
        List<ArtistSummary> list = store.ArtistList
            .Where(x => x.UserID == userID)
            .OrderByDescending(x => x.TimeCreated)
            .ThenByDescending(x => x.ID)
            .Select(x => new ArtistSummary(FakeStore.CopyArtist(x), store.MediaFileList.Count(f => f.ArtistLinkList.Any(l => l.ArtistID == x.ID))))
            .ToList();

        return Task.FromResult(list);
    }

    public Task<List<MetadataStamp>> GetArtistStampList(Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.ArtistList.Where(x => x.UserID == userID).Select(x => new MetadataStamp(x.ID, x.TimeModified, store.MediaFileList.Count(f => f.ArtistLinkList.Any(l => l.ArtistID == x.ID)))).ToList());
    }

    public void UpdateArtist(EntityArtist artist)
    {
        _artistUpdateList.Add(FakeStore.CopyArtist(artist));
    }

    public void RemoveArtist(EntityArtist artist)
    {
        _artistRemovalList.Add(artist.ID);
    }

    public Task<List<EntityMediaFile>> GetMediaFileListByArtist(Guid userID, Guid artistID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaFileList.Where(x => x.UserID == userID && x.ArtistLinkList.Any(l => l.ArtistID == artistID)).OrderByDescending(x => x.TimeCreated).ThenByDescending(x => x.ID).Select(store.CopyMediaFile).ToList());
    }

    public void AddMediaFileArtist(EntityMediaFileArtist link)
    {
        _fileArtistAddList.Add(new() { ID = link.ID, TimeCreated = link.TimeCreated, MediaFileID = link.MediaFileID, ArtistID = link.ArtistID, Position = link.Position });
    }

    public void RemoveMediaFileArtist(EntityMediaFileArtist link)
    {
        _fileArtistRemovalList.Add(link.ID);
    }

    #endregion -- Artist --

    #region -- Genre --

    public void AddGenre(EntityGenre genre)
    {
        _genreAddList.Add(FakeStore.CopyGenre(genre));
    }

    public Task<EntityGenre?> FindGenre(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FakeStore.CopyGenre(store.GenreList.FirstOrDefault(x => x.ID == id && x.UserID == userID)));
    }

    public Task<EntityGenre?> FindGenreByKey(Guid userID, string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FakeStore.CopyGenre(_genreAddList.Concat(store.GenreList).FirstOrDefault(x => x.UserID == userID && x.NameKey == key)));
    }

    public Task<List<GenreSummary>> GetGenreSummaryList(Guid userID, CancellationToken cancellationToken = default)
    {
        List<GenreSummary> list = store.GenreList
            .Where(x => x.UserID == userID)
            .OrderByDescending(x => x.TimeCreated)
            .ThenByDescending(x => x.ID)
            .Select(x => new GenreSummary(FakeStore.CopyGenre(x), store.MediaFileList.Count(f => f.GenreLinkList.Any(l => l.GenreID == x.ID))))
            .ToList();

        return Task.FromResult(list);
    }

    public Task<List<MetadataStamp>> GetGenreStampList(Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.GenreList.Where(x => x.UserID == userID).Select(x => new MetadataStamp(x.ID, x.TimeModified, store.MediaFileList.Count(f => f.GenreLinkList.Any(l => l.GenreID == x.ID)))).ToList());
    }

    public void UpdateGenre(EntityGenre genre)
    {
        _genreUpdateList.Add(FakeStore.CopyGenre(genre));
    }

    public void RemoveGenre(EntityGenre genre)
    {
        _genreRemovalList.Add(genre.ID);
    }

    public Task<List<EntityMediaFile>> GetMediaFileListByGenre(Guid userID, Guid genreID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaFileList.Where(x => x.UserID == userID && x.GenreLinkList.Any(l => l.GenreID == genreID)).OrderByDescending(x => x.TimeCreated).ThenByDescending(x => x.ID).Select(store.CopyMediaFile).ToList());
    }

    public void AddMediaFileGenre(EntityMediaFileGenre link)
    {
        _fileGenreAddList.Add(new() { ID = link.ID, TimeCreated = link.TimeCreated, MediaFileID = link.MediaFileID, GenreID = link.GenreID, Position = link.Position });
    }

    public void RemoveMediaFileGenre(EntityMediaFileGenre link)
    {
        _fileGenreRemovalList.Add(link.ID);
    }

    #endregion -- Genre --

    #region -- Album --

    public void AddAlbum(EntityAlbum album)
    {
        EntityAlbum copy = store.CopyAlbum(album);
        _albumAddList.Add(copy);
    }

    public Task<EntityAlbum?> FindAlbum(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.CopyAlbum(store.AlbumList.FirstOrDefault(x => x.ID == id && x.UserID == userID)));
    }

    public Task<EntityAlbum?> FindAlbumByIdentityKey(Guid userID, string identityKey, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.CopyAlbum(_albumAddList.Concat(store.AlbumList).FirstOrDefault(x => x.UserID == userID && x.IdentityKey == identityKey)));
    }

    public Task<List<AlbumSummary>> GetAlbumSummaryList(Guid userID, CancellationToken cancellationToken = default)
    {
        List<AlbumSummary> list = store.AlbumList
            .Where(x => x.UserID == userID)
            .OrderByDescending(x => x.TimeCreated)
            .ThenByDescending(x => x.ID)
            .Select(x => new AlbumSummary(store.CopyAlbum(x), store.MediaFileList.Count(f => f.AlbumID == x.ID)))
            .ToList();

        return Task.FromResult(list);
    }

    public Task<List<MetadataStamp>> GetAlbumStampList(Guid userID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.AlbumList.Where(x => x.UserID == userID).Select(x => new MetadataStamp(x.ID, x.TimeModified, store.MediaFileList.Count(f => f.AlbumID == x.ID), x.TimeArtModified)).ToList());
    }


    public void UpdateAlbum(EntityAlbum album)
    {
        _albumUpdateList.Add(store.CopyAlbum(album));
    }

    public void RemoveAlbum(EntityAlbum album)
    {
        _albumRemovalList.Add(album.ID);
    }

    public Task<List<EntityMediaFile>> GetMediaFileListByAlbum(Guid userID, Guid albumID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.MediaFileList.Where(x => x.UserID == userID && x.AlbumID == albumID).OrderByDescending(x => x.TimeCreated).ThenByDescending(x => x.ID).Select(store.CopyMediaFile).ToList());
    }

    public Task<List<EntityAlbum>> GetAlbumListByArtist(Guid userID, Guid artistID, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.AlbumList.Where(x => x.UserID == userID && x.ArtistLinkList.Any(l => l.ArtistID == artistID)).OrderBy(x => x.TimeCreated).ThenBy(x => x.ID).Select(x => store.CopyAlbum(x)).ToList());
    }

    public void AddAlbumArtist(EntityAlbumArtist link)
    {
        _albumArtistAddList.Add(new() { ID = link.ID, TimeCreated = link.TimeCreated, AlbumID = link.AlbumID, ArtistID = link.ArtistID, Position = link.Position });
    }

    public void RemoveAlbumArtist(EntityAlbumArtist link)
    {
        _albumArtistRemovalList.Add(link.ID);
    }

    #endregion -- Album --
}
