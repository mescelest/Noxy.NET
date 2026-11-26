using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public abstract record CommandNamedItemSetWeight<TResponse>(Guid UserID, Guid ID, int Weight) : ICommand<TResponse>;
