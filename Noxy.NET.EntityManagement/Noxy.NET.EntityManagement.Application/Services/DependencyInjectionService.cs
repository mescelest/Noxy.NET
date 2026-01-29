using Microsoft.Extensions.DependencyInjection;

namespace Noxy.NET.EntityManagement.Application.Services;

public class DependencyInjectionService(IServiceProvider serviceProvider)
{
    public T GetService<T>() where T : notnull
    {
        return serviceProvider.GetService<T>() ?? throw new InvalidOperationException($"Service {typeof(T).Name} not found.");
    }
}
