using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Album;

public class HandlerAlbumDelete(
    IUnitOfWorkFactory factoryUnitOfWork,
    INameKeyService serviceNameKey,
    IMetadataChangeService serviceMetadataChange,
    IPublisher publisher,
    TimeProvider time) : BaseHandlerAlbumChange<CommandAlbumDelete, Unit>(factoryUnitOfWork, serviceNameKey, serviceMetadataChange, publisher, time)
{
    // Removes the album from its files, which stay. Throws MetadataItemNotFoundException for an unknown album or the album of another user.
    public override async ValueTask<Unit> Handle(CommandAlbumDelete command, CancellationToken cancellationToken)
    {
        await Execute(command.UserID, async uow =>
        {
            EntityAlbum album = await Load(uow, command.UserID, command.ID, cancellationToken);
            DateTime now = Now;
            MetadataChangeSet changeSet = new();

            foreach (EntityMediaFile file in changeSet.Adopt(await uow.Metadata.GetMediaFileListByAlbum(command.UserID, album.ID, cancellationToken)))
            {
                file.AlbumID = null;
                file.Album = null;
                changeSet.Touch(file);
            }

            uow.Metadata.RemoveAlbum(album);
            changeSet.AddDeletedAlbum(album.ID);

            List<ResponseMediaFile> mediaList = TouchChangeSet(uow, changeSet, now);
            await uow.Commit(cancellationToken);

            return new MetadataChangeResult<Unit>(Unit.Value, true, changeSet, mediaList);
        }, cancellationToken);

        return Unit.Value;
    }
}
