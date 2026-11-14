using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeUploadNotifier : IUploadNotifier
{
    public List<(Guid UserID, ResponseUploadResult Result)> NotificationList { get; } = [];
    public Exception? Failure { get; set; }

    public Task Notify(Guid userID, ResponseUploadResult result, CancellationToken cancellationToken = default)
    {
        if (Failure is not null) throw Failure;

        NotificationList.Add((userID, result));
        return Task.CompletedTask;
    }
}
