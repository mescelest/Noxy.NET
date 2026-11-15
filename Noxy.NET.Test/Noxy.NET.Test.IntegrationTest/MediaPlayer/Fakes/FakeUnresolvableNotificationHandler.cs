using Mediator;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

// A stream is never registered in the container, so this handler can not be constructed.
public sealed class FakeUnresolvableNotificationHandler(Stream unregistered) : INotificationHandler<NotificationMediaUpdated>
{
    public ValueTask Handle(NotificationMediaUpdated notification, CancellationToken cancellationToken)
    {
        unregistered.Flush();
        return ValueTask.CompletedTask;
    }
}
