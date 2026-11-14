using Noxy.NET.MediaPlayer.Application.Interfaces.Notifiers;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeMediaNotifier : IMediaNotifier
{
    public List<(Guid UserID, ResponseMediaFile MediaFile)> UpdatedList { get; } = [];
    public List<(Guid UserID, Guid ID)> DeletedList { get; } = [];
    public Exception? Failure { get; set; }

    public Task Updated(Guid userID, ResponseMediaFile mediaFile, CancellationToken cancellationToken = default)
    {
        if (Failure is not null) throw Failure;

        UpdatedList.Add((userID, mediaFile));
        return Task.CompletedTask;
    }

    public Task Deleted(Guid userID, Guid id, CancellationToken cancellationToken = default)
    {
        if (Failure is not null) throw Failure;

        DeletedList.Add((userID, id));
        return Task.CompletedTask;
    }
}
