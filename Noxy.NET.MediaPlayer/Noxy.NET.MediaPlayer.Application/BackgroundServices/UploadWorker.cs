using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces.Processors;
using Noxy.NET.MediaPlayer.Application.Interfaces.Signals;

namespace Noxy.NET.MediaPlayer.Application.BackgroundServices;

public class UploadWorker(IUploadProcessor processor, IUploadSignal signal, IOptions<ConfigUpload> options, ILogger<UploadWorker> logger) : BackgroundService
{
    private static readonly TimeSpan ErrorDelay = TimeSpan.FromSeconds(1);

    // Recovery runs while the host is still starting, before the server takes requests,
    // so it can never delete a file that an upload has just staged.
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await processor.Recover(cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Upload recovery failed");
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TimeSpan poll = TimeSpan.FromSeconds(options.Value.PollSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Work through everything that is pending, then sleep until woken or the poll interval passes.
                if (await processor.ProcessNext(stoppingToken)) continue;

                await signal.WaitAsync(poll, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "The upload worker failed while processing");

                try
                {
                    await Task.Delay(ErrorDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }
}
