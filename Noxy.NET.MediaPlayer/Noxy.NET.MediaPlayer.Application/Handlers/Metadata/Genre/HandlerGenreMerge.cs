using Mediator;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.NamedItem;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Genre;

public class HandlerGenreMerge(
    IUnitOfWorkFactory factoryUnitOfWork,
    INamedItemService<EntityGenre, ResponseGenre> serviceGenreItem,
    INameKeyService serviceNameKey,
    IPublisher publisher,
    TimeProvider time) : BaseHandlerNamedItemMerge<CommandGenreMerge, EntityGenre, ResponseGenre>(factoryUnitOfWork, serviceGenreItem, serviceNameKey, publisher, time);
