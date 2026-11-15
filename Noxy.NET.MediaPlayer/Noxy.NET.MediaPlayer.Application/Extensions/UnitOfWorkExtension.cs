using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

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
    }
}
