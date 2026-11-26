using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.NamedItem;

public abstract class BaseHandlerNamedItemSetWeight<TCommand, TItem, TItemResponse>(
    IUnitOfWorkFactory factoryUnitOfWork,
    INamedItemService<TItem, TItemResponse> serviceItem,
    INameKeyService serviceNameKey,
    IPublisher publisher,
    TimeProvider time) : BaseHandlerNamedItemChange<TCommand, TItemResponse, TItem, TItemResponse>(factoryUnitOfWork, serviceItem, serviceNameKey, publisher, time) where TCommand : CommandNamedItemSetWeight<TItemResponse> where TItem : class, IEntityNamedItem
{
    // Sets where the item stands in the list of a song (MetadataWeight). Every file that carries the item lists its items in a new order, so those files are touched and pushed with the item; the same weight changes nothing. Throws ArgumentOutOfRangeException for a weight outside MetadataWeight.Minimum to Maximum, and MetadataItemNotFoundException for an unknown item or another user's.
    public override async ValueTask<TItemResponse> Handle(TCommand command, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(command.Weight, MetadataWeight.Minimum, nameof(command));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(command.Weight, MetadataWeight.Maximum, nameof(command));

        return await Execute(command.UserID, async uow =>
        {
            TItem item = await Load(uow, command.UserID, command.ID, cancellationToken);
            List<EntityMediaFile> linkedList = await ServiceItem.GetLinkedMediaFileList(uow, command.UserID, item.ID, cancellationToken);

            if (command.Weight == item.Weight) return new(ServiceItem.ToResponse(item, linkedList.Count), false, new(), []);

            DateTime now = Now;
            MetadataChangeSet changeSet = new();

            item.Weight = command.Weight;
            item.TimeModified = now;
            ServiceItem.Update(uow, item);

            // The files carry their own copy of the item: it takes the new weight, so their response lists the items in the new order.
            foreach (EntityMediaFile file in changeSet.Adopt(linkedList))
            {
                ServiceItem.ApplyRename(file, item);
                changeSet.Touch(file);
            }

            TItemResponse response = ServiceItem.ToResponse(item, linkedList.Count);
            ServiceItem.AddUpdated(changeSet, response);

            List<ResponseMediaFile> mediaList = TouchChangeSet(uow, changeSet, now);
            await uow.Commit(cancellationToken);

            return new MetadataChangeResult<TItemResponse>(response, true, changeSet, mediaList);
        }, cancellationToken);
    }
}
