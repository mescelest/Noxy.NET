using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.NamedItem;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Artist;

public class HandlerArtistCreate(
    IUnitOfWorkFactory factoryUnitOfWork,
    INamedItemService<EntityArtist, ResponseArtist> serviceArtistItem,
    INameKeyService serviceNameKey,
    IPublisher publisher,
    TimeProvider time) : BaseHandlerNamedItemCreate<CommandArtistCreate, EntityArtist, ResponseArtist>(factoryUnitOfWork, serviceArtistItem, serviceNameKey, publisher, time);
