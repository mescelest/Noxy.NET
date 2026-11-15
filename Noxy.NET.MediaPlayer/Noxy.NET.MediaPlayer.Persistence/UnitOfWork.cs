using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Persistence.Interfaces.Mappers;
using Noxy.NET.MediaPlayer.Persistence.Repositories;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playback;
using Noxy.NET.MediaPlayer.Persistence.Tables.Setting;

namespace Noxy.NET.MediaPlayer.Persistence;

public sealed class UnitOfWork(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : IUnitOfWork
{
    private const int SqliteConstraintErrorCode = 19;

    public IAuthenticationRepository Authentication { get; } = new AuthenticationRepository(context, mapperE2T, mapperT2E);
    public IMediaRepository Media { get; } = new MediaRepository(context, mapperE2T, mapperT2E);
    public IPlaylistRepository Playlist { get; } = new PlaylistRepository(context, mapperE2T, mapperT2E);
    public IMetadataRepository Metadata { get; } = new MetadataRepository(context, mapperE2T, mapperT2E);
    public ISettingRepository Setting { get; } = new SettingRepository(context, mapperE2T, mapperT2E);
    public IPlaybackRepository Playback { get; } = new PlaybackRepository(context, mapperE2T, mapperT2E);

    public async Task Commit(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsMetadataKeyConflict(exception))
        {
            throw new MetadataKeyConflictException(exception);
        }
    }

    public ValueTask DisposeAsync()
    {
        return context.DisposeAsync();
    }

    // A unique index on an item's per-user key was hit: another request created the same item first.
    private static bool IsMetadataKeyConflict(DbUpdateException exception)
    {
        if (exception.InnerException is not SqliteException { SqliteErrorCode: SqliteConstraintErrorCode } inner) return false;

        return inner.Message.Contains(nameof(EntityArtist.NameKey), StringComparison.Ordinal) || inner.Message.Contains(nameof(EntityAlbum.IdentityKey), StringComparison.Ordinal) ||
               inner.Message.Contains($"{TableUserSetting.TableName}.{nameof(TableUserSetting.Key)}", StringComparison.Ordinal) ||
               inner.Message.Contains($"{TablePlaybackSession.TableName}.{nameof(TablePlaybackSession.UserID)}", StringComparison.Ordinal);
    }
}
