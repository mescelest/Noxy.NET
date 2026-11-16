using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Application.Models.Media;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Persistence.Extensions;
using Noxy.NET.MediaPlayer.Persistence.Interfaces.Mappers;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;

namespace Noxy.NET.MediaPlayer.Persistence.Repositories;

public class MediaRepository(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : IMediaRepository
{
    public void AddMediaFile(EntityMediaFile mediaFile)
    {
        context.MediaFile.Add(mapperE2T.Map(mediaFile));
    }

    public async Task<EntityMediaFile?> FindMediaFile(Guid id, Guid userID, CancellationToken cancellationToken = default)
    {
        TableMediaFile? table = await context.MediaFile.AsNoTracking()
            .IncludeLinks()
            .FirstOrDefaultAsync(x => x.ID == id && x.UserID == userID, cancellationToken);

        return table is null ? null : mapperT2E.Map(table);
    }

    public async Task<int> CountMediaFiles(Guid userID, CancellationToken cancellationToken = default)
    {
        return await context.MediaFile.CountAsync(x => x.UserID == userID, cancellationToken);
    }

    public async Task<List<Guid>> GetMediaFileIDListWithLegacyText(CancellationToken cancellationToken = default)
    {
        return await context.MediaFile.AsNoTracking()
            .Where(x => x.Metadata != null && (x.Metadata.Artist != null || x.Metadata.Album != null || x.Metadata.AlbumArtist != null || x.Metadata.Genre != null))
            .OrderBy(x => x.ID)
            .Select(x => x.ID)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EntityMediaFile>> GetMediaFileListByID(List<Guid> idList, CancellationToken cancellationToken = default)
    {
        List<TableMediaFile> list = await context.MediaFile.AsNoTracking()
            .IncludeLinks()
            .Where(x => idList.Contains(x.ID))
            .OrderBy(x => x.ID)
            .ToListAsync(cancellationToken);

        return [.. list.Select(mapperT2E.Map)];
    }

    public async Task<List<Guid>> GetOwnedMediaFileIDList(Guid userID, List<Guid> idList, CancellationToken cancellationToken = default)
    {
        return await context.MediaFile.AsNoTracking()
            .Where(x => x.UserID == userID && idList.Contains(x.ID))
            .Select(x => x.ID)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> SumMediaFileBytes(Guid userID, CancellationToken cancellationToken = default)
    {
        return await context.MediaFile.Where(x => x.UserID == userID).SumAsync(x => x.SizeBytes, cancellationToken);
    }

    public async Task<List<EntityMediaFile>> GetMediaFileList(Guid userID, int skip, int take, CancellationToken cancellationToken = default)
    {
        List<TableMediaFile> list = await context.MediaFile.AsNoTracking()
            .IncludeLinks()
            .Where(x => x.UserID == userID)
            .OrderByDescending(x => x.TimeCreated)
            .ThenByDescending(x => x.ID)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return [.. list.Select(x => mapperT2E.Map(x))];
    }

    public async Task<List<EntityMediaFile>> GetMediaFileList(Guid userID, CancellationToken cancellationToken = default)
    {
        List<TableMediaFile> list = await context.MediaFile.AsNoTracking()
            .IncludeLinks()
            .Where(x => x.UserID == userID)
            .OrderByDescending(x => x.TimeCreated)
            .ThenByDescending(x => x.ID)
            .ToListAsync(cancellationToken);

        return [.. list.Select(mapperT2E.Map)];
    }

    public async Task<List<MediaFileStamp>> GetMediaFileStampList(Guid userID, CancellationToken cancellationToken = default)
    {
        return await context.MediaFile.AsNoTracking()
            .Where(x => x.UserID == userID)
            .Select(x => new MediaFileStamp(x.ID, x.TimeModified))
            .ToListAsync(cancellationToken);
    }

    public void UpdateMediaFile(EntityMediaFile mediaFile)
    {
        context.MediaFile.Update(mapperE2T.Map(mediaFile));
    }

    public void RemoveMediaFile(EntityMediaFile mediaFile)
    {
        context.MediaFile.Remove(mapperE2T.Map(mediaFile));
    }

    public void AddMediaUploadJob(EntityMediaUploadJob job)
    {
        context.MediaUploadJob.Add(mapperE2T.Map(job));
    }

    public void RemoveMediaUploadJob(EntityMediaUploadJob job)
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

    public async Task<EntityMediaUploadJob?> FindNextPendingMediaUploadJob(CancellationToken cancellationToken = default)
    {
        TableMediaUploadJob? table = await context.MediaUploadJob.AsNoTracking()
            .Where(x => x.Status == MediaUploadJobStatusEnum.Pending)
            .OrderBy(x => x.TimeCreated)
            .ThenBy(x => x.ID)
            .FirstOrDefaultAsync(cancellationToken);

        return table is null ? null : mapperT2E.Map(table);
    }

    public async Task<bool> MarkMediaUploadJobProcessing(Guid id, CancellationToken cancellationToken = default)
    {
        int updated = await context.MediaUploadJob
            .Where(x => x.ID == id && x.Status == MediaUploadJobStatusEnum.Pending)
            .ExecuteUpdateAsync(x => x.SetProperty(e => e.Status, MediaUploadJobStatusEnum.Processing), cancellationToken);

        return updated == 1;
    }

    public async Task<int> ResetProcessingMediaUploadJobs(CancellationToken cancellationToken = default)
    {
        return await context.MediaUploadJob
            .Where(x => x.Status == MediaUploadJobStatusEnum.Processing)
            .ExecuteUpdateAsync(x => x.SetProperty(e => e.Status, MediaUploadJobStatusEnum.Pending), cancellationToken);
    }

    public async Task<List<string>> GetMediaUploadJobFileNameList(CancellationToken cancellationToken = default)
    {
        return await context.MediaUploadJob.AsNoTracking().Select(x => x.StagedFileName).ToListAsync(cancellationToken);
    }
}
