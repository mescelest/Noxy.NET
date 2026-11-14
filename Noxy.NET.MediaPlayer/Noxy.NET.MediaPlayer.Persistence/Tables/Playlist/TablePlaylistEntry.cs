using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Persistence.Abstractions.Tables;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;

namespace Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;

[Table("PlaylistEntry")]
[Index(nameof(PlaylistID), nameof(Position))]
[Index(nameof(MediaFileID))]
public class TablePlaylistEntry : BaseTable
{
    [Required]
    public Guid PlaylistID { get; set; }

    [ForeignKey(nameof(PlaylistID))]
    public TablePlaylist? Playlist { get; set; }

    [Required]
    public Guid MediaFileID { get; set; }

    [ForeignKey(nameof(MediaFileID))]
    public TableMediaFile? MediaFile { get; set; }

    [Required]
    public int Position { get; set; }
}
