using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Attributes;

namespace Noxy.NET.MediaPlayer.Presentation.Models.FormModel;

// The limits are the server's (RequestUpdateMedia), so a value the form takes is a value the server takes.
public class FormModelEditMedia
{
    [Required]
    [MaxLength(255)]
    [DisplayText(nameof(PresentationTexts.FieldTitle))]
    public string Title { get; set; } = string.Empty;

    [MaxLength(256)]
    [DisplayText(nameof(PresentationTexts.FieldArtist))]
    public string? Artist { get; set; }

    [MaxLength(256)]
    [DisplayText(nameof(PresentationTexts.FieldAlbum))]
    public string? Album { get; set; }

    [MaxLength(256)]
    [DisplayText(nameof(PresentationTexts.FieldAlbumArtist))]
    public string? AlbumArtist { get; set; }

    [MaxLength(128)]
    [DisplayText(nameof(PresentationTexts.FieldGenre))]
    public string? Genre { get; set; }

    [MaxLength(256)]
    [DisplayText(nameof(PresentationTexts.FieldSubtitle))]
    public string? Subtitle { get; set; }

    [Range(0, 9999)]
    [DisplayText(nameof(PresentationTexts.FieldYear))]
    public int? Year { get; set; }

    [Range(0, 9999)]
    [DisplayText(nameof(PresentationTexts.FieldTrack))]
    public int? TrackNumber { get; set; }

    [Range(0, 9999)]
    [DisplayText(nameof(PresentationTexts.FieldDisc))]
    public int? DiscNumber { get; set; }

    [MaxLength(2048)]
    [DisplayText(nameof(PresentationTexts.FieldComment))]
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
