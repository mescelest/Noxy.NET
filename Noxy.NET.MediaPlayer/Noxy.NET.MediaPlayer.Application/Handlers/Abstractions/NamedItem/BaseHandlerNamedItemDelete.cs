using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.NamedItem;

public abstract class BaseHandlerNamedItemDelete<TCommand, TItem, TItemResponse>(
    IUnitOfWorkFactory factoryUnitOfWork,
    INamedItemService<TItem, TItemResponse> serviceItem,
    INameKeyService serviceNameKey,
    IPublisher publisher,
    TimeProvider time) : BaseHandlerNamedItemChange<TCommand, Unit, TItem, TItemResponse>(factoryUnitOfWork, serviceItem, serviceNameKey, publisher, time) where TCommand : CommandNamedItemDelete where TItem : class, IEntityNamedItem
{
    // Removes the item from every file that has it and everywhere else it is used. The files are never deleted. Throws MetadataItemNotFoundException for an unknown item or the item of another user.
    public override async ValueTask<Unit> Handle(TCommand command, CancellationToken cancellationToken)
    {
        await Execute(command.UserID, async uow =>
        {
            TItem item = await Load(uow, command.UserID, command.ID, cancellationToken);
            DateTime now = Now;
            MetadataChangeSet changeSet = new();

            await Replace(uow, command.UserID, item, [], now, changeSet, cancellationToken);
            ServiceItem.Remove(uow, item);
            ServiceItem.AddDeleted(changeSet, item.ID);

            List<ResponseMediaFile> mediaList = TouchChangeSet(uow, changeSet, now);
            await uow.Commit(cancellationToken);

            return new MetadataChangeResult<Unit>(Unit.Value, true, changeSet, mediaList);
        }, cancellationToken);

        return Unit.Value;
    }
}
