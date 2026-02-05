using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Persistence.Tables.Authentication;
using Noxy.NET.EntityManagement.Persistence.Tables.Data;
using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    private static List<Func<ModelBuilder, TableSchema?, TableSchema?>> MigrationSeedList { get; } = [];

    public DbSet<TableAuthentication> Authentication { get; set; } = null!;
    public DbSet<TableIdentity> Identity { get; set; } = null!;
    public DbSet<TableUser> User { get; set; } = null!;

    public DbSet<TableDataElement> DataElement { get; set; } = null!;
    public DbSet<TableDataProperty> DataProperty { get; set; } = null!;
    public DbSet<TableDataPropertyBoolean> DataPropertyBoolean { get; set; } = null!;
    public DbSet<TableDataPropertyDateTime> DataPropertyDateTime { get; set; } = null!;
    public DbSet<TableDataPropertyString> DataPropertyString { get; set; } = null!;

    public DbSet<TableDataParameter> DataParameter { get; set; } = null!;
    public DbSet<TableDataParameterStyle> DataStyleParameter { get; set; } = null!;
    public DbSet<TableDataParameterSystem> DataSystemParameter { get; set; } = null!;
    public DbSet<TableDataParameterText> DataTextParameter { get; set; } = null!;

    public DbSet<TableSchema> Schema { get; set; } = null!;
    public DbSet<TableSchemaContext> SchemaContext { get; set; } = null!;
    public DbSet<TableSchemaElement> SchemaElement { get; set; } = null!;
    public DbSet<TableSchemaParameter> SchemaParameter { get; set; } = null!;
    public DbSet<TableSchemaParameterSystem> SchemaParameterSystem { get; set; } = null!;
    public DbSet<TableSchemaParameterStyle> SchemaParameterStyle { get; set; } = null!;
    public DbSet<TableSchemaParameterText> SchemaParameterText { get; set; } = null!;
    public DbSet<TableSchemaProperty> SchemaProperty { get; set; } = null!;
    public DbSet<TableSchemaPropertyBoolean> SchemaPropertyBoolean { get; set; } = null!;
    public DbSet<TableSchemaPropertyCollection> SchemaPropertyCollection { get; set; } = null!;
    public DbSet<TableSchemaPropertyDateTime> SchemaPropertyDateTime { get; set; } = null!;
    public DbSet<TableSchemaPropertyDecimal> SchemaPropertyDecimal { get; set; } = null!;
    public DbSet<TableSchemaPropertyImage> SchemaPropertyImage { get; set; } = null!;
    public DbSet<TableSchemaPropertyInteger> SchemaPropertyInteger { get; set; } = null!;
    public DbSet<TableSchemaPropertyString> SchemaPropertyString { get; set; } = null!;
    public DbSet<TableSchemaPropertyTable> SchemaPropertyTable { get; set; } = null!;

    public DbSet<TableJunctionSchemaContextHasElement> SchemaContextHasElement { get; set; } = null!;
    public DbSet<TableJunctionSchemaElementHasProperty> SchemaElementHasProperty { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TableDataProperty>().UseTpcMappingStrategy();
        modelBuilder.Entity<TableDataProperty>().Property(e => e.ID).ValueGeneratedNever();

        modelBuilder.Entity<TableDataParameter>().UseTpcMappingStrategy();
        modelBuilder.Entity<TableDataParameter>().Property(e => e.ID).ValueGeneratedNever();

        modelBuilder.Entity<TableSchemaProperty>().UseTpcMappingStrategy();
        modelBuilder.Entity<TableSchemaProperty>().Property(e => e.ID).ValueGeneratedNever();

        modelBuilder.Entity<TableSchemaParameter>().UseTpcMappingStrategy();
        modelBuilder.Entity<TableSchemaParameter>().Property(e => e.ID).ValueGeneratedNever();

        modelBuilder.Entity<TableUser>()
            .HasOne(e => e.Authentication)
            .WithOne(e => e.User)
            .HasForeignKey<TableUser>(x => x.AuthenticationID);

        #region -- Schema --

        modelBuilder.Entity<TableSchema>()
            .HasMany(e => e.ContextList)
            .WithOne(e => e.Schema)
            .HasForeignKey(x => x.SchemaID);

        modelBuilder.Entity<TableSchema>()
            .HasMany(e => e.ElementList)
            .WithOne(e => e.Schema)
            .HasForeignKey(x => x.SchemaID);

        modelBuilder.Entity<TableSchema>()
            .HasMany(e => e.PropertyList)
            .WithOne(e => e.Schema)
            .HasForeignKey(x => x.SchemaID);

        modelBuilder.Entity<TableSchema>()
            .HasMany(e => e.PropertyList)
            .WithOne(e => e.Schema)
            .HasForeignKey(x => x.SchemaID);

        #endregion -- Schema --

        #region -- Junctions --

        modelBuilder.Entity<TableJunctionSchemaElementHasElement>()
            .HasOne(e => e.Entity)
            .WithMany(e => e.ElementList)
            .HasForeignKey(x => x.EntityID);

        modelBuilder.Entity<TableJunctionSchemaElementHasElement>()
            .HasOne(e => e.Relation)
            .WithMany(e => e.RelationElementList)
            .HasForeignKey(x => x.RelationID);

        modelBuilder.Entity<TableJunctionSchemaPropertyCollectionHasProperty>()
            .HasOne(e => e.Entity)
            .WithMany(e => e.PropertyList)
            .HasForeignKey(x => x.EntityID);

        modelBuilder.Entity<TableJunctionSchemaPropertyCollectionHasProperty>()
            .HasOne(e => e.Relation)
            .WithMany(e => e.RelationPropertyCollectionList)
            .HasForeignKey(x => x.RelationID);

        modelBuilder.Entity<TableJunctionSchemaPropertyTableHasProperty>()
            .HasOne(e => e.Entity)
            .WithMany(e => e.PropertyList)
            .HasForeignKey(x => x.EntityID);

        modelBuilder.Entity<TableJunctionSchemaPropertyTableHasProperty>()
            .HasOne(e => e.Relation)
            .WithMany(e => e.RelationPropertyTableList)
            .HasForeignKey(x => x.RelationID);

        #endregion -- Junctions --

        TableSchema? tableSchema = null;
        foreach (TableSchema result in MigrationSeedList.Select(action => action(modelBuilder, tableSchema)).OfType<TableSchema>())
        {
            tableSchema = result;
        }
    }

    public static void AddMigrationSeed(Func<ModelBuilder, TableSchema?, TableSchema?> action)
    {
        MigrationSeedList.Add(action);
    }
}
