using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.NamedItem;

public abstract class BaseHandlerNamedItemMediaFileListRemove<TCommand, TItem, TItemResponse>(
    IUnitOfWorkFactory factoryUnitOfWork,
    INamedItemService<TItem, TItemResponse> serviceItem,
    INameKeyService serviceNameKey,
    IPublisher publisher,
    TimeProvider time) : BaseHandlerNamedItemChange<TCommand, TItemResponse, TItem, TItemResponse>(factoryUnitOfWork, serviceItem, serviceNameKey, publisher, time)
    where TCommand : CommandNamedItemMediaFileListRemove<TItemResponse> where TItem : class, IEntityNamedItem
{
    // Clears the item from each of the files, leaving their other items. Throws MediaNotFoundException for an unknown file or the file of another user, and then nothing changes. A file that does not have the item is left alone.
    public override async ValueTask<TItemResponse> Handle(TCommand command, CancellationToken cancellationToken)
    {
        return await Execute(command.UserID, async uow =>
        {
            TItem item = await Load(uow, command.UserID, command.ID, cancellationToken);
            MetadataChangeSet changeSet = new();
            List<EntityMediaFile> fileList = await LoadMediaFileList(uow, command.UserID, command.MediaFileIDList, changeSet, cancellationToken);
            int linkedCount = (await ServiceItem.GetLinkedMediaFileList(uow, command.UserID, item.ID, cancellationToken)).Count;

            int removed = 0;
            foreach (EntityMediaFile file in fileList)
            {
                List<TItem> itemList = ServiceItem.GetItemList(file);
                if (itemList.RemoveAll(x => x.ID == item.ID) == 0) continue;
                if (!ServiceItem.SetItemList(uow, file, itemList)) continue;

                changeSet.Touch(file);
                removed++;
            }

            if (removed == 0) return new(ServiceItem.ToResponse(item, linkedCount), false, changeSet, []);

            TItemResponse response = ServiceItem.ToResponse(item, linkedCount - removed);
            ServiceItem.AddUpdated(changeSet, response);

            DateTime now = Now;
            List<ResponseMediaFile> mediaList = TouchChangeSet(uow, changeSet, now);
            await uow.Commit(cancellationToken);

            return new MetadataChangeResult<TItemResponse>(response, true, changeSet, mediaList);
        }, cancellationToken);
    }
}
