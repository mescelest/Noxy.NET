using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class GenreItemService : INamedItemService<EntityGenre, ResponseGenre>
{
    public Task<EntityGenre?> Find(IUnitOfWork uow, Guid userID, Guid id, CancellationToken cancellationToken = default)
    {
        return uow.Metadata.FindGenre(id, userID, cancellationToken);
    }

    public Task<EntityGenre?> FindByKey(IUnitOfWork uow, Guid userID, string key, CancellationToken cancellationToken = default)
    {
        return uow.Metadata.FindGenreByKey(userID, key, cancellationToken);
    }

    public EntityGenre Create(Guid userID, string name, string key)
    {
        return new() { UserID = userID, Name = name, NameKey = key };
    }

    public void Add(IUnitOfWork uow, EntityGenre item)
    {
        uow.Metadata.AddGenre(item);
    }

    public void Update(IUnitOfWork uow, EntityGenre item)
    {
        uow.Metadata.UpdateGenre(item);
    }

    public void Remove(IUnitOfWork uow, EntityGenre item)
    {
        uow.Metadata.RemoveGenre(item);
    }

    public Task<List<EntityMediaFile>> GetLinkedMediaFileList(IUnitOfWork uow, Guid userID, Guid itemID, CancellationToken cancellationToken = default)
    {
        return uow.Metadata.GetMediaFileListByGenre(userID, itemID, cancellationToken);
    }

    public Task<List<EntityMediaFile>> GetAffectedMediaFileList(IUnitOfWork uow, Guid userID, Guid itemID, CancellationToken cancellationToken = default)
    {
        return uow.Metadata.GetMediaFileListByGenre(userID, itemID, cancellationToken);
    }

    public List<EntityGenre> GetItemList(EntityMediaFile file)
    {
        return [.. file.GenreLinkList.OrderBy(x => x.Position).Select(x => x.Genre ?? throw new InvalidOperationException($"The media file {file.ID} was loaded without its genres."))];
    }

    public bool SetItemList(IUnitOfWork uow, EntityMediaFile file, List<EntityGenre> desiredList)
    {
        return uow.SetGenreList(file, desiredList);
    }

    public void ApplyRename(EntityMediaFile file, EntityGenre item)
    {
        foreach (EntityMediaFileGenre link in file.GenreLinkList.Where(x => x.GenreID == item.ID))
        {
            link.Genre = item;
        }
    }

    public Task OnItemReplaced(IUnitOfWork uow, Guid userID, EntityGenre old, List<EntityGenre> replacementList, DateTime now, MetadataChangeSet changeSet, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public ResponseGenre ToResponse(EntityGenre item, int mediaFileCount)
    {
        return item.ToResponse(mediaFileCount);
    }

    public void AddUpdated(MetadataChangeSet changeSet, ResponseGenre response)
    {
        changeSet.AddUpdated(response);
    }

    public void AddDeleted(MetadataChangeSet changeSet, Guid id)
    {
        changeSet.AddDeletedGenre(id);
    }
}
