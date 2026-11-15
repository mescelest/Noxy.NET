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
    }
}
