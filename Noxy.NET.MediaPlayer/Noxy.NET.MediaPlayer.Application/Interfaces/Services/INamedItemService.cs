using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Interfaces;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface INamedItemService<TItem, TResponse> where TItem : class, IEntityNamedItem
{
    Task<TItem?> Find(IUnitOfWork uow, Guid userID, Guid id, CancellationToken cancellationToken = default);

    // Includes an item added earlier in the same unit of work.
    Task<TItem?> FindByKey(IUnitOfWork uow, Guid userID, string key, CancellationToken cancellationToken = default);

    TItem Create(Guid userID, string name, string key);
    void Add(IUnitOfWork uow, TItem item);
    void Update(IUnitOfWork uow, TItem item);

    // Its links go with it.
    void Remove(IUnitOfWork uow, TItem item);

    // The files that carry the item itself, links loaded. Their number is the item's file count.
    Task<List<EntityMediaFile>> GetLinkedMediaFileList(IUnitOfWork uow, Guid userID, Guid itemID, CancellationToken cancellationToken = default);

    // The files whose response mentions the item: the linked ones and, for an artist, the files whose album lists it as an album artist.
    Task<List<EntityMediaFile>> GetAffectedMediaFileList(IUnitOfWork uow, Guid userID, Guid itemID, CancellationToken cancellationToken = default);

    // The file's items of this kind in position order.
    List<TItem> GetItemList(EntityMediaFile file);

    // Makes the file's items exactly desiredList. False, and nothing saved, when they already are.
    bool SetItemList(IUnitOfWork uow, EntityMediaFile file, List<TItem> desiredList);

    // The names the loaded file shows follow the item's current name.
    void ApplyRename(EntityMediaFile file, TItem item);

    // Called when an item is replaced by others everywhere (merged, split or deleted, the replacement then being empty), so an artist can update the albums that list it.
    Task OnItemReplaced(IUnitOfWork uow, Guid userID, TItem old, List<TItem> replacementList, DateTime now, MetadataChangeSet changeSet, CancellationToken cancellationToken = default);

    TResponse ToResponse(TItem item, int mediaFileCount);
    void AddUpdated(MetadataChangeSet changeSet, TResponse response);
    void AddDeleted(MetadataChangeSet changeSet, Guid id);
}
