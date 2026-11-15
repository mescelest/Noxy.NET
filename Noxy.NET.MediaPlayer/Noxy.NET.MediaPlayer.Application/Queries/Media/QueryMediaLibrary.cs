using Mediator;
using Noxy.NET.MediaPlayer.Application.Models.Media;

namespace Noxy.NET.MediaPlayer.Application.Queries.Media;

public record QueryMediaLibrary(Guid UserID, string? IfNoneMatch) : IQuery<MediaLibrary>;
