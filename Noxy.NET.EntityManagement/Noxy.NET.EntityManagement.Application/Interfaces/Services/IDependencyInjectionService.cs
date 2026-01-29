namespace Noxy.NET.EntityManagement.Application.Interfaces.Services;

public interface IDependencyInjectionService
{
    T GetService<T>() where T : notnull;
}
