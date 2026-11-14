using Noxy.NET.MediaPlayer.Application.Interfaces.Notifiers;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Notifiers;

public class NoOpUploadNotifier : IUploadNotifier
{
    public Task Notify(Guid userID, ResponseUploadResult result, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
