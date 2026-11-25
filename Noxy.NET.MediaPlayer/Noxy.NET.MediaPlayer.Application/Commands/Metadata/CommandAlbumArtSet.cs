using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandAlbumArtSet(Guid UserID, Guid ID, byte[] Picture) : ICommand;
