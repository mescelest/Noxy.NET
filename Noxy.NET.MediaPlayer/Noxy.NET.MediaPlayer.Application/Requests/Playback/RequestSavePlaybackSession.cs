using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Commands.Playback;

namespace Noxy.NET.MediaPlayer.Application.Requests.Playback;

public class RequestSavePlaybackSession
{
    // The longest queue allowed is a setting, so the handler checks the length.
    [Required]
    public List<Guid> QueueMediaFileIDList { get; set; } = [];

    [Range(0, int.MaxValue)]
    public int? CurrentIndex { get; set; }

    [Range(0, double.MaxValue)]
    public double PositionSeconds { get; set; }

    public Guid? PlaylistID { get; set; }

    public CommandPlaybackSessionSave ToCommand(Guid userID)
    {
        return new(userID, QueueMediaFileIDList, CurrentIndex, PositionSeconds, PlaylistID);
    }
}
