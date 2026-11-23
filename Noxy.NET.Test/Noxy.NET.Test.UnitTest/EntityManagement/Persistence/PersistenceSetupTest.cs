// These tests run against SQLite in memory and finish at once, so there is nothing to cancel: the repository calls are left on their default token.

#pragma warning disable xUnit1051

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Noxy.NET.EntityManagement.Domain.Interfaces;
using Noxy.NET.EntityManagement.Persistence.Repositories;

namespace Noxy.NET.Test.EntityManagement.Persistence;

public class PersistenceSetupTest
{
    // An in-memory database that lives as long as one connection is open, and can be reached by name from the connections the services open.
    private static SqliteConnection KeepAlive(out string connectionString)
    {
        connectionString = $"Data Source=file:{Guid.NewGuid():N}?mode=memory&cache=shared";

        SqliteConnection connection = new(connectionString);
        connection.Open();
        return connection;
    }

    [Fact]
    public async Task TheSetupMethods_GiveARepository_ThatWorksOnceTheMigrationHasRun()
    {
        using SqliteConnection keepAlive = KeepAlive(out string connectionString);
        ServiceCollection services = new();
        services.AddEntityManagementPersistence(options => options.UseSqlite(connectionString));
        await using ServiceProvider provider = services.BuildServiceProvider();

        await provider.MigrateEntityManagementAsync();

        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            IDataRepository repository = scope.ServiceProvider.GetRequiredService<IDataRepository>();
            Assert.IsType<DataRepository>(repository);
            Assert.IsType<TextRepository>(scope.ServiceProvider.GetRequiredService<ITextRepository>());
            await repository.CreateElement(new() { SchemaIdentifier = "person", CreatedByUserID = Guid.Empty });
        }

        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            Assert.Equal(1, await scope.ServiceProvider.GetRequiredService<IDataRepository>().GetElementCount("person"));
        }
    }

    [Fact]
    public async Task MigratingTwice_ChangesNothing()
    {
        using SqliteConnection keepAlive = KeepAlive(out string connectionString);
        ServiceCollection services = new();
        services.AddEntityManagementPersistence(options => options.UseSqlite(connectionString));
        await using ServiceProvider provider = services.BuildServiceProvider();

        await provider.MigrateEntityManagementAsync();
        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<IDataRepository>().CreateElement(new() { SchemaIdentifier = "person", CreatedByUserID = Guid.Empty });
        }

        await provider.MigrateEntityManagementAsync();

        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            Assert.Equal(1, await scope.ServiceProvider.GetRequiredService<IDataRepository>().GetElementCount("person"));
        }
    }

    [Fact]
    public void TheRepository_IsScoped_ButTheClockIsShared()
    {
        ServiceCollection services = new();
        services.AddEntityManagementPersistence(options => options.UseSqlite("Data Source=:memory:"));
        using ServiceProvider provider = services.BuildServiceProvider();

        using IServiceScope first = provider.CreateScope();
        using IServiceScope second = provider.CreateScope();

        Assert.NotSame(first.ServiceProvider.GetRequiredService<IDataRepository>(), second.ServiceProvider.GetRequiredService<IDataRepository>());
        Assert.Same(first.ServiceProvider.GetRequiredService<TimeProvider>(), second.ServiceProvider.GetRequiredService<TimeProvider>());
    }
}
