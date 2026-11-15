using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IMetadataChangeService
{
    // Replaces the artist in the artist list of every album that has it. Nothing is committed; the albums, files and notifications are recorded in the change set.
    Task ReplaceAlbumArtist(IUnitOfWork uow, Guid userID, EntityArtist old, List<EntityArtist> replacementList, DateTime now, MetadataChangeSet changeSet, CancellationToken cancellationToken = default);

    // Moves every file of the source album to the target album and removes the source. The target keeps its own artists.
    Task MergeAlbum(IUnitOfWork uow, Guid userID, EntityAlbum source, EntityAlbum target, DateTime now, MetadataChangeSet changeSet, CancellationToken cancellationToken = default);
}
