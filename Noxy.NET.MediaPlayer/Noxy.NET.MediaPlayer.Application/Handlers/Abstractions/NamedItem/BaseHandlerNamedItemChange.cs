using Mediator;
using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Interfaces;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.NamedItem;

public abstract class BaseHandlerNamedItemChange<TCommand, TCommandResponse, TItem, TItemResponse>(
    IUnitOfWorkFactory factoryUnitOfWork,
    INamedItemService<TItem, TItemResponse> serviceItem,
    INameKeyService serviceNameKey,
    IPublisher publisher,
    TimeProvider time) : BaseHandlerMetadataChange<TCommand, TCommandResponse>(factoryUnitOfWork, publisher, time) where TCommand : ICommand<TCommandResponse> where TItem : class, IEntityNamedItem
{
    public const int MaxNameLength = 255;

    protected INamedItemService<TItem, TItemResponse> ServiceItem { get; } = serviceItem;
    protected INameKeyService ServiceNameKey { get; } = serviceNameKey;

    protected async Task<TItem> Load(IUnitOfWork uow, Guid userID, Guid id, CancellationToken cancellationToken)
    {
        return await ServiceItem.Find(uow, userID, id, cancellationToken) ?? throw new MetadataItemNotFoundException();
    }

    // The name as it is stored and its key. A name with no key (blank) is never stored.
    protected (string Name, string Key) NormalizeName(string name)
    {
        string trimmed = name.Trim();
        string key = ServiceNameKey.GetKey(trimmed);
        if (key.Length == 0) throw new ArgumentException("The name cannot be blank.", nameof(name));
        return trimmed.Length <= MaxNameLength ? (trimmed, key) : throw new ArgumentException($"The name can be at most {MaxNameLength} characters.", nameof(name));
    }

    protected async Task<bool> IsMergeOnRename(IUnitOfWork uow, Guid userID, CancellationToken cancellationToken)
    {
        return await uow.GetBoolean(userID, SettingCatalog.MergeOnRename, cancellationToken);
    }

    // Every file whose response mentions the item, as the change set's own copies.
    protected async Task<List<EntityMediaFile>> LoadAffected(IUnitOfWork uow, Guid userID, Guid itemID, MetadataChangeSet changeSet, CancellationToken cancellationToken)
    {
        return changeSet.Adopt(await ServiceItem.GetAffectedMediaFileList(uow, userID, itemID, cancellationToken));
    }

    // Replaces the item by the replacement items on every file that carries it, in the same position and without repeating one the file already has, and everywhere else it is used. The item itself stays; the caller removes it or keeps it.
    protected async Task Replace(IUnitOfWork uow, Guid userID, TItem old, List<TItem> replacementList, DateTime now, MetadataChangeSet changeSet, CancellationToken cancellationToken)
    {
        foreach (EntityMediaFile file in await LoadAffected(uow, userID, old.ID, changeSet, cancellationToken))
        {
            List<TItem> currentList = ServiceItem.GetItemList(file);
            if (currentList.All(x => x.ID != old.ID)) continue;

            List<TItem> desiredList = [];
            foreach (TItem current in currentList)
            {
                IEnumerable<TItem> insertList = current.ID == old.ID ? replacementList : [current];
                foreach (TItem insert in insertList.Where(x => desiredList.All(d => d.ID != x.ID)))
                {
                    desiredList.Add(insert);
                }
            }

            if (ServiceItem.SetItemList(uow, file, desiredList)) changeSet.Touch(file);
        }

        await ServiceItem.OnItemReplaced(uow, userID, old, replacementList, now, changeSet, cancellationToken);
    }

    // Moves everything of the source to the target and removes the source. Returns the target response with its new file count.
    protected async Task<TItemResponse> MergeInto(IUnitOfWork uow, Guid userID, TItem source, TItem target, DateTime now, MetadataChangeSet changeSet, CancellationToken cancellationToken)
    {
        HashSet<Guid> fileIDSet = [.. (await ServiceItem.GetLinkedMediaFileList(uow, userID, target.ID, cancellationToken)).Select(x => x.ID)];
        fileIDSet.UnionWith((await ServiceItem.GetLinkedMediaFileList(uow, userID, source.ID, cancellationToken)).Select(x => x.ID));

        await Replace(uow, userID, source, [target], now, changeSet, cancellationToken);

        ServiceItem.Remove(uow, source);
        ServiceItem.AddDeleted(changeSet, source.ID);

        TItemResponse response = ServiceItem.ToResponse(target, fileIDSet.Count);
        ServiceItem.AddUpdated(changeSet, response);
        return response;
    }
}
