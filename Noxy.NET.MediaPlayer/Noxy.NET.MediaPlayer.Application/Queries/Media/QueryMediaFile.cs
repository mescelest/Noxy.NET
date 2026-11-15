using Mediator;
using Noxy.NET.MediaPlayer.Application.Models.Media;

namespace Noxy.NET.MediaPlayer.Application.Queries.Media;

public record QueryMediaFile(Guid UserID, Guid ID) : IQuery<MediaDownload>;
