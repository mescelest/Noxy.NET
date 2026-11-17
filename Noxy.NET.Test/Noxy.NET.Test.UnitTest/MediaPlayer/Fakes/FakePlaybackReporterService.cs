using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakePlaybackReporterService : IPlaybackReporterService
{
    public int StartCount { get; private set; }

    public int ReportNowCount { get; private set; }

    public int ClaimNowCount { get; private set; }

    public int SaveNowCount { get; private set; }

    // What ClaimNow answers.
    public bool? ClaimResult { get; set; } = true;

    // Runs inside ReportNow and ClaimNow, so a test can record what the world looked like at that moment.
    public Action? OnReportNow { get; set; }

    public Action? OnClaimNow { get; set; }

    public void Start()
    {
        StartCount++;
    }

    public Task SaveNow()
    {
        SaveNowCount++;
        return Task.CompletedTask;
    }

    public Task ReportNow()
    {
        ReportNowCount++;
        OnReportNow?.Invoke();

        return Task.CompletedTask;
    }

    public Task<bool?> ClaimNow()
    {
        ClaimNowCount++;
        OnClaimNow?.Invoke();

        return Task.FromResult(ClaimResult);
    }
}
