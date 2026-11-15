using Mediator;
using Noxy.NET.MediaPlayer.Application.Models;

namespace Noxy.NET.MediaPlayer.Application.Queries.Media;

public record QueryMediaFileWithTags(Guid UserID, Guid ID) : IQuery<MediaDownload>;
