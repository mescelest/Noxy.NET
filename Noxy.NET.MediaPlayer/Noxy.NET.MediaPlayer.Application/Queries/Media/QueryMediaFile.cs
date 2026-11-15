using Mediator;
using Noxy.NET.MediaPlayer.Application.Models;

namespace Noxy.NET.MediaPlayer.Application.Queries.Media;

public record QueryMediaFile(Guid UserID, Guid ID) : IQuery<MediaDownload>;
