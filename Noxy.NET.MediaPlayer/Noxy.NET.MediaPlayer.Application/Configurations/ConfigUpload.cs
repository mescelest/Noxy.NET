using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Application.Configurations;

public class ConfigUpload
{
    public const string SectionName = "Upload";

    // Per user. Bounds how many staged files can wait on disk.
    [Range(1, 100_000)]
    public int MaxPendingPerUser { get; set; } = 100;

    // The worker also wakes on its own at this interval, in case a wake-up signal was missed.
    [Range(1, 3600)]
    public int PollSeconds { get; set; } = 30;
}
