using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Presentation.Models.FormModel;

// The limits are the server's (RequestUpdateMedia), so a value the form takes is a value the server takes.
public class FormModelEditMedia
{
    [Required]
    [MaxLength(255)]
    [DisplayName("Title")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(256)]
    [DisplayName("Artist")]
    public string? Artist { get; set; }

    [MaxLength(256)]
    [DisplayName("Album")]
    public string? Album { get; set; }

    [MaxLength(256)]
    [DisplayName("Album artist")]
    public string? AlbumArtist { get; set; }

    [MaxLength(128)]
    [DisplayName("Genre")]
    public string? Genre { get; set; }

    [MaxLength(256)]
    [DisplayName("Subtitle")]
    public string? Subtitle { get; set; }

    [Range(0, 9999)]
    [DisplayName("Year")]
    public int? Year { get; set; }

    [Range(0, 9999)]
    [DisplayName("Track")]
    public int? TrackNumber { get; set; }

    [Range(0, 9999)]
    [DisplayName("Disc")]
    public int? DiscNumber { get; set; }

    [MaxLength(2048)]
    [DisplayName("Comment")]
    public string? Comment { get; set; }

    public static FormModelEditMedia From(ResponseMediaFile media)
    {
        return new()
        {
            Title = media.Title,
            Artist = media.Artist,
            Album = media.Album,
            AlbumArtist = media.AlbumArtist,
            Genre = media.Genre,
            Subtitle = media.Subtitle,
            Year = media.Year,
            TrackNumber = media.TrackNumber,
            DiscNumber = media.DiscNumber,
            Comment = media.Comment
        };
    }
}
