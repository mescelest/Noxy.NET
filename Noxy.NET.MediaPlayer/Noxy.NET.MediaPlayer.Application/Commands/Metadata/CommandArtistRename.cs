using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandArtistRename(Guid UserID, Guid ID, string Name) : CommandNamedItemRename<ResponseArtist>(UserID, ID, Name);
