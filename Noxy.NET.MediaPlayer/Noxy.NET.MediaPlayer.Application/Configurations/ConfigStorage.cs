using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Application.Configurations;

public class ConfigStorage
{
    public const string SectionName = "Storage";

    // Absolute, or relative to the working directory like the database path.
    [Required]
    public string RootPath { get; set; } = string.Empty;

    [Range(1, long.MaxValue)]
    public long MaxSizeBytes { get; set; } = 100L * 1024 * 1024;

    // Listed in configuration only, because the binder appends to a default array instead of replacing it.
    [MinLength(1)]
    public string[] AllowedExtensions { get; set; } = [];
}
