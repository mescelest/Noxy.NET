using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Persistence.Abstractions.Tables;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

namespace Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;

[Table("Playlist")]
[Index(nameof(UserID))]
public class TablePlaylist : BaseTable
{
    [Required]
    public Guid UserID { get; set; }

    [ForeignKey(nameof(UserID))]
    public TableUser? User { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [Required]
    public bool AllowDuplicates { get; set; }

    [Required]
    public DateTime TimeModified { get; set; }

    public DateTime? TimeLastPlayed { get; set; }


    public List<TablePlaylistEntry> EntryList { get; set; } = [];
}
