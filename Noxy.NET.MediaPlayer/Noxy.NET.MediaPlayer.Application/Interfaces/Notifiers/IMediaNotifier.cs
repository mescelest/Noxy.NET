using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Notifiers;

public interface IMediaNotifier
{
    // Tells the user's connected devices a song changed, with its new state. A user with no live connection is not an error.
    Task Updated(Guid userID, ResponseMediaFile mediaFile, CancellationToken cancellationToken = default);

    Task Deleted(Guid userID, Guid id, CancellationToken cancellationToken = default);
}
