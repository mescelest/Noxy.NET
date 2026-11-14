using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
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

    public async Task<int> CountMediaFiles(Guid userID, CancellationToken cancellationToken = default)
    {
        return await context.MediaFile.CountAsync(x => x.UserID == userID, cancellationToken);
    }

    public async Task<long> SumMediaFileBytes(Guid userID, CancellationToken cancellationToken = default)
    {
        return await context.MediaFile.Where(x => x.UserID == userID).SumAsync(x => x.SizeBytes, cancellationToken);
    }

    public async Task<List<MediaFile>> GetMediaFileList(Guid userID, int skip, int take, CancellationToken cancellationToken = default)
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

    public void AddMediaUploadJob(MediaUploadJob job)
    {
        context.MediaUploadJob.Add(mapperE2T.Map(job));
    }

    public void RemoveMediaUploadJob(MediaUploadJob job)
    {
        context.MediaUploadJob.Remove(mapperE2T.Map(job));
    }

    public async Task<int> CountMediaUploadJobs(Guid userID, CancellationToken cancellationToken = default)
    {
        return await context.MediaUploadJob.CountAsync(x => x.UserID == userID, cancellationToken);
    }

    public async Task<long> SumMediaUploadJobBytes(Guid userID, CancellationToken cancellationToken = default)
    {
        return await context.MediaUploadJob.Where(x => x.UserID == userID).SumAsync(x => x.SizeBytes, cancellationToken);
    }

    public async Task<MediaUploadJob?> FindNextPendingMediaUploadJob(CancellationToken cancellationToken = default)
    {
        TableMediaUploadJob? table = await context.MediaUploadJob.AsNoTracking()
            .Where(x => x.Status == MediaUploadJobStatus.Pending)
            .OrderBy(x => x.TimeCreated)
            .ThenBy(x => x.ID)
            .FirstOrDefaultAsync(cancellationToken);

        return table == null ? null : mapperT2E.Map(table);
    }

    public async Task<bool> MarkMediaUploadJobProcessing(Guid id, CancellationToken cancellationToken = default)
    {
        int updated = await context.MediaUploadJob
            .Where(x => x.ID == id && x.Status == MediaUploadJobStatus.Pending)
            .ExecuteUpdateAsync(x => x.SetProperty(e => e.Status, MediaUploadJobStatus.Processing), cancellationToken);

        return updated == 1;
    }

    public async Task<int> ResetProcessingMediaUploadJobs(CancellationToken cancellationToken = default)
    {
        return await context.MediaUploadJob
            .Where(x => x.Status == MediaUploadJobStatus.Processing)
            .ExecuteUpdateAsync(x => x.SetProperty(e => e.Status, MediaUploadJobStatus.Pending), cancellationToken);
    }

    public async Task<List<string>> GetMediaUploadJobFileNameList(CancellationToken cancellationToken = default)
    {
        return await context.MediaUploadJob.AsNoTracking().Select(x => x.StagedFileName).ToListAsync(cancellationToken);
    }
}
