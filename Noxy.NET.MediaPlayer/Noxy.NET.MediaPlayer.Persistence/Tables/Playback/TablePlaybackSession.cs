using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Persistence.Abstractions.Tables;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

namespace Noxy.NET.MediaPlayer.Persistence.Tables.Playback;

[Table(TableName)]
[Index(nameof(UserID), IsUnique = true)]
public class TablePlaybackSession : BaseTable
{
    public const string TableName = "PlaybackSession";

    [Required]
    public Guid UserID { get; set; }

    [ForeignKey(nameof(UserID))]
    public TableUser? User { get; set; }

    // Saved as one JSON text: the queue is read and written whole, never searched.
    [Required]
    public List<Guid> QueueMediaFileIDList { get; set; } = [];

    public int? CurrentIndex { get; set; }

    [Required]
    public double PositionSeconds { get; set; }

    public Guid? PlaylistID { get; set; }

    [Required]
    public DateTime TimeModified { get; set; }
}
