using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandAlbumDelete(Guid UserID, Guid ID) : ICommand;
