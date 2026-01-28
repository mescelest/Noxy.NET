using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Persistence.Interfaces.Services;

namespace Noxy.NET.EntityManagement.Persistence.Abstractions;

public class BaseRepository(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : IRepository
{
    protected DataContext Context { get; set; } = context;
    protected IEntityToTableMapper MapperE2T { get; set; } = mapperE2T;
    protected ITableToEntityMapper MapperT2E { get; set; } = mapperT2E;
}
