using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Album;

public class HandlerAlbumMerge(
    IUnitOfWorkFactory factoryUnitOfWork,
    INameKeyService serviceNameKey,
    IMetadataChangeService serviceMetadataChange,
    IPublisher publisher,
    TimeProvider time) : BaseHandlerAlbumChange<CommandAlbumMerge, ResponseAlbum>(factoryUnitOfWork, serviceNameKey, serviceMetadataChange, publisher, time)
{
    // Moves every file of the album to the target album and removes the album; the target keeps its own title and artists. Throws InvalidMetadataOperationException when the target is the album itself, and MetadataItemNotFoundException for an unknown album or the album of another user.
    public override async ValueTask<ResponseAlbum> Handle(CommandAlbumMerge command, CancellationToken cancellationToken)
    {
        if (command.ID == command.TargetID) throw new InvalidMetadataOperationException("An album cannot be merged into itself.");

        return await Execute(command.UserID, async uow =>
        {
            EntityAlbum source = await Load(uow, command.UserID, command.ID, cancellationToken);
            EntityAlbum target = await Load(uow, command.UserID, command.TargetID, cancellationToken);
            DateTime now = Now;
            MetadataChangeSet changeSet = new();

            await ServiceMetadataChange.MergeAlbum(uow, command.UserID, source, target, now, changeSet, cancellationToken);

            List<ResponseMediaFile> mediaList = TouchChangeSet(uow, changeSet, now);
            await uow.Commit(cancellationToken);

            return new MetadataChangeResult<ResponseAlbum>(changeSet.AlbumUpdatedList.Single(x => x.ID == target.ID), true, changeSet, mediaList);
        }, cancellationToken);
    }
}
