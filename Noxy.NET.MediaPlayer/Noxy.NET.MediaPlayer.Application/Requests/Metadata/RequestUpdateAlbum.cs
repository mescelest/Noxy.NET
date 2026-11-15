using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Attributes;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Application.Models;

namespace Noxy.NET.MediaPlayer.Application.Requests.Metadata;

public class RequestUpdateAlbum : IValidatableObject
{
    [OptionalNotBlank]
    [OptionalMaxLength(255)]
    public Optional<string?> Title { get; set; }

    public Optional<List<Guid>?> ArtistIDList { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ArtistIDList is { HasValue: true, Value.Count: > RequestCreateAlbum.MaxArtistCount })
        {
            return [new($"The field {nameof(ArtistIDList)} can hold at most {RequestCreateAlbum.MaxArtistCount} IDs.", [nameof(ArtistIDList)])];
        }

        return [];
    }

    public CommandAlbumUpdate ToCommand(Guid userID, Guid id)
    {
        return new(userID, id, Title, ArtistIDList);
    }
}
