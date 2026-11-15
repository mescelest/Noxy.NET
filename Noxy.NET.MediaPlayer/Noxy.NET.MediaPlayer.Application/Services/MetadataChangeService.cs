using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Extensions;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class MetadataChangeService : IMetadataChangeService
{
    public async Task ReplaceAlbumArtist(IUnitOfWork uow, Guid userID, EntityArtist old, List<EntityArtist> replacementList, DateTime now, MetadataChangeSet changeSet, CancellationToken cancellationToken = default)
    {
        foreach (EntityAlbum album in await uow.Metadata.GetAlbumListByArtist(userID, old.ID, cancellationToken))
        {
            List<EntityArtist> desiredList = [];
            foreach (EntityAlbumArtist link in album.ArtistLinkList.OrderBy(x => x.Position))
            {
                if (link.ArtistID == old.ID)
                {
                    foreach (EntityArtist replacement in replacementList.Where(x => desiredList.All(d => d.ID != x.ID)))
                    {
                        desiredList.Add(replacement);
                    }

                    continue;
                }

                EntityArtist artist = link.Artist ?? throw new InvalidOperationException($"The album {album.ID} was loaded without its artists.");
                if (desiredList.All(x => x.ID != artist.ID)) desiredList.Add(artist);
            }

            if (album.ArtistLinkList.OrderBy(x => x.Position).Select(x => x.ArtistID).SequenceEqual(desiredList.Select(x => x.ID))) continue;

            string identityKey = EntityAlbum.BuildIdentityKey(album.TitleKey, desiredList.Select(x => x.ID));
            EntityAlbum? existing = await uow.Metadata.FindAlbumByIdentityKey(userID, identityKey, cancellationToken);
            if (existing is not null && existing.ID != album.ID)
            {
                await MergeAlbum(uow, userID, album, existing, now, changeSet, cancellationToken);
                continue;
            }

            foreach (EntityAlbumArtist removed in album.ArtistLinkList)
            {
                uow.Metadata.RemoveAlbumArtist(removed);
            }

            album.ArtistLinkList = [.. desiredList.Select((x, index) => new EntityAlbumArtist { AlbumID = album.ID, ArtistID = x.ID, Position = index, Artist = x })];
            foreach (EntityAlbumArtist added in album.ArtistLinkList)
            {
                uow.Metadata.AddAlbumArtist(added);
            }

            album.IdentityKey = identityKey;
            album.TimeModified = now;
            uow.Metadata.UpdateAlbum(album);

            // The album artist text of its files changed with it.
            List<EntityMediaFile> fileList = changeSet.Adopt(await uow.Metadata.GetMediaFileListByAlbum(userID, album.ID, cancellationToken));
            foreach (EntityMediaFile file in fileList)
            {
                file.Album = album;
                changeSet.Touch(file);
            }

            changeSet.AddUpdated(album.ToResponse(fileList.Count));
        }
    }

    public async Task MergeAlbum(IUnitOfWork uow, Guid userID, EntityAlbum source, EntityAlbum target, DateTime now, MetadataChangeSet changeSet, CancellationToken cancellationToken = default)
    {
        List<EntityMediaFile> sourceFileList = changeSet.Adopt(await uow.Metadata.GetMediaFileListByAlbum(userID, source.ID, cancellationToken));
        int targetFileCount = (await uow.Metadata.GetMediaFileListByAlbum(userID, target.ID, cancellationToken)).Count;

        foreach (EntityMediaFile file in sourceFileList)
        {
            file.AlbumID = target.ID;
            file.Album = target;
            changeSet.Touch(file);
        }

        uow.Metadata.RemoveAlbum(source);

        changeSet.AddDeletedAlbum(source.ID);
        changeSet.AddUpdated(target.ToResponse(targetFileCount + sourceFileList.Count));
    }
}
