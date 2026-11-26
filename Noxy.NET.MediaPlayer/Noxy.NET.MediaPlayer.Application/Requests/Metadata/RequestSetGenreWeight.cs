using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;
using Noxy.NET.MediaPlayer.Domain.Declarations;

namespace Noxy.NET.MediaPlayer.Application.Requests.Metadata;

public class RequestSetGenreWeight
{
    [Range(MetadataWeight.Minimum, MetadataWeight.Maximum)]
    public int Weight { get; set; } = MetadataWeight.Default;

    public CommandGenreSetWeight ToCommand(Guid userID, Guid id)
    {
        return new(userID, id, Weight);
    }
}
