using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Repositories;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Persistence.Repositories;

namespace Noxy.NET.EntityManagement.Persistence.Services;

public sealed class UnitOfWork(DataContext context, IDependencyInjectionService serviceDependencyInjection) : IUnitOfWork
{
    static UnitOfWork()
    {
        foreach (Type type in typeof(UnitOfWork).Assembly.GetTypes().Where(t => t is { IsAbstract: false, IsInterface: false } && typeof(IRepository).IsAssignableFrom(t)))
        {
            GeneratorCollection[type] = (context, di) => (IRepository)Activator.CreateInstance(type, context, di)!;
        }
    }

    private DataContext Context { get; } = context;
    private IDependencyInjectionService DI { get; } = serviceDependencyInjection;

    private Dictionary<Type, IRepository> InstanceCollection { get; } = [];

    private static Dictionary<Type, RepositoryInstanceGeneratorFunc> GeneratorCollection { get; } = [];
    public IAuthenticationRepository Authentication => GetRepository<AuthenticationRepository>();
    public IDataRepository Data => GetRepository<DataRepository>();
    public IJunctionRepository Junction => GetRepository<JunctionRepository>();
    public ISchemaRepository Schema => GetRepository<SchemaRepository>();
    public ITemplateRepository Template => GetRepository<TemplateRepository>();

    public async Task Commit(bool useClearTracking = false)
    {
        await Context.SaveChangesAsync();
        if (useClearTracking) Context.ChangeTracker.Clear();
    }

    public T GetRepository<T>() where T : IRepository
    {
        Type type = typeof(T);
        if (!InstanceCollection.TryGetValue(type, out IRepository? result))
        {
            result = InstanceCollection[type] = InstantiateRepository<T>(Context, DI);
        }

        return (T)result;
    }

    public ValueTask DisposeAsync()
    {
        return Context.DisposeAsync();
    }

    private static T InstantiateRepository<T>(DataContext context, IDependencyInjectionService serviceDependencyInjection)
    {
        Type type = typeof(T);
        if (!GeneratorCollection.TryGetValue(type, out RepositoryInstanceGeneratorFunc? generator))
        {
            throw new InvalidOperationException($"No generator registered for repository type {type.Name}");
        }

        return (T)generator(context, serviceDependencyInjection);
    }

    private delegate IRepository RepositoryInstanceGeneratorFunc(DataContext context, IDependencyInjectionService serviceDependencyInjection);
}
