using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Album;

public class HandlerAlbumCreate(
    IUnitOfWorkFactory factoryUnitOfWork,
    INameKeyService serviceNameKey,
    IMetadataChangeService serviceMetadataChange,
    IPublisher publisher,
    TimeProvider time) : BaseHandlerAlbumChange<CommandAlbumCreate, ResponseAlbum>(factoryUnitOfWork, serviceNameKey, serviceMetadataChange, publisher, time)
{
    // Throws NameAlreadyInUseException, naming the existing album, when the user already has an album with that title and those artists, and MetadataItemNotFoundException for an unknown artist.
    public override async ValueTask<ResponseAlbum> Handle(CommandAlbumCreate command, CancellationToken cancellationToken)
    {
        (string title, string titleKey) = NormalizeTitle(command.Title);

        return await Execute(command.UserID, async uow =>
        {
            List<EntityArtist> artistList = await LoadArtistList(uow, command.UserID, command.ArtistIDList, cancellationToken);
            string identityKey = EntityAlbum.BuildIdentityKey(titleKey, artistList.Select(x => x.ID));

            EntityAlbum? existing = await uow.Metadata.FindAlbumByIdentityKey(command.UserID, identityKey, cancellationToken);
            if (existing is not null) throw new NameAlreadyInUseException(existing.ID);

            EntityAlbum album = new() { UserID = command.UserID, Title = title, TitleKey = titleKey, IdentityKey = identityKey, TimeModified = Now };
            album.ArtistLinkList = [.. artistList.Select((x, index) => new EntityAlbumArtist { AlbumID = album.ID, ArtistID = x.ID, Position = index, Artist = x })];
            uow.Metadata.AddAlbum(album);
            await uow.Commit(cancellationToken);

            MetadataChangeSet changeSet = new();
            ResponseAlbum response = album.ToResponse(0);
            changeSet.AddUpdated(response);

            return new MetadataChangeResult<ResponseAlbum>(response, true, changeSet, []);
        }, cancellationToken);
    }
}
