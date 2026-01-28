using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Persistence.Interfaces.Services;

namespace Noxy.NET.EntityManagement.Persistence.Services;

public sealed class UnitOfWorkFactory(IDbContextFactory<DataContext> factory, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : IUnitOfWorkFactory
{
    public async Task<IUnitOfWork> Create()
    {
        return new UnitOfWork(await factory.CreateDbContextAsync(), mapperE2T, mapperT2E);
    }
}
