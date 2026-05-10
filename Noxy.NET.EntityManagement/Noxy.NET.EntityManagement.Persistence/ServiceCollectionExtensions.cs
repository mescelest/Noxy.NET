using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Application.Services;
using Noxy.NET.EntityManagement.Persistence;
using Noxy.NET.EntityManagement.Persistence.Interfaces.Services;
using Noxy.NET.EntityManagement.Persistence.Services;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, Action<DbContextOptionsBuilder> options)
    {
        services.AddDbContextFactory<DataContext>(options);

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
        services.AddScoped<IDependencyInjectionService, DependencyInjectionService>();

        services.AddSingleton<IEntityToTableMapper, EntityToTableMapper>();
        services.AddSingleton<ITableToEntityMapper, TableToEntityMapper>();

        return services;
    }

    public static async Task<IHost> UsePersistence(this IHost app)
    {
        IDbContextFactory<DataContext> factory = app.Services.GetRequiredService<IDbContextFactory<DataContext>>();
        await using DataContext context = await factory.CreateDbContextAsync();
        await context.Database.MigrateAsync();

        return app;
    }
}
