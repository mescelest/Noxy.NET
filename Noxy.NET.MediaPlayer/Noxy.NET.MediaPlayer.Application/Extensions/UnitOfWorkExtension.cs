using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;

namespace Noxy.NET.MediaPlayer.Application.Extensions;

public static class UnitOfWorkExtension
{
    extension(IUnitOfWork uow)
    {
        // The entries are in position order, which is the order everything that reads or changes a playlist works on.
        public async Task<EntityPlaylist> LoadPlaylistWithEntries(Guid userID, Guid id, CancellationToken cancellationToken)
        {
            EntityPlaylist playlist = await uow.Playlist.FindPlaylistWithEntries(id, userID, cancellationToken) ?? throw new PlaylistNotFoundException();
            playlist.EntryList = [.. playlist.EntryList.OrderBy(x => x.Position)];
            return playlist;
        }

        // Makes the artists of the file exactly desiredList, in that order. Does nothing and returns false when they already are. The file's own list is updated too, so a response built from it afterwards is right.
        public bool SetArtistList(EntityMediaFile file, List<EntityArtist> desiredList)
        {
            if (file.ArtistLinkList.OrderBy(x => x.Position).Select(x => x.ArtistID).SequenceEqual(desiredList.Select(x => x.ID))) return false;

            foreach (EntityMediaFileArtist old in file.ArtistLinkList)
            {
                uow.Metadata.RemoveMediaFileArtist(old);
            }

            file.ArtistLinkList = [.. desiredList.Select((x, index) => new EntityMediaFileArtist { MediaFileID = file.ID, ArtistID = x.ID, Position = index, Artist = x })];
            foreach (EntityMediaFileArtist link in file.ArtistLinkList)
            {
                uow.Metadata.AddMediaFileArtist(link);
            }

            return true;
        }

        public bool SetGenreList(EntityMediaFile file, List<EntityGenre> desiredList)
        {
            if (file.GenreLinkList.OrderBy(x => x.Position).Select(x => x.GenreID).SequenceEqual(desiredList.Select(x => x.ID))) return false;

            foreach (EntityMediaFileGenre old in file.GenreLinkList)
            {
                uow.Metadata.RemoveMediaFileGenre(old);
            }

            file.GenreLinkList = [.. desiredList.Select((x, index) => new EntityMediaFileGenre { MediaFileID = file.ID, GenreID = x.ID, Position = index, Genre = x })];
            foreach (EntityMediaFileGenre link in file.GenreLinkList)
            {
                uow.Metadata.AddMediaFileGenre(link);
            }

            return true;
        }

        // Puts into the change set the items the file joined or left with their new file counts, so devices learn of new items and changed counts. The count is the committed one, which does not include this change yet, and an item that is new has none. Call it before the commit. The artists of an album the file has now are included, since one may be new and without any file.
        public async Task AddItemCountList(MetadataChangeSet changeSet, Guid userID, EntityMediaFile file, MediaFileItemSet before, CancellationToken cancellationToken)
        {
            MediaFileItemSet after = MediaFileItemSet.Of(file);

            // Album artists first: an artist that is also one of the file's artists is then replaced by the entry with the right count.
            foreach (EntityArtist artist in (after.Album?.ArtistLinkList ?? []).Select(x => x.Artist).OfType<EntityArtist>())
            {
                int count = (await uow.Metadata.GetMediaFileListByArtist(userID, artist.ID, cancellationToken)).Count;
                changeSet.AddUpdated(artist.ToResponse(count));
            }

            foreach ((EntityArtist artist, int delta) in GetMembershipChangeList(before.ArtistList, after.ArtistList, x => x.ID))
            {
                int count = (await uow.Metadata.GetMediaFileListByArtist(userID, artist.ID, cancellationToken)).Count;
                changeSet.AddUpdated(artist.ToResponse(Math.Max(0, count + delta)));
            }

            foreach ((EntityGenre genre, int delta) in GetMembershipChangeList(before.GenreList, after.GenreList, x => x.ID))
            {
                int count = (await uow.Metadata.GetMediaFileListByGenre(userID, genre.ID, cancellationToken)).Count;
                changeSet.AddUpdated(genre.ToResponse(Math.Max(0, count + delta)));
            }

            List<EntityAlbum> albumBeforeList = before.Album is null ? [] : [before.Album];
            List<EntityAlbum> albumAfterList = after.Album is null ? [] : [after.Album];
            foreach ((EntityAlbum album, int delta) in GetMembershipChangeList(albumBeforeList, albumAfterList, x => x.ID))
            {
                int count = (await uow.Metadata.GetMediaFileListByAlbum(userID, album.ID, cancellationToken)).Count;
                changeSet.AddUpdated(album.ToResponse(Math.Max(0, count + delta)));
            }
        }
    }

    // +1 for an item only in after, -1 for one only in before; an item in both is not listed.
    private static List<(TItem Item, int Delta)> GetMembershipChangeList<TItem>(List<TItem> beforeList, List<TItem> afterList, Func<TItem, Guid> getID)
    {
        HashSet<Guid> beforeSet = [.. beforeList.Select(getID)];
        HashSet<Guid> afterSet = [.. afterList.Select(getID)];

        List<(TItem Item, int Delta)> result = [];
        result.AddRange(afterList.Where(x => !beforeSet.Contains(getID(x))).Select(x => (x, 1)));
        result.AddRange(beforeList.Where(x => !afterSet.Contains(getID(x))).Select(x => (x, -1)));

        return result;
    }
}
