using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Interfaces.Services;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;

namespace Noxy.NET.MediaPlayer.Persistence.Repositories;

public class MediaRepository(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : IMediaRepository
{
    public void AddMediaFile(MediaFile mediaFile)
    {
        context.MediaFile.Add(mapperE2T.Map(mediaFile));
    }

    public async Task<MediaFile?> FindMediaFile(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        TableMediaFile? table = await context.MediaFile.AsNoTracking()
            .Include(x => x.Metadata)
            .FirstOrDefaultAsync(x => x.ID == id && x.UserID == userID, cancellationToken);

        return table == null ? null : mapperT2E.Map(table);
    }

    public async Task<List<MediaFile>> ListMediaFiles(Guid userID, int skip, int take, CancellationToken cancellationToken = default)
    {
        List<TableMediaFile> list = await context.MediaFile.AsNoTracking()
            .Include(x => x.Metadata)
            .Where(x => x.UserID == userID)
            .OrderByDescending(x => x.TimeCreated)
            .ThenByDescending(x => x.ID)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return [.. list.Select(x => mapperT2E.Map(x))];
    }
}
