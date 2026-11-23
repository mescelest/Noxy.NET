using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Persistence;
using Noxy.NET.EntityManagement.Persistence.Repositories;

namespace Noxy.NET.Test.EntityManagement.Persistence;

// A real SQLite database in memory, built by the migrations of the library. Each context is new, so what it reads is what is in the database, not what another context has tracked.
internal sealed class TestDatabase : IDisposable
{
    public SqliteConnection Connection { get; } = new("Data Source=:memory:");

    public TestDatabase()
    {
        Connection.Open();

        using EntityManagementContext context = CreateContext();
        context.Database.Migrate();
    }

    public EntityManagementContext CreateContext()
    {
        return new(new DbContextOptionsBuilder<EntityManagementContext>().UseSqlite(Connection).Options);
    }

    public DataRepository CreateRepository(TimeProvider? timeProvider = null)
    {
        return new(CreateContext(), timeProvider ?? TimeProvider.System);
    }

    public void Dispose()
    {
        Connection.Dispose();
    }
}

internal sealed class ManualTimeProvider(DateTime now) : TimeProvider
{
    public DateTime Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => new(DateTime.SpecifyKind(Now, DateTimeKind.Utc));
}
