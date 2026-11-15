using Noxy.NET.MediaPlayer.Application.Attributes;
using Noxy.NET.MediaPlayer.Application.Commands.Media;
using Noxy.NET.MediaPlayer.Application.Models;

namespace Noxy.NET.MediaPlayer.Application.Requests.Media;

public class RequestUpdateMedia
{
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

    public CommandMediaUpdate ToCommand(Guid userID, Guid id)
    {
        return new(userID, id, Title, Artist, Album, AlbumArtist, Subtitle, Comment, Genre, TrackNumber, DiscNumber, Year);
    }
}
