using System.Diagnostics;
using Noxy.NET.MediaPlayer.Application.Models.Upload;

namespace Noxy.NET.Test.MediaPlayer.Upload;

public class UploadSignalTest
{
    private static readonly TimeSpan Short = TimeSpan.FromMilliseconds(80);

    private readonly UploadSignal _signal = new();

    [Fact]
    public async Task WaitAsync_ReturnsImmediatelyWhenNotifiedBefore()
    {
        _signal.Notify();
        Stopwatch watch = Stopwatch.StartNew();

        await _signal.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        Assert.True(watch.Elapsed < TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task WaitAsync_TimesOutWithoutANotify()
    {
        Stopwatch watch = Stopwatch.StartNew();

        await _signal.WaitAsync(Short, TestContext.Current.CancellationToken);

        Assert.True(watch.Elapsed >= Short - TimeSpan.FromMilliseconds(30));
    }

    [Fact]
    public async Task Notify_ManyTimes_ReleasesOnlyOneWait()
    {
        _signal.Notify();
        _signal.Notify();
        _signal.Notify();
        await _signal.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);
        Stopwatch watch = Stopwatch.StartNew();

        await _signal.WaitAsync(Short, TestContext.Current.CancellationToken);

        Assert.True(watch.Elapsed >= Short - TimeSpan.FromMilliseconds(30));
    }

    [Fact]
    public async Task WaitAsync_Cancelled_Throws()
    {
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _signal.WaitAsync(TimeSpan.FromSeconds(30), cts.Token));
    }
}
