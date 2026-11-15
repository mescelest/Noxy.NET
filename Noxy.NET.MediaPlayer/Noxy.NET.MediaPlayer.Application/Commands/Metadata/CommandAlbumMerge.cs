using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandAlbumMerge(Guid UserID, Guid ID, Guid TargetID) : ICommand<ResponseAlbum>;
