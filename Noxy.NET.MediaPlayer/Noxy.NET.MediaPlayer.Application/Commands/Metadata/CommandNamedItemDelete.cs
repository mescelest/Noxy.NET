using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public abstract record CommandNamedItemDelete(Guid UserID, Guid ID) : ICommand;
