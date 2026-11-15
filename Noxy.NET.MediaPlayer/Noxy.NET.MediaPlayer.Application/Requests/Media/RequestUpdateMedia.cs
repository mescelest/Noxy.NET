using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Attributes;
using Noxy.NET.MediaPlayer.Application.Commands.Media;
using Noxy.NET.MediaPlayer.Application.Models;

namespace Noxy.NET.MediaPlayer.Application.Requests.Media;

public class RequestUpdateMedia : IValidatableObject
{
    public const int MaxIDListLength = 100;

    [OptionalNotBlank]
    [OptionalMaxLength(255)]
    public Optional<string?> Title { get; set; }

    [OptionalMaxLength(256)]
    public Optional<string?> Artist { get; set; }

    [OptionalMaxLength(256)]
    public Optional<string?> Album { get; set; }

    [OptionalMaxLength(256)]
    public Optional<string?> AlbumArtist { get; set; }

    [OptionalMaxLength(256)]
    public Optional<string?> Subtitle { get; set; }

    [OptionalMaxLength(2048)]
    public Optional<string?> Comment { get; set; }

    [OptionalMaxLength(128)]
    public Optional<string?> Genre { get; set; }

    [OptionalRange(0, 9999)]
    public Optional<int?> TrackNumber { get; set; }

    [OptionalRange(0, 9999)]
    public Optional<int?> DiscNumber { get; set; }

    [OptionalRange(0, 9999)]
    public Optional<int?> Year { get; set; }

    public Optional<List<Guid>?> ArtistIDList { get; set; }
    public Optional<Guid?> AlbumID { get; set; }
    public Optional<List<Guid>?> AlbumArtistIDList { get; set; }
    public Optional<List<Guid>?> GenreIDList { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        List<ValidationResult> resultList = [];

        AddConflict(resultList, Artist.HasValue, ArtistIDList.HasValue, nameof(Artist), nameof(ArtistIDList));
        AddConflict(resultList, Genre.HasValue, GenreIDList.HasValue, nameof(Genre), nameof(GenreIDList));
        AddConflict(resultList, AlbumArtist.HasValue, AlbumArtistIDList.HasValue, nameof(AlbumArtist), nameof(AlbumArtistIDList));
        AddConflict(resultList, Album.HasValue, AlbumID.HasValue, nameof(Album), nameof(AlbumID));

        // An album chosen by its ID is taken as it is, so the artists of an album named by title cannot be sent with it.
        AddConflict(resultList, AlbumArtist.HasValue, AlbumID.HasValue, nameof(AlbumArtist), nameof(AlbumID));
        AddConflict(resultList, AlbumArtistIDList.HasValue, AlbumID.HasValue, nameof(AlbumArtistIDList), nameof(AlbumID));

        AddTooLong(resultList, ArtistIDList, nameof(ArtistIDList));
        AddTooLong(resultList, AlbumArtistIDList, nameof(AlbumArtistIDList));
        AddTooLong(resultList, GenreIDList, nameof(GenreIDList));

        return resultList;
    }

    public CommandMediaUpdate ToCommand(Guid userID, Guid id)
    {
        return new(userID, id, Title, Artist, Album, AlbumArtist, Subtitle, Comment, Genre, TrackNumber, DiscNumber, Year, ArtistIDList, AlbumID, AlbumArtistIDList, GenreIDList);
    }

    private static void AddConflict(List<ValidationResult> resultList, bool first, bool second, string firstName, string secondName)
    {
        if (first && second) resultList.Add(new($"Send either {firstName} or {secondName}, not both.", [firstName, secondName]));
    }

    private static void AddTooLong(List<ValidationResult> resultList, Optional<List<Guid>?> idList, string name)
    {
        if (idList is { HasValue: true, Value.Count: > MaxIDListLength }) resultList.Add(new($"The field {name} can hold at most {MaxIDListLength} IDs.", [name]));
    }
}
