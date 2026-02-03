namespace Noxy.NET.UI.Services;

public class PageLoadingService
{
    private int _count;
    private TaskCompletionSource _idleTcs = new();
    public bool IsLoading => _count > 0;
    public int PendingOperations => _count;

    public event Action? StateChanged;

    public Task WaitForIdleAsync() => _idleTcs.Task;

    public IDisposable BeginOperation()
    {
        bool wasLoading = IsLoading;
        Interlocked.Increment(ref _count);
        if (wasLoading) return new Operation(this);

        _idleTcs = new();
        StateChanged?.Invoke();

        return new Operation(this);
    }

    public Task WaitForIdleExcept(IDisposable scope)
    {
        TaskCompletionSource tcs = new();

        StateChanged += Handler;
        if (PendingOperations == 1) tcs.TrySetResult();
        return tcs.Task;

        void Handler()
        {
            if (PendingOperations != 1) return;
            StateChanged -= Handler;
            tcs.TrySetResult();
        }
    }

    private void EndOperation()
    {
        bool wasLoading = IsLoading;
        if (Interlocked.Decrement(ref _count) != 0 || !wasLoading) return;

        StateChanged?.Invoke();
        _idleTcs.TrySetResult();
    }

    private class Operation(PageLoadingService coordinator) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            coordinator.EndOperation();
        }
    }
}
