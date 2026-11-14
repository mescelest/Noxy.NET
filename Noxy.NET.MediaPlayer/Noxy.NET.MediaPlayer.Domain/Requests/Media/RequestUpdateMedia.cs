using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Domain.Requests.Media;

public class RequestUpdateMedia : IValidatableObject
{
    public Optional<string?> Title { get; set; }
    public Optional<string?> Artist { get; set; }
    public Optional<string?> Album { get; set; }
    public Optional<string?> AlbumArtist { get; set; }
    public Optional<string?> Subtitle { get; set; }
    public Optional<string?> Comment { get; set; }
    public Optional<string?> Genre { get; set; }
    public Optional<int?> TrackNumber { get; set; }
    public Optional<int?> DiscNumber { get; set; }
    public Optional<int?> Year { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Title.HasValue)
        {
            if (string.IsNullOrWhiteSpace(Title.Value)) yield return Problem("The title can't be empty.", nameof(Title));
            else if (Title.Value.Length > 255) yield return Problem("The title can be at most 255 characters.", nameof(Title));
        }

        foreach (ValidationResult result in CheckLength(Artist, 256, nameof(Artist))) yield return result;
        foreach (ValidationResult result in CheckLength(Album, 256, nameof(Album))) yield return result;
        foreach (ValidationResult result in CheckLength(AlbumArtist, 256, nameof(AlbumArtist))) yield return result;
        foreach (ValidationResult result in CheckLength(Subtitle, 256, nameof(Subtitle))) yield return result;
        foreach (ValidationResult result in CheckLength(Genre, 128, nameof(Genre))) yield return result;
        foreach (ValidationResult result in CheckLength(Comment, 2048, nameof(Comment))) yield return result;

        foreach (ValidationResult result in CheckRange(TrackNumber, nameof(TrackNumber))) yield return result;
        foreach (ValidationResult result in CheckRange(DiscNumber, nameof(DiscNumber))) yield return result;
        foreach (ValidationResult result in CheckRange(Year, nameof(Year))) yield return result;
    }

    private static IEnumerable<ValidationResult> CheckLength(Optional<string?> value, int maxLength, string member)
    {
        if (value.HasValue && value.Value?.Length > maxLength) yield return Problem($"{member} can be at most {maxLength} characters.", member);
    }

    private static IEnumerable<ValidationResult> CheckRange(Optional<int?> value, string member)
    {
        if (value.HasValue && value.Value is < 0 or > 9999) yield return Problem($"{member} must be between 0 and 9999.", member);
    }

    private static ValidationResult Problem(string message, string member)
    {
        return new(message, [member]);
    }
}
