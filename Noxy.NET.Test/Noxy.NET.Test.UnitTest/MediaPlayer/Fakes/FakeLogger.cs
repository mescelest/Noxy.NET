using Microsoft.Extensions.Logging;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeLogger<T> : ILogger<T>
{
    private readonly Lock _lock = new();

    public List<(LogLevel Level, string Message)> EntryList { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        lock (_lock) EntryList.Add((logLevel, formatter(state, exception)));
    }
}
