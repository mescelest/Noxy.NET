using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.NamedItem;

public abstract class BaseHandlerNamedItemRename<TCommand, TItem, TItemResponse>(
    IUnitOfWorkFactory factoryUnitOfWork,
    INamedItemService<TItem, TItemResponse> serviceItem,
    INameKeyService serviceNameKey,
    IPublisher publisher,
    TimeProvider time) : BaseHandlerNamedItemChange<TCommand, TItemResponse, TItem, TItemResponse>(factoryUnitOfWork, serviceItem, serviceNameKey, publisher, time) where TCommand : CommandNamedItemRename<TItemResponse> where TItem : class, IEntityNamedItem
{
    // Renames the item. A different spelling of the same name (case, spacing) only changes how it is shown. A name that another item of the user already has is a NameAlreadyInUseException, naming that item, unless the settings of the user say to merge on rename: then this item is merged into the other one, which keeps its name.
    public override async ValueTask<TItemResponse> Handle(TCommand command, CancellationToken cancellationToken)
    {
        (string name, string key) = NormalizeName(command.Name);

        return await Execute(command.UserID, async uow =>
        {
            TItem item = await Load(uow, command.UserID, command.ID, cancellationToken);
            int linkedCount = (await ServiceItem.GetLinkedMediaFileList(uow, command.UserID, item.ID, cancellationToken)).Count;

            if (name == item.Name) return new MetadataChangeResult<TItemResponse>(ServiceItem.ToResponse(item, linkedCount), false, new(), []);

            DateTime now = Now;
            MetadataChangeSet changeSet = new();

            if (key != item.NameKey)
            {
                TItem? other = await ServiceItem.FindByKey(uow, command.UserID, key, cancellationToken);
                if (other is not null && other.ID != item.ID)
                {
                    if (!await IsMergeOnRename(uow, command.UserID, cancellationToken)) throw new NameAlreadyInUseException(other.ID);

                    TItemResponse merged = await MergeInto(uow, command.UserID, item, other, now, changeSet, cancellationToken);
                    List<ResponseMediaFile> mergedMediaList = TouchChangeSet(uow, changeSet, now);
                    await uow.Commit(cancellationToken);

                    return new MetadataChangeResult<TItemResponse>(merged, true, changeSet, mergedMediaList);
                }
            }

            item.Name = name;
            item.NameKey = key;
            item.TimeModified = now;
            ServiceItem.Update(uow, item);

            // Every file that shows the name changes with it.
            foreach (EntityMediaFile file in await LoadAffected(uow, command.UserID, item.ID, changeSet, cancellationToken))
            {
                ServiceItem.ApplyRename(file, item);
                changeSet.Touch(file);
            }

            TItemResponse response = ServiceItem.ToResponse(item, linkedCount);
            ServiceItem.AddUpdated(changeSet, response);

            List<ResponseMediaFile> mediaList = TouchChangeSet(uow, changeSet, now);
            await uow.Commit(cancellationToken);

            return new MetadataChangeResult<TItemResponse>(response, true, changeSet, mediaList);
        }, cancellationToken);
    }
}
