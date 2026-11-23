using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Noxy.NET.EntityManagement.Domain.Interfaces;
using Noxy.NET.EntityManagement.Persistence;
using Noxy.NET.EntityManagement.Persistence.Repositories;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    // Registers the database and the repository. The migrations that create the tables are written for SQLite.
    public static IServiceCollection AddEntityManagementPersistence(this IServiceCollection services, Action<DbContextOptionsBuilder> options)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddDbContext<EntityManagementContext>(options);
        services.AddScoped<IDataRepository, DataRepository>();
        services.AddScoped<IRelationRepository, RelationRepository>();
        services.AddScoped<ITextRepository, TextRepository>();

        return services;
    }

    // Creates the tables, or brings them up to date. Call it once at startup.
    public static async Task MigrateEntityManagementAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        EntityManagementContext context = scope.ServiceProvider.GetRequiredService<EntityManagementContext>();

        await context.Database.MigrateAsync(cancellationToken);
    }
}
