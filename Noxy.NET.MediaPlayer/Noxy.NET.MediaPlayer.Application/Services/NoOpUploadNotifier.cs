using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class NoOpUploadNotifier : IUploadNotifier
{
    public Task Notify(Guid userID, ResponseUploadResult result, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
