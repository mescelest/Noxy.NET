using Mediator;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandAlbumUpdate(Guid UserID, Guid ID, Optional<string?> Title = default, Optional<List<Guid>?> ArtistIDList = default) : ICommand<ResponseAlbum>;
