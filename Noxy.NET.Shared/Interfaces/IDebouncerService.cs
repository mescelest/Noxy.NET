using Noxy.NET.Services;

namespace Noxy.NET.Interfaces;

public interface IDebouncerService
{
    void Debounce(Action callback, DebounceOptions? options = null);
    void Debounce(Func<Task> callback, DebounceOptions? options = null);
}
