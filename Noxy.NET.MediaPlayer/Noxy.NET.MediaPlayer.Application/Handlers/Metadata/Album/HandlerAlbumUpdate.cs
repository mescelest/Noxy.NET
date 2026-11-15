using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Album;

public class HandlerAlbumUpdate(
    IUnitOfWorkFactory factoryUnitOfWork,
    INameKeyService serviceNameKey,
    IMetadataChangeService serviceMetadataChange,
    IPublisher publisher,
    TimeProvider time) : BaseHandlerAlbumChange<CommandAlbumUpdate, ResponseAlbum>(factoryUnitOfWork, serviceNameKey, serviceMetadataChange, publisher, time)
{
    // Changes the title and the artists of the album, whichever were sent. When the result is the title and artists of another album of the user, that is a NameAlreadyInUseException naming it, unless the settings of the user say to merge on rename: then this album is merged into the other one, which keeps its title and artists.
    public override async ValueTask<ResponseAlbum> Handle(CommandAlbumUpdate command, CancellationToken cancellationToken)
    {
        (string Title, string Key)? requested = command.Title.HasValue ? NormalizeTitle(command.Title.Value ?? string.Empty) : null;

        return await Execute(command.UserID, async uow =>
        {
            EntityAlbum album = await Load(uow, command.UserID, command.ID, cancellationToken);
            List<EntityMediaFile> fileList = await uow.Metadata.GetMediaFileListByAlbum(command.UserID, album.ID, cancellationToken);

            List<EntityArtist> artistList = command.ArtistIDList.HasValue
                ? await LoadArtistList(uow, command.UserID, command.ArtistIDList.Value ?? [], cancellationToken)
                : [.. album.ArtistLinkList.OrderBy(x => x.Position).Select(x => x.Artist ?? throw new InvalidOperationException($"The album {album.ID} was loaded without its artists."))];

            string title = requested?.Title ?? album.Title;
            string titleKey = requested?.Key ?? album.TitleKey;
            string identityKey = EntityAlbum.BuildIdentityKey(titleKey, artistList.Select(x => x.ID));
            bool artistsChanged = !album.ArtistLinkList.OrderBy(x => x.Position).Select(x => x.ArtistID).SequenceEqual(artistList.Select(x => x.ID));

            if (title == album.Title && !artistsChanged) return new MetadataChangeResult<ResponseAlbum>(album.ToResponse(fileList.Count), false, new(), []);

            DateTime now = Now;
            MetadataChangeSet changeSet = new();

            if (identityKey != album.IdentityKey)
            {
                EntityAlbum? other = await uow.Metadata.FindAlbumByIdentityKey(command.UserID, identityKey, cancellationToken);
                if (other is not null && other.ID != album.ID)
                {
                    if (!await IsMergeOnRename(uow, command.UserID, cancellationToken)) throw new NameAlreadyInUseException(other.ID);

                    await ServiceMetadataChange.MergeAlbum(uow, command.UserID, album, other, now, changeSet, cancellationToken);
                    List<ResponseMediaFile> mergedMediaList = TouchChangeSet(uow, changeSet, now);
                    await uow.Commit(cancellationToken);

                    return new MetadataChangeResult<ResponseAlbum>(changeSet.AlbumUpdatedList.Single(x => x.ID == other.ID), true, changeSet, mergedMediaList);
                }
            }

            if (artistsChanged)
            {
                foreach (EntityAlbumArtist removed in album.ArtistLinkList)
                {
                    uow.Metadata.RemoveAlbumArtist(removed);
                }

                album.ArtistLinkList = [.. artistList.Select((x, index) => new EntityAlbumArtist { AlbumID = album.ID, ArtistID = x.ID, Position = index, Artist = x })];
                foreach (EntityAlbumArtist added in album.ArtistLinkList)
                {
                    uow.Metadata.AddAlbumArtist(added);
                }
            }

            album.Title = title;
            album.TitleKey = titleKey;
            album.IdentityKey = identityKey;
            album.TimeModified = now;
            uow.Metadata.UpdateAlbum(album);

            // The album text of every file of the album changes with it.
            foreach (EntityMediaFile file in changeSet.Adopt(fileList))
            {
                file.Album = album;
                changeSet.Touch(file);
            }

            ResponseAlbum response = album.ToResponse(fileList.Count);
            changeSet.AddUpdated(response);

            List<ResponseMediaFile> mediaList = TouchChangeSet(uow, changeSet, now);
            await uow.Commit(cancellationToken);

            return new MetadataChangeResult<ResponseAlbum>(response, true, changeSet, mediaList);
        }, cancellationToken);
    }
}
