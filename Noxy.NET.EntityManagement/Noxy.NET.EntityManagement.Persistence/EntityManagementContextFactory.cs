using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Noxy.NET.EntityManagement.Persistence;

// Only used by the dotnet ef tool, to generate migrations without a startup project.
internal sealed class EntityManagementContextFactory : IDesignTimeDbContextFactory<EntityManagementContext>
{
    public EntityManagementContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<EntityManagementContext> options = new();
        options.UseSqlite("Data Source=design-time.sqlite");

        return new(options.Options);
    }
}
