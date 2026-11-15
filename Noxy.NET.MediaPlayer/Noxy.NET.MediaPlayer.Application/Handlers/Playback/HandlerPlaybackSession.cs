using Mediator;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Playback;
using Noxy.NET.MediaPlayer.Application.Queries.Playback;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Extensions;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Playback;

public class HandlerPlaybackSession(IUnitOfWorkFactory factoryUnitOfWork, IETagService serviceETag) : IQueryHandler<QueryPlaybackSession, PlaybackSessionView>
{
    public async ValueTask<PlaybackSessionView> Handle(QueryPlaybackSession query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        EntityPlaybackSession? session = await uow.Playback.FindSession(query.UserID, cancellationToken);
        if (session is null) return new(null, null);

        // Every change of the session moves its modified time, so the time alone says whether the caller has it.
        string eTag = serviceETag.Compute([(session.ID, session.TimeModified)]);
        if (!string.IsNullOrWhiteSpace(query.IfNoneMatch) && serviceETag.Matches(query.IfNoneMatch, eTag)) return new(eTag, null);

        return new(eTag, session.ToResponse());
    }
}
