using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class CleanupService(IUnitOfWorkFactory uowFactory, IOptions<ConfigCleanup> config, TimeProvider time, ILogger<CleanupService> logger) : BackgroundService
{
    // The schedule: once at startup, then on every tick.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await TryDeleteExpired(stoppingToken);

        using PeriodicTimer timer = new(TimeSpan.FromHours(config.Value.IntervalHours), time);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await TryDeleteExpired(stoppingToken);
        }
    }

    // The work. Can be called on its own, and throws if it fails.
    public async Task DeleteExpired(CancellationToken cancellationToken = default)
    {
        DateTime expiredBefore = time.GetUtcNow().UtcDateTime.AddDays(-config.Value.RetentionDays);

        await using IUnitOfWork uow = uowFactory.Create();
        int deleted = await uow.Authentication.DeleteExpiredRefreshTokens(expiredBefore, cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Refresh token cleanup deleted {Count} expired tokens.", deleted);
        }
    }

    // A failed scheduled run must not take the host down; the next tick tries again.
    private async Task TryDeleteExpired(CancellationToken cancellationToken)
    {
        try
        {
            await DeleteExpired(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Refresh token cleanup failed.");
        }
    }
}
