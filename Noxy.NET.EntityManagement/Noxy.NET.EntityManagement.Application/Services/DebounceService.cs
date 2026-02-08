using System.Reflection;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;

namespace Noxy.NET.EntityManagement.Application.Services;

public sealed class DebouncerService : IDebouncerService
{
    private readonly Lock _lock = new();
    private readonly Dictionary<string, TaskCompletionSource<bool>> _map = new();
    private readonly Dictionary<string, bool> _running = new();

    public void Debounce(Func<Task> callback, int delayMs = 100)
    {
        string key = GetKey(callback);

        lock (_lock)
        {
            if (!_map.TryGetValue(key, out TaskCompletionSource<bool>? tcs))
            {
                tcs = new();
                _map[key] = tcs;
            }

            if (!_running.TryGetValue(key, out bool isRunning) || !isRunning)
            {
                _running[key] = true;
                _ = Run(key, callback, delayMs);
            }
            else
            {
                tcs.TrySetResult(true);
            }
        }
    }

    private async Task Run(string key, Func<Task> callback, int delayMs)
    {
        while (true)
        {
            TaskCompletionSource<bool> local;

            lock (_lock)
            {
                local = _map[key];
                _map[key] = new();
            }

            Task delay = Task.Delay(delayMs);
            Task completed = await Task.WhenAny(delay, local.Task);
            if (completed != delay) continue;

            await callback();

            lock (_lock)
            {
                _running[key] = false;
            }

            break;
        }
    }

    private static string GetKey(Func<Task> callback)
    {
        MethodInfo method = callback.Method;
        return $"{method.Module.ModuleVersionId}:{method.MetadataToken}";
    }
}
