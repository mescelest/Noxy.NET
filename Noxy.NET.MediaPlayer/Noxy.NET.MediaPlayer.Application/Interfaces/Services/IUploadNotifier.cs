using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IUploadNotifier
{
    // Tells the user's connected devices how an upload ended. A user with no live connection is not an error.
    Task Notify(Guid userID, ResponseUploadResult result, CancellationToken cancellationToken = default);
}
