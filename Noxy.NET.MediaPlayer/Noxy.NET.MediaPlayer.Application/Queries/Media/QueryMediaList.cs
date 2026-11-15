using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Queries.Media;

public record QueryMediaList(Guid UserID, int Skip, int Take) : IQuery<List<ResponseMediaFile>>;
