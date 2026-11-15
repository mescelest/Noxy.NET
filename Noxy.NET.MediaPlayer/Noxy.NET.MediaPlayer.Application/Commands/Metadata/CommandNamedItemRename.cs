using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public abstract record CommandNamedItemRename<TResponse>(Guid UserID, Guid ID, string Name) : ICommand<TResponse>;
