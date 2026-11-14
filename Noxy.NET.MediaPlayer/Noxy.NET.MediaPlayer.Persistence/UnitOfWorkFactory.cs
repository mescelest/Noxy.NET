using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Persistence.Interfaces.Mappers;

namespace Noxy.NET.MediaPlayer.Persistence;

public class UnitOfWorkFactory(IDbContextFactory<DataContext> contextFactory, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : IUnitOfWorkFactory
{
    public IUnitOfWork Create()
    {
        return new UnitOfWork(contextFactory.CreateDbContext(), mapperE2T, mapperT2E);
    }
}
