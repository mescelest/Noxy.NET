using System.Reflection;
using Noxy.NET.Interfaces;

namespace Noxy.NET.Services;

public sealed record DebounceOptions(int Delay = 500, bool Update = false, string? Key = null);

public sealed class DebouncerService : IDebouncerService
{
    private readonly Lock _lock = new();

    private readonly Dictionary<string, bool> _running = [];
    private readonly Dictionary<string, TaskCompletionSource> _map = [];
    private readonly Dictionary<string, Func<Task>> _latestCallback = [];

    public void Debounce(Action callback, DebounceOptions? options = null)
    {
        Debounce(AsyncWrap(callback), options);
    }

    public void Debounce(Func<Task> callback, DebounceOptions? options = null)
    {
        options ??= new();
        string key = options.Key ?? GetKey(callback);

        if (options.Update)
        {
            DebounceLatest(callback, key, options.Delay);
        }
        else
        {
            DebounceFirst(callback, key, options.Delay);
        }
    }

    private void DebounceFirst(Func<Task> callback, string key, int delayMs)
    {
        lock (_lock)
        {
            if (!_map.TryGetValue(key, out TaskCompletionSource? tcs))
            {
                tcs = new();
                _map[key] = tcs;
            }

            if (!_running.TryGetValue(key, out bool isRunning) || !isRunning)
            {
                _running[key] = true;
                _ = RunFirst(key, callback, delayMs);
            }
            else
            {
                tcs.TrySetResult();
            }
        }
    }

    private async Task RunFirst(string key, Func<Task> callback, int delayMs)
    {
        while (true)
        {
            TaskCompletionSource local;
            lock (_lock) local = _map[key];

            Task delay = Task.Delay(delayMs);
            Task completed = await Task.WhenAny(delay, local.Task);

            if (completed != delay)
            {
                lock (_lock) _map[key] = new TaskCompletionSource();
                continue;
            }

            lock (_lock)
            {
                _running[key] = false;
                _map[key] = new TaskCompletionSource();
            }

            await callback();
            break;
        }
    }

    private void DebounceLatest(Func<Task> callback, string key, int delayMs)
    {
        lock (_lock)
        {
            _latestCallback[key] = callback;

            if (!_map.TryGetValue(key, out TaskCompletionSource? tcs))
            {
                tcs = new();
                _map[key] = tcs;
            }

            if (!_running.TryGetValue(key, out bool isRunning) || !isRunning)
            {
                _running[key] = true;
                _ = RunLatest(key, delayMs);
            }
            else
            {
                tcs.TrySetResult();
            }
        }
    }

    private async Task RunLatest(string key, int delayMs)
    {
        while (true)
        {
            TaskCompletionSource local;
            lock (_lock) local = _map[key];

            Task delay = Task.Delay(delayMs);
            Task completed = await Task.WhenAny(delay, local.Task);

            if (completed != delay)
            {
                lock (_lock) _map[key] = new TaskCompletionSource();
                continue;
            }

            Func<Task> callback;
            lock (_lock)
            {
                _running[key] = false;
                _map[key] = new TaskCompletionSource();

                callback = _latestCallback[key];
                _latestCallback.Remove(key);
            }

            await callback();
            break;
        }
    }

    private static Func<Task> AsyncWrap(Action action)
    {
        return () =>
        {
            action();
            return Task.CompletedTask;
        };
    }

    private static string GetKey(Func<Task> callback)
    {
        MethodInfo method = callback.Method;
        return $"{method.Module.ModuleVersionId}:{method.MetadataToken}";
    }
}
