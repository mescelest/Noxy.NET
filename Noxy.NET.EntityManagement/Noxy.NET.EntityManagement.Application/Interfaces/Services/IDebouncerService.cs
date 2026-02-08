namespace Noxy.NET.EntityManagement.Application.Interfaces.Services;

public interface IDebouncerService
{
    void Debounce(Func<Task> callback, int delayMs = 100);
}
