using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public abstract record CommandNamedItemCreate<TResponse>(Guid UserID, string Name) : ICommand<TResponse>;
