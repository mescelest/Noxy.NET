using Mediator;
using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Hubs;

namespace Noxy.NET.MediaPlayer.API.Handlers;

public abstract class HandlerNotification<TNotification>(IHubContext<MediaHub> hub, ILogger logger) : INotificationHandler<TNotification> where TNotification : INotification
{
    // A user with no live connection simply receives nothing: their next start-up diff covers what they missed. Whatever goes wrong with the push, the change it reports has already been saved, so it is logged and never rethrown.
    public async ValueTask Handle(TNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await Push(hub, notification, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not push {Notification}", typeof(TNotification).Name);
        }
    }

    protected abstract Task Push(IHubContext<MediaHub> hub, TNotification notification, CancellationToken cancellationToken);
}
