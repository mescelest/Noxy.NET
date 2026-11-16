using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Application.Configurations;

public class ConfigRemote
{
    public const string SectionName = "Remote";

    [Range(0, 3600)]
    public int ActiveGraceSeconds { get; set; } = 10;

    [Range(1, 3600)]
    public int TransferTimeoutSeconds { get; set; } = 5;

    [Range(0.01, 10_080)]
    public double IdleTeardownMinutes { get; set; } = 10;

    [Range(1, 1000)]
    public int MaxDeviceCount { get; set; } = 10;

    [Range(1, 255)]
    public int MaxDeviceNameLength { get; set; } = 50;

    [Range(0, 10_000)]
    public int HistoryLimit { get; set; } = 50;

    [Range(1, 1_000_000)]
    public int MaxQueueLength { get; set; } = 20_000;

    [Range(32_768, 134_217_728)]
    public int MaxHubMessageBytes { get; set; } = 4_194_304;
}
