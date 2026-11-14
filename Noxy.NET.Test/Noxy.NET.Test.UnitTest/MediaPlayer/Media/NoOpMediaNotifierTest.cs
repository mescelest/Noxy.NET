using Noxy.NET.MediaPlayer.Application.Notifiers;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.Test.MediaPlayer.Media;

public class NoOpMediaNotifierTest
{
    [Fact]
    public async Task Updated_And_Deleted_CompleteWithoutThrowing()
    {
        NoOpMediaNotifier notifier = new();
        ResponseMediaFile file = new()
        {
            ID = Guid.CreateVersion7(),
            Title = "Song",
            ContentType = "audio/mpeg",
            SizeBytes = 1,
            ContentHash = new string('a', 64),
            TimeCreated = DateTime.UtcNow,
            TimeModified = DateTime.UtcNow
        };

        await notifier.Updated(Guid.CreateVersion7(), file, TestContext.Current.CancellationToken);
        await notifier.Deleted(Guid.CreateVersion7(), file.ID, TestContext.Current.CancellationToken);
    }
}
