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

public class HandlerAlbumMediaFileListAdd(
    IUnitOfWorkFactory factoryUnitOfWork,
    INameKeyService serviceNameKey,
    IMetadataChangeService serviceMetadataChange,
    IPublisher publisher,
    TimeProvider time) : BaseHandlerAlbumChange<CommandAlbumMediaFileListAdd, ResponseAlbum>(factoryUnitOfWork, serviceNameKey, serviceMetadataChange, publisher, time)
{
    // Sets the album of each file, replacing the album it had. Throws MediaNotFoundException for an unknown file or the file of another user, and then nothing changes. A file that is in the album already is left alone, and when none changes nothing is saved or pushed.
    public override async ValueTask<ResponseAlbum> Handle(CommandAlbumMediaFileListAdd command, CancellationToken cancellationToken)
    {
        return await Execute(command.UserID, async uow =>
        {
            EntityAlbum album = await Load(uow, command.UserID, command.ID, cancellationToken);
            MetadataChangeSet changeSet = new();
            List<EntityMediaFile> fileList = await LoadMediaFileList(uow, command.UserID, command.MediaFileIDList, changeSet, cancellationToken);
            int fileCount = (await uow.Metadata.GetMediaFileListByAlbum(command.UserID, album.ID, cancellationToken)).Count;

            int added = 0;
            foreach (EntityMediaFile file in fileList.Where(x => x.AlbumID != album.ID))
            {
                file.AlbumID = album.ID;
                file.Album = album;
                changeSet.Touch(file);
                added++;
            }

            if (added == 0) return new MetadataChangeResult<ResponseAlbum>(album.ToResponse(fileCount), false, changeSet, []);

            ResponseAlbum response = album.ToResponse(fileCount + added);
            changeSet.AddUpdated(response);

            List<ResponseMediaFile> mediaList = TouchChangeSet(uow, changeSet, Now);
            await uow.Commit(cancellationToken);

            return new MetadataChangeResult<ResponseAlbum>(response, true, changeSet, mediaList);
        }, cancellationToken);
    }
}
