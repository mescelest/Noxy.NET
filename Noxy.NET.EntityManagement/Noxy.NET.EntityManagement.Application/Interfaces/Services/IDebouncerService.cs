namespace Noxy.NET.EntityManagement.Application.Interfaces.Services;

public interface IDebouncerService
{
    void Debounce(Func<Task> callback, string? key = null, int delayMs = 500);
}
