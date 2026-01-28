using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Noxy.NET.CaseManagement.Application.Interfaces;
using Noxy.NET.CaseManagement.Persistence;
using Noxy.NET.CaseManagement.Persistence.Abstractions;
using Noxy.NET.CaseManagement.Persistence.Interfaces.Services;
using Noxy.NET.CaseManagement.Persistence.Seeds;
using Noxy.NET.CaseManagement.Persistence.Services;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas;

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

        services.AddSingleton<IEntityToTableMapper, EntityToTableMapper>();
        services.AddSingleton<ITableToEntityMapper, TableToEntityMapper>();

        return services;
    }

    public static async Task<IHost> UsePersistence(this IHost app, Func<DataContext, bool, Task>? seedAction = null)
    {
        IDbContextFactory<DataContext> factory = app.Services.GetRequiredService<IDbContextFactory<DataContext>>();
        await using DataContext context = await factory.CreateDbContextAsync();

        IEnumerable<string> migrations = await context.Database.GetAppliedMigrationsAsync();
        await context.Database.MigrateAsync();

        if (seedAction != null)
        {
            await seedAction(context, System.Reflection.Assembly.GetEntryAssembly() != null && !migrations.Any());
        }

        return app;
    }

    public static IServiceCollection AddBaseToPersistence(this IServiceCollection services)
    {
        DataContext.AddMigrationSeed(SeedSchema);
        DataContext.AddMigrationSeed(SeedText);

        return services;
    }

    public static async Task<IHost> UseBaseWithPersistence(this IHost app)
    {
        return await Task.FromResult(app);
    }

    private static TableSchema SeedSchema(ModelBuilder builder, TableSchema? schema)
    {
        schema ??= BaseSeed.CreateSchema("01974e8c-ecb8-75ab-9070-ef902ff370a7", "Base schema", note: "This is a base schema intended to be cloned and extended.", isActive: true, timeActivated: BaseSeed.Now);
        
        builder.Entity<TableSchema>().HasData(schema);
        new SchemaSeed(builder, schema).Apply();
        
        return schema;
    }

    private static TableSchema SeedText(ModelBuilder builder, TableSchema? schema)
    {
        ArgumentNullException.ThrowIfNull(schema);
        new TextSeed(builder, schema).Apply();
        return schema;
    }
}
