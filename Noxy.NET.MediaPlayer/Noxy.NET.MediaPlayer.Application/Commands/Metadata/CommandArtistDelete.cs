namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandArtistDelete(Guid UserID, Guid ID) : CommandNamedItemDelete(UserID, ID);
