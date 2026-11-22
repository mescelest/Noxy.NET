using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Album;

public class HandlerAlbumMediaFileListRemove(
    IUnitOfWorkFactory factoryUnitOfWork,
    INameKeyService serviceNameKey,
    IMetadataChangeService serviceMetadataChange,
    IPublisher publisher,
    TimeProvider time) : BaseHandlerAlbumChange<CommandAlbumMediaFileListRemove, ResponseAlbum>(factoryUnitOfWork, serviceNameKey, serviceMetadataChange, publisher, time)
{
    // Clears the album from each of the files that are in it. Throws MediaNotFoundException for an unknown file or the file of another user, and then nothing changes.
    public override async ValueTask<ResponseAlbum> Handle(CommandAlbumMediaFileListRemove command, CancellationToken cancellationToken)
    {
        return await Execute(command.UserID, async uow =>
        {
            EntityAlbum album = await Load(uow, command.UserID, command.ID, cancellationToken);
            MetadataChangeSet changeSet = new();
            List<EntityMediaFile> fileList = await LoadMediaFileList(uow, command.UserID, command.MediaFileIDList, changeSet, cancellationToken);
            int fileCount = (await uow.Metadata.GetMediaFileListByAlbum(command.UserID, album.ID, cancellationToken)).Count;

            int removed = 0;
            foreach (EntityMediaFile file in fileList.Where(x => x.AlbumID == album.ID))
            {
                file.AlbumID = null;
                file.Album = null;
                changeSet.Touch(file);
                removed++;
            }

            if (removed == 0) return new(album.ToResponse(fileCount), false, changeSet, []);

            ResponseAlbum response = album.ToResponse(fileCount - removed);
            changeSet.AddUpdated(response);

            List<ResponseMediaFile> mediaList = TouchChangeSet(uow, changeSet, Now);
            await uow.Commit(cancellationToken);

            return new MetadataChangeResult<ResponseAlbum>(response, true, changeSet, mediaList);
        }, cancellationToken);
    }
}
