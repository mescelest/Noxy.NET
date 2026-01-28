using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Repositories;
using Noxy.NET.EntityManagement.Persistence.Interfaces.Services;
using Noxy.NET.EntityManagement.Persistence.Repositories;

namespace Noxy.NET.EntityManagement.Persistence.Services;

public sealed class UnitOfWork(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : IUnitOfWork
{
    static UnitOfWork()
    {
        Register<AuthenticationRepository>();
        Register<DataRepository>();
        Register<JunctionRepository>();
        Register<SchemaRepository>();
        Register<TemplateRepository>();
    }

    private Dictionary<Type, IRepository> InstanceCollection { get; } = [];

    private static Dictionary<Type, RepositoryInstanceGeneratorFunc> GeneratorCollection { get; } = [];

    public IAuthenticationRepository Authentication => GetRepository<AuthenticationRepository>();
    public IDataRepository Data => GetRepository<DataRepository>();
    public IJunctionRepository Junction => GetRepository<JunctionRepository>();
    public ISchemaRepository Schema => GetRepository<SchemaRepository>();
    public ITemplateRepository Template => GetRepository<TemplateRepository>();

    public async Task Commit(bool useClearTracking = false)
    {
        await context.SaveChangesAsync();
        if (useClearTracking) context.ChangeTracker.Clear();
    }

    public T GetRepository<T>() where T : IRepository
    {
        Type type = typeof(T);
        if (InstanceCollection.TryGetValue(type, out IRepository? result) && result is T instance)
        {
            return instance;
        }

        return (T)(InstanceCollection[type] = InstantiateRepository<T>(context, mapperE2T, mapperT2E));
    }

    public ValueTask DisposeAsync()
    {
        return context.DisposeAsync();
    }

    private static void Register<T>() where T : IRepository
    {
        Type type = typeof(T);
        GeneratorCollection[type] = (context, mapperE2T, mapperT2E) => (T)Activator.CreateInstance(type, context, mapperE2T, mapperT2E)!;
    }

    private static T InstantiateRepository<T>(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E)
    {
        Type type = typeof(T);
        RepositoryInstanceGeneratorFunc generator = GetGeneratorFunc(type);
        return (T)generator(context, mapperE2T, mapperT2E);
    }

    private static RepositoryInstanceGeneratorFunc GetGeneratorFunc(Type type)
    {
        if (GeneratorCollection.TryGetValue(type, out RepositoryInstanceGeneratorFunc? result))
        {
            return result;
        }

        throw new();
    }

    private delegate IRepository RepositoryInstanceGeneratorFunc(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E);
}
