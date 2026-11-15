namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandGenreDelete(Guid UserID, Guid ID) : CommandNamedItemDelete(UserID, ID);
