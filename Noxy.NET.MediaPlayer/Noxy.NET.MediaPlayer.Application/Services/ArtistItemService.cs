using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class ArtistItemService(IMetadataChangeService serviceMetadataChange) : INamedItemService<EntityArtist, ResponseArtist>
{
    public Task<EntityArtist?> Find(IUnitOfWork uow, Guid userID, Guid id, CancellationToken cancellationToken = default)
    {
        return uow.Metadata.FindArtist(id, userID, cancellationToken);
    }

    public Task<EntityArtist?> FindByKey(IUnitOfWork uow, Guid userID, string key, CancellationToken cancellationToken = default)
    {
        return uow.Metadata.FindArtistByKey(userID, key, cancellationToken);
    }

    public EntityArtist Create(Guid userID, string name, string key)
    {
        return new() { UserID = userID, Name = name, NameKey = key };
    }

    public void Add(IUnitOfWork uow, EntityArtist item)
    {
        uow.Metadata.AddArtist(item);
    }

    public void Update(IUnitOfWork uow, EntityArtist item)
    {
        uow.Metadata.UpdateArtist(item);
    }

    public void Remove(IUnitOfWork uow, EntityArtist item)
    {
        uow.Metadata.RemoveArtist(item);
    }

    public Task<List<EntityMediaFile>> GetLinkedMediaFileList(IUnitOfWork uow, Guid userID, Guid itemID, CancellationToken cancellationToken = default)
    {
        return uow.Metadata.GetMediaFileListByArtist(userID, itemID, cancellationToken);
    }

    public async Task<List<EntityMediaFile>> GetAffectedMediaFileList(IUnitOfWork uow, Guid userID, Guid itemID, CancellationToken cancellationToken = default)
    {
        List<EntityMediaFile> fileList = await uow.Metadata.GetMediaFileListByArtist(userID, itemID, cancellationToken);

        // The album artist text of a file comes from its album.
        foreach (EntityAlbum album in await uow.Metadata.GetAlbumListByArtist(userID, itemID, cancellationToken))
        {
            fileList.AddRange((await uow.Metadata.GetMediaFileListByAlbum(userID, album.ID, cancellationToken)).Where(x => fileList.All(f => f.ID != x.ID)));
        }

        return fileList;
    }

    public List<EntityArtist> GetItemList(EntityMediaFile file)
    {
        return [.. file.ArtistLinkList.OrderBy(x => x.Position).Select(x => x.Artist ?? throw new InvalidOperationException($"The media file {file.ID} was loaded without its artists."))];
    }

    public bool SetItemList(IUnitOfWork uow, EntityMediaFile file, List<EntityArtist> desiredList)
    {
        return uow.SetArtistList(file, desiredList);
    }

    public void ApplyRename(EntityMediaFile file, EntityArtist item)
    {
        foreach (EntityMediaFileArtist link in file.ArtistLinkList.Where(x => x.ArtistID == item.ID))
        {
            link.Artist = item;
        }

        foreach (EntityAlbumArtist link in file.Album?.ArtistLinkList.Where(x => x.ArtistID == item.ID) ?? [])
        {
            link.Artist = item;
        }
    }

    public Task OnItemReplaced(IUnitOfWork uow, Guid userID, EntityArtist old, List<EntityArtist> replacementList, DateTime now, MetadataChangeSet changeSet, CancellationToken cancellationToken = default)
    {
        return serviceMetadataChange.ReplaceAlbumArtist(uow, userID, old, replacementList, now, changeSet, cancellationToken);
    }

    public ResponseArtist ToResponse(EntityArtist item, int mediaFileCount)
    {
        return item.ToResponse(mediaFileCount);
    }

    public void AddUpdated(MetadataChangeSet changeSet, ResponseArtist response)
    {
        changeSet.AddUpdated(response);
    }

    public void AddDeleted(MetadataChangeSet changeSet, Guid id)
    {
        changeSet.AddDeletedArtist(id);
    }
}
