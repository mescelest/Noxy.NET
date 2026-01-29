using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Persistence.Interfaces.Services;

namespace Noxy.NET.EntityManagement.Persistence.Abstractions;

public class BaseRepository(DataContext context, IDependencyInjectionService serviceDependencyInjection) : IRepository
{
    protected DataContext Context { get; set; } = context;
    protected IDependencyInjectionService DI { get; set; } = serviceDependencyInjection;
    protected IEntityToTableMapper MapperE2T { get; set; } = serviceDependencyInjection.GetService<IEntityToTableMapper>();
    protected ITableToEntityMapper MapperT2E { get; set; } = serviceDependencyInjection.GetService<ITableToEntityMapper>();
}
