using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Interfaces.Mappers;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playback;

namespace Noxy.NET.MediaPlayer.Persistence.Repositories;

public class PlaybackRepository(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : IPlaybackRepository
{
    public async Task<EntityPlaybackSession?> FindSession(Guid userID, CancellationToken cancellationToken = default)
    {
        TablePlaybackSession? table = await context.PlaybackSession.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserID == userID, cancellationToken);

        return table is null ? null : mapperT2E.Map(table);
    }

    public void AddSession(EntityPlaybackSession session)
    {
        context.PlaybackSession.Add(mapperE2T.Map(session));
    }

    public void UpdateSession(EntityPlaybackSession session)
    {
        context.PlaybackSession.Update(mapperE2T.Map(session));
    }

    public void RemoveSession(EntityPlaybackSession session)
    {
        context.PlaybackSession.Remove(mapperE2T.Map(session));
    }
}
