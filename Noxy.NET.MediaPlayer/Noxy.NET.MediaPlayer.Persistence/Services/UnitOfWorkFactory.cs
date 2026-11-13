using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces;

namespace Noxy.NET.MediaPlayer.Persistence.Services;

public class UnitOfWorkFactory(IDbContextFactory<DataContext> contextFactory) : IUnitOfWorkFactory
{
    public IUnitOfWork Create()
    {
        return new UnitOfWork(contextFactory.CreateDbContext());
    }
}
