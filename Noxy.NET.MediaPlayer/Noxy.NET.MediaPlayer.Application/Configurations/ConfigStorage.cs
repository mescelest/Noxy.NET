using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Application.Configurations;

public class ConfigStorage : IValidatableObject
{
    public const string SectionName = "Storage";

    // One plain folder name: it starts with a letter or digit, so it cannot be "." or "..", and has no separator, so it cannot leave the root.
    private const string FolderNamePattern = "^[A-Za-z0-9][A-Za-z0-9._-]*$";

    // Absolute, or relative to the working directory like the database path.
    [Required]
    public string RootPath { get; set; } = string.Empty;

    // The folders inside the root. The defaults are also in appsettings.json.

    // Where the media files of the library are kept.
    [Required]
    [RegularExpression(FolderNamePattern)]
    public string LibraryFolderName { get; set; } = "library";

    // Where an uploaded file waits until its upload job has been processed.
    [Required]
    [RegularExpression(FolderNamePattern)]
    public string StagingFolderName { get; set; } = "staging";

    // Copies that live as long as one read or download. Whatever is in it when the host starts is deleted, so it must be a folder of its own.
    [Required]
    [RegularExpression(FolderNamePattern)]
    public string TemporaryFolderName { get; set; } = "temporary";

    // Where the album thumbnails are kept.
    [Required]
    [RegularExpression(FolderNamePattern)]
    public string AlbumArtFolderName { get; set; } = "albumart";

    [Range(1, long.MaxValue)]
    public long MaxSizeBytes { get; set; } = 100L * 1024 * 1024;

    // Listed in configuration only, because the binder appends to a default array instead of replacing it.
    [MinLength(1)]
    public string[] AllowedExtensionList { get; set; } = [];

    // Per user. Keeps the library list that every device downloads at a known size.
    [Range(1, int.MaxValue)]
    public int MaxFilesPerUser { get; set; } = 5000;

    // Per user. Zero means no limit.
    [Range(0, long.MaxValue)]
    public long MaxBytesPerUser { get; set; }

    // Two names for one folder would mix what each is for, and the temporary folder is emptied at every start.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        string[] nameList = [LibraryFolderName, StagingFolderName, TemporaryFolderName, AlbumArtFolderName];
        if (nameList.Distinct(StringComparer.OrdinalIgnoreCase).Count() != nameList.Length)
        {
            yield return new("The library, staging, temporary and album art folders must be four different folders.", [nameof(LibraryFolderName), nameof(StagingFolderName), nameof(TemporaryFolderName), nameof(AlbumArtFolderName)]);
        }
    }
}
