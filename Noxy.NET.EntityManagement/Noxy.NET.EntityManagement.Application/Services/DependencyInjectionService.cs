using Microsoft.Extensions.DependencyInjection;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;

namespace Noxy.NET.EntityManagement.Application.Services;

public class DependencyInjectionService(IServiceProvider serviceProvider) : IDependencyInjectionService
{
    public T GetService<T>() where T : notnull
    {
        return serviceProvider.GetService<T>() ?? throw new InvalidOperationException($"Service {typeof(T).Name} not found.");
    }
}
