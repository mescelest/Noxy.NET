using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandArtistCreate(Guid UserID, string Name) : CommandNamedItemCreate<ResponseArtist>(UserID, Name);
