using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.NamedItem;

public abstract class BaseHandlerNamedItemMerge<TCommand, TItem, TItemResponse>(
    IUnitOfWorkFactory factoryUnitOfWork,
    INamedItemService<TItem, TItemResponse> serviceItem,
    INameKeyService serviceNameKey,
    IPublisher publisher,
    TimeProvider time) : BaseHandlerNamedItemChange<TCommand, TItemResponse, TItem, TItemResponse>(factoryUnitOfWork, serviceItem, serviceNameKey, publisher, time) where TCommand : CommandNamedItemMerge<TItemResponse> where TItem : class, IEntityNamedItem
{
    // Moves everything of the item to the target and removes the item; the target keeps its name. Throws InvalidMetadataOperationException when the target is the item itself, and MetadataItemNotFoundException for an unknown item or the item of another user.
    public override async ValueTask<TItemResponse> Handle(TCommand command, CancellationToken cancellationToken)
    {
        if (command.ID == command.TargetID) throw new InvalidMetadataOperationException("An item cannot be merged into itself.");

        return await Execute(command.UserID, async uow =>
        {
            TItem source = await Load(uow, command.UserID, command.ID, cancellationToken);
            TItem target = await Load(uow, command.UserID, command.TargetID, cancellationToken);
            DateTime now = Now;
            MetadataChangeSet changeSet = new();

            TItemResponse response = await MergeInto(uow, command.UserID, source, target, now, changeSet, cancellationToken);

            List<ResponseMediaFile> mediaList = TouchChangeSet(uow, changeSet, now);
            await uow.Commit(cancellationToken);

            return new MetadataChangeResult<TItemResponse>(response, true, changeSet, mediaList);
        }, cancellationToken);
    }
}
