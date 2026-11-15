using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.NamedItem;

public abstract class BaseHandlerNamedItemSplit<TCommand, TItem, TItemResponse>(
    IUnitOfWorkFactory factoryUnitOfWork,
    INamedItemService<TItem, TItemResponse> serviceItem,
    INameKeyService serviceNameKey,
    IPublisher publisher,
    TimeProvider time) : BaseHandlerNamedItemChange<TCommand, List<TItemResponse>, TItem, TItemResponse>(factoryUnitOfWork, serviceItem, serviceNameKey, publisher, time)
    where TCommand : CommandNamedItemSplit<TItemResponse> where TItem : class, IEntityNamedItem
{
    // Replaces the item, on every file that has it and in the album artist lists, by the items with the given names, in the original position: existing items are reused and the rest created, by the usual rules for names. A name that is the item itself keeps it. Throws InvalidMetadataOperationException unless the names make at least two different items, and MetadataItemNotFoundException for an unknown item or the item of another user.
    public override async ValueTask<List<TItemResponse>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        List<(string Name, string Key)> nameList = [];
        foreach (string raw in command.NameList)
        {
            (string name, string key) = NormalizeName(raw);
            if (nameList.All(x => x.Key != key)) nameList.Add((name, key));
        }

        if (nameList.Count < 2) throw new InvalidMetadataOperationException("A split needs at least two different names.");

        return await Execute(command.UserID, async uow =>
        {
            TItem source = await Load(uow, command.UserID, command.ID, cancellationToken);
            DateTime now = Now;
            MetadataChangeSet changeSet = new();

            HashSet<Guid> sourceFileIDSet = [.. (await ServiceItem.GetLinkedMediaFileList(uow, command.UserID, source.ID, cancellationToken)).Select(x => x.ID)];

            List<TItem> targetList = [];
            Dictionary<Guid, HashSet<Guid>> fileIDSetByTarget = [];
            foreach ((string name, string key) in nameList)
            {
                TItem? target = key == source.NameKey ? source : await ServiceItem.FindByKey(uow, command.UserID, key, cancellationToken);
                HashSet<Guid> fileIDSet = [];
                if (target is null)
                {
                    target = ServiceItem.Create(command.UserID, name, key);
                    target.TimeModified = now;
                    ServiceItem.Add(uow, target);
                }
                else if (target.ID != source.ID)
                {
                    fileIDSet.UnionWith((await ServiceItem.GetLinkedMediaFileList(uow, command.UserID, target.ID, cancellationToken)).Select(x => x.ID));
                }

                if (targetList.Any(x => x.ID == target.ID)) continue;

                targetList.Add(target);
                fileIDSet.UnionWith(sourceFileIDSet);
                fileIDSetByTarget[target.ID] = fileIDSet;
            }

            await Replace(uow, command.UserID, source, targetList, now, changeSet, cancellationToken);

            if (targetList.All(x => x.ID != source.ID))
            {
                ServiceItem.Remove(uow, source);
                ServiceItem.AddDeleted(changeSet, source.ID);
            }

            List<TItemResponse> responseList = [];
            foreach (TItem target in targetList)
            {
                TItemResponse response = ServiceItem.ToResponse(target, fileIDSetByTarget[target.ID].Count);
                ServiceItem.AddUpdated(changeSet, response);
                responseList.Add(response);
            }

            List<ResponseMediaFile> mediaList = TouchChangeSet(uow, changeSet, now);
            await uow.Commit(cancellationToken);

            return new MetadataChangeResult<List<TItemResponse>>(responseList, true, changeSet, mediaList);
        }, cancellationToken);
    }
}
