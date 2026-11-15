using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Queries.Media;

namespace Noxy.NET.MediaPlayer.Application.Requests.Media;

public class RequestListMedia
{
    [Range(0, int.MaxValue)]
    public int Skip { get; set; }

    [Range(1, 100)]
    public int Take { get; set; } = 50;

    public QueryMediaList ToQuery(Guid userID)
    {
        return new(userID, Skip, Take);
    }
}
