using Noxy.NET.CaseManagement.Application.Interfaces;
using Noxy.NET.CaseManagement.Persistence.Interfaces.Services;

namespace Noxy.NET.CaseManagement.Persistence.Abstractions;

public class BaseRepository(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : IRepository
{
    protected DataContext Context { get; set; } = context;
    protected IEntityToTableMapper MapperE2T { get; set; } = mapperE2T;
    protected ITableToEntityMapper MapperT2E { get; set; } = mapperT2E;
}
