using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public abstract record CommandNamedItemMerge<TResponse>(Guid UserID, Guid ID, Guid TargetID) : ICommand<TResponse>;
