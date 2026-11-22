using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.NamedItem;

public abstract class BaseHandlerNamedItemMediaFileListAdd<TCommand, TItem, TItemResponse>(
    IUnitOfWorkFactory factoryUnitOfWork,
    INamedItemService<TItem, TItemResponse> serviceItem,
    INameKeyService serviceNameKey,
    IPublisher publisher,
    TimeProvider time) : BaseHandlerNamedItemChange<TCommand, TItemResponse, TItem, TItemResponse>(factoryUnitOfWork, serviceItem, serviceNameKey, publisher, time)
    where TCommand : CommandNamedItemMediaFileListAdd<TItemResponse> where TItem : class, IEntityNamedItem
{
    // Adds the item at the end of the items of each file, unless the file has it already. Throws MediaNotFoundException for an unknown file or the file of another user, and then nothing changes. Assigning what is already there changes nothing, saves nothing and pushes nothing.
    public override async ValueTask<TItemResponse> Handle(TCommand command, CancellationToken cancellationToken)
    {
        return await Execute(command.UserID, async uow =>
        {
            TItem item = await Load(uow, command.UserID, command.ID, cancellationToken);
            MetadataChangeSet changeSet = new();
            List<EntityMediaFile> fileList = await LoadMediaFileList(uow, command.UserID, command.MediaFileIDList, changeSet, cancellationToken);
            int linkedCount = (await ServiceItem.GetLinkedMediaFileList(uow, command.UserID, item.ID, cancellationToken)).Count;

            int added = 0;
            foreach (EntityMediaFile file in fileList)
            {
                List<TItem> itemList = ServiceItem.GetItemList(file);
                if (itemList.Any(x => x.ID == item.ID)) continue;

                itemList.Add(item);
                if (!ServiceItem.SetItemList(uow, file, itemList)) continue;

                changeSet.Touch(file);
                added++;
            }

            if (added == 0) return new(ServiceItem.ToResponse(item, linkedCount), false, changeSet, []);

            TItemResponse response = ServiceItem.ToResponse(item, linkedCount + added);
            ServiceItem.AddUpdated(changeSet, response);

            DateTime now = Now;
            List<ResponseMediaFile> mediaList = TouchChangeSet(uow, changeSet, now);
            await uow.Commit(cancellationToken);

            return new MetadataChangeResult<TItemResponse>(response, true, changeSet, mediaList);
        }, cancellationToken);
    }
}
