using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class NoOpMediaNotifier : IMediaNotifier
{
    public Task Updated(Guid userID, ResponseMediaFile mediaFile, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task Deleted(Guid userID, Guid id, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
