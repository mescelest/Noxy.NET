using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandAlbumCreate(Guid UserID, string Title, List<Guid> ArtistIDList) : ICommand<ResponseAlbum>;
