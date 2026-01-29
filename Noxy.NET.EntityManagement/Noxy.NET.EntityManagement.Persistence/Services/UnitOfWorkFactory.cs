using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;

namespace Noxy.NET.EntityManagement.Persistence.Services;

public sealed class UnitOfWorkFactory(IDbContextFactory<DataContext> factory, IDependencyInjectionService serviceDependencyInjection) : IUnitOfWorkFactory
{
    public async Task<IUnitOfWork> Create()
    {
        return new UnitOfWork(await factory.CreateDbContextAsync(), serviceDependencyInjection);
    }
}
