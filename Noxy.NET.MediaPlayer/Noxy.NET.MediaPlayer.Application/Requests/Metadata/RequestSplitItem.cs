using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Requests.Metadata;

public class RequestSplitItem : IValidatableObject
{
    public const int MaxNameCount = 20;
    public const int MaxNameLength = 255;

    [Required]
    [MinLength(2)]
    [MaxLength(MaxNameCount)]
    public List<string> NameList { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NameList.Any(x => string.IsNullOrWhiteSpace(x) || x.Length > MaxNameLength))
        {
            return [new($"Every name in {nameof(NameList)} must be filled in and at most {MaxNameLength} characters.", [nameof(NameList)])];
        }

        return [];
    }

    public CommandArtistSplit ToArtistCommand(Guid userID, Guid id)
    {
        return new(userID, id, NameList);
    }

    public CommandGenreSplit ToGenreCommand(Guid userID, Guid id)
    {
        return new(userID, id, NameList);
    }
}
