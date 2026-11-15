using Mediator;
using Noxy.NET.MediaPlayer.Application.Models.Media;

namespace Noxy.NET.MediaPlayer.Application.Queries.Media;

public record QueryMediaFileWithTags(Guid UserID, Guid ID) : IQuery<MediaDownload>;
