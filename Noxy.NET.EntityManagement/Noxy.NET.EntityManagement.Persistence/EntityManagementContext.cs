using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Domain.Entities;
using Noxy.NET.EntityManagement.Persistence.Configurations;

namespace Noxy.NET.EntityManagement.Persistence;

public class EntityManagementContext(DbContextOptions<EntityManagementContext> options) : DbContext(options)
{
    public DbSet<DataElement> DataElement { get; set; } = null!;
    public DbSet<HistoryElement> HistoryElement { get; set; } = null!;

    // Every property of every type, as a single set. Each type is its own table.
    public DbSet<DataProperty> DataProperty { get; set; } = null!;
    public DbSet<HistoryProperty> HistoryProperty { get; set; } = null!;

    public DbSet<DataRelation> DataRelation { get; set; } = null!;
    public DbSet<HistoryRelation> HistoryRelation { get; set; } = null!;

    public DbSet<DataTextParameter> DataTextParameter { get; set; } = null!;
    public DbSet<HistoryTextParameter> HistoryTextParameter { get; set; } = null!;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new DataElementConfiguration());
        builder.ApplyConfiguration(new HistoryElementConfiguration());
        DataPropertyConfiguration.Apply(builder);
        builder.ApplyConfiguration(new DataRelationConfiguration());
        builder.ApplyConfiguration(new HistoryRelationConfiguration());
        builder.ApplyConfiguration(new DataTextParameterConfiguration());
        builder.ApplyConfiguration(new HistoryTextParameterConfiguration());
    }
}
