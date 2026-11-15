using Mediator;
using Noxy.NET.MediaPlayer.Application.Models;

namespace Noxy.NET.MediaPlayer.Application.Queries.Media;

public record QueryMediaLibrary(Guid UserID, string? IfNoneMatch) : IQuery<MediaLibrary>;
