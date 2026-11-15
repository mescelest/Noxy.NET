using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Interfaces;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.NamedItem;

public abstract class BaseHandlerNamedItemCreate<TCommand, TItem, TItemResponse>(
    IUnitOfWorkFactory factoryUnitOfWork,
    INamedItemService<TItem, TItemResponse> serviceItem,
    INameKeyService serviceNameKey,
    IPublisher publisher,
    TimeProvider time) : BaseHandlerNamedItemChange<TCommand, TItemResponse, TItem, TItemResponse>(factoryUnitOfWork, serviceItem, serviceNameKey, publisher, time) where TCommand : CommandNamedItemCreate<TItemResponse> where TItem : class, IEntityNamedItem
{
    // Throws NameAlreadyInUseException, naming the existing item, when the user already has an item with that name.
    public override async ValueTask<TItemResponse> Handle(TCommand command, CancellationToken cancellationToken)
    {
        (string name, string key) = NormalizeName(command.Name);

        return await Execute(command.UserID, async uow =>
        {
            TItem? existing = await ServiceItem.FindByKey(uow, command.UserID, key, cancellationToken);
            if (existing is not null) throw new NameAlreadyInUseException(existing.ID);

            TItem item = ServiceItem.Create(command.UserID, name, key);
            item.TimeModified = Now;
            ServiceItem.Add(uow, item);
            await uow.Commit(cancellationToken);

            MetadataChangeSet changeSet = new();
            TItemResponse response = ServiceItem.ToResponse(item, 0);
            ServiceItem.AddUpdated(changeSet, response);

            return new MetadataChangeResult<TItemResponse>(response, true, changeSet, []);
        }, cancellationToken);
    }
}
