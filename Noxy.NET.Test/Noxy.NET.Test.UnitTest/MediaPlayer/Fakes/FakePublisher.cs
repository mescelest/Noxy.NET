using Mediator;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakePublisher : IPublisher
{
    public List<object> PublishedList { get; } = [];
    public List<CancellationToken> TokenList { get; } = [];
    public Action<object>? OnPublish { get; set; }

    public List<T> GetList<T>()
    {
        return [.. PublishedList.OfType<T>()];
    }

    public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        return Record(notification, cancellationToken);
    }

    public ValueTask Publish(object notification, CancellationToken cancellationToken = default)
    {
        return Record(notification, cancellationToken);
    }

    private ValueTask Record(object notification, CancellationToken cancellationToken)
    {
        OnPublish?.Invoke(notification);
        PublishedList.Add(notification);
        TokenList.Add(cancellationToken);
        return ValueTask.CompletedTask;
    }
}
