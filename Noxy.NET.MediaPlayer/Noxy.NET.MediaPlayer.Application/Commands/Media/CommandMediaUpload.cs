using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Commands.Media;

// The caller owns Content and disposes it: the handler only reads it.
public record CommandMediaUpload(Guid UserID, string FileName, string? Title, Stream Content) : ICommand<ResponseUploadAccepted>;
