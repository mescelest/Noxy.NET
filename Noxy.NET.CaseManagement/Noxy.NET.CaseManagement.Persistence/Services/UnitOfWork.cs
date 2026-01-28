using Noxy.NET.CaseManagement.Application.Interfaces;
using Noxy.NET.CaseManagement.Application.Interfaces.Repositories;
using Noxy.NET.CaseManagement.Persistence.Interfaces.Services;
using Noxy.NET.CaseManagement.Persistence.Repositories;

namespace Noxy.NET.CaseManagement.Persistence.Services;

public sealed class UnitOfWork(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : IUnitOfWork
{
    private Dictionary<Type, IRepository> InstanceCollection { get; } = [];

    private static Dictionary<Type, RepositoryInstanceGeneratorFunc> GeneratorCollection { get; } = [];

    private delegate IRepository RepositoryInstanceGeneratorFunc(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E);

    public IAuthenticationRepository Authentication => GetRepository<AuthenticationRepository>();
    public IAssociationRepository Association => GetRepository<AssociationRepository>();
    public IDataRepository Data => GetRepository<DataRepository>();
    public IJunctionRepository Junction => GetRepository<JunctionRepository>();
    public ISchemaRepository Schema => GetRepository<SchemaRepository>();
    public ITemplateRepository Template => GetRepository<TemplateRepository>();

    static UnitOfWork()
    {
        Register<AssociationRepository>();
        Register<AuthenticationRepository>();
        Register<DataRepository>();
        Register<JunctionRepository>();
        Register<SchemaRepository>();
        Register<TemplateRepository>();
    }

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

    public ValueTask DisposeAsync()
    {
        return context.DisposeAsync();
    }
}
