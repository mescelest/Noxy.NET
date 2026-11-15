using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Commands.Media;

public record CommandMediaDelete(Guid UserID, Guid ID) : ICommand;
