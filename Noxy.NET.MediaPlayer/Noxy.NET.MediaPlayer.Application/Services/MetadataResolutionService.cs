using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class MetadataResolutionService(INameKeyService serviceNameKey, ICreditSplittingService serviceCreditSplitting) : IMetadataResolutionService
{
    private const int MaxNameLength = 255;

    public async Task<ResolvedMetadata> Resolve(IUnitOfWork uow, Guid userID, MediaTagSet tags, CancellationToken cancellationToken = default)
    {
        List<EntityArtist> artistList = await ResolveArtistList(uow, userID, tags.ArtistList, cancellationToken);
        EntityAlbum? album = await ResolveAlbum(uow, userID, tags.Album, tags.AlbumArtistList, cancellationToken);
        List<EntityGenre> genreList = await ResolveGenreList(uow, userID, tags.GenreList, cancellationToken);

        return new(artistList, album, genreList);
    }

    private async Task<List<EntityArtist>> ResolveArtistList(IUnitOfWork uow, Guid userID, List<string> valueList, CancellationToken cancellationToken)
    {
        List<EntityArtist> result = [];

        foreach ((string name, string key) in GetNameKeyList(valueList))
        {
            EntityArtist? artist = await uow.Metadata.FindArtistByKey(userID, key, cancellationToken);
            if (artist is null)
            {
                artist = new() { UserID = userID, Name = name, NameKey = key };
                uow.Metadata.AddArtist(artist);
            }

            if (result.All(x => x.ID != artist.ID)) result.Add(artist);
        }

        return result;
    }

    private async Task<List<EntityGenre>> ResolveGenreList(IUnitOfWork uow, Guid userID, List<string> valueList, CancellationToken cancellationToken)
    {
        List<EntityGenre> result = [];

        foreach ((string name, string key) in GetNameKeyList(valueList))
        {
            EntityGenre? genre = await uow.Metadata.FindGenreByKey(userID, key, cancellationToken);
            if (genre is null)
            {
                genre = new() { UserID = userID, Name = name, NameKey = key };
                uow.Metadata.AddGenre(genre);
            }

            if (result.All(x => x.ID != genre.ID)) result.Add(genre);
        }

        return result;
    }

    private async Task<EntityAlbum?> ResolveAlbum(IUnitOfWork uow, Guid userID, string? title, List<string> albumArtistValueList, CancellationToken cancellationToken)
    {
        string? cutTitle = Cut(title);
        string titleKey = cutTitle is null ? string.Empty : serviceNameKey.GetKey(cutTitle);

        // Without a title there is no album, and its album artists would have nothing to belong to.
        if (cutTitle is null || titleKey.Length == 0) return null;

        // A title made only of separators (";") has no usable text.
        if (serviceCreditSplitting.Split([cutTitle]).Count == 0) return null;

        List<EntityArtist> albumArtistList = await ResolveArtistList(uow, userID, albumArtistValueList, cancellationToken);
        string identityKey = EntityAlbum.BuildIdentityKey(titleKey, albumArtistList.Select(x => x.ID));

        EntityAlbum? album = await uow.Metadata.FindAlbumByIdentityKey(userID, identityKey, cancellationToken);
        if (album is not null) return album;

        album = new() { UserID = userID, Title = cutTitle, TitleKey = titleKey, IdentityKey = identityKey };
        album.ArtistLinkList = [.. albumArtistList.Select((x, index) => new EntityAlbumArtist { AlbumID = album.ID, ArtistID = x.ID, Position = index, Artist = x })];
        uow.Metadata.AddAlbum(album);

        return album;
    }

    // The names in order: credits split, each cut to the column length, and repeats by key removed.
    private List<(string Name, string Key)> GetNameKeyList(List<string> valueList)
    {
        List<(string Name, string Key)> result = [];

        foreach (string value in serviceCreditSplitting.Split(valueList))
        {
            string? name = Cut(value);
            if (name is null) continue;

            string key = serviceNameKey.GetKey(name);
            if (key.Length == 0 || result.Any(x => x.Key == key)) continue;

            result.Add((name, key));
        }

        return result;
    }

    // Trimmed and cut to the column length, or null when nothing is left.
    private static string? Cut(string? value)
    {
        string trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0) return null;

        return trimmed.CutAt(MaxNameLength);
    }
}
