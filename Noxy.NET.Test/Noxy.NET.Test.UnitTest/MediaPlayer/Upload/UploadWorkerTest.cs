using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.BackgroundServices;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Upload;

public class UploadWorkerTest : IAsyncLifetime
{
    private readonly FakeUploadProcessor _processor = new();
    private readonly FakeUploadSignal _signal = new();
    private readonly UploadWorker _worker;

    public UploadWorkerTest()
    {
        _worker = new(_processor, _signal, Options.Create(new ConfigUpload { PollSeconds = 7 }), NullLogger<UploadWorker>.Instance);
    }

    public ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await _worker.StopAsync(CancellationToken.None);
        _worker.Dispose();
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(10);
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, "The condition was not met in time.");
            await Task.Delay(5, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Worker_RecoversBeforeProcessing()
    {
        await _worker.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntil(() => _signal.WaitCount == 1);

        Assert.Equal(["Recover", "ProcessNext"], _processor.Calls);
    }

    [Fact]
    public async Task StartAsync_DoesNotCompleteUntilRecoveryHas()
    {
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _processor.RecoverBehavior = () => release.Task;

        Task start = _worker.StartAsync(TestContext.Current.CancellationToken);
        await Task.Delay(100, TestContext.Current.CancellationToken);
        Assert.False(start.IsCompleted);
        Assert.Equal(0, _processor.ProcessNextCount);

        release.SetResult();
        await start;
    }

    [Fact]
    public async Task Worker_RecoveryFails_StillProcesses()
    {
        _processor.RecoverBehavior = () => throw new InvalidOperationException("recovery failed");

        await _worker.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntil(() => _signal.WaitCount == 1);

        Assert.Equal(1, _processor.ProcessNextCount);
    }

    [Fact]
    public async Task Worker_DrainsAllPendingBeforeWaiting()
    {
        _processor.ProcessNextBehavior = call => Task.FromResult(call < 3);

        await _worker.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntil(() => _signal.WaitCount == 1);

        Assert.Equal(4, _processor.ProcessNextCount);
    }

    [Fact]
    public async Task Worker_WaitsOnTheSignalWhenIdle_ThenWorksWhenNotified()
    {
        await _worker.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntil(() => _signal.WaitCount == 1);
        Assert.Equal(1, _processor.ProcessNextCount);
        Assert.Equal(TimeSpan.FromSeconds(7), _signal.LastTimeout);

        _signal.Notify();
        await WaitUntil(() => _signal.WaitCount == 2);

        Assert.Equal(2, _processor.ProcessNextCount);
    }

    [Fact]
    public async Task Worker_ProcessorThrows_KeepsRunning()
    {
        _processor.ProcessNextBehavior = call => call == 0 ? throw new InvalidOperationException("boom") : Task.FromResult(false);

        await _worker.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntil(() => _signal.WaitCount == 1);

        Assert.Equal(2, _processor.ProcessNextCount);
    }

    [Fact]
    public async Task Worker_Cancelled_StopsCleanly()
    {
        await _worker.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntil(() => _signal.WaitCount == 1);

        await _worker.StopAsync(TestContext.Current.CancellationToken);

        Assert.True(_worker.ExecuteTask?.IsCompletedSuccessfully);
    }
}
