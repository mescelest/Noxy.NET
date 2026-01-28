using Microsoft.EntityFrameworkCore;
using Noxy.NET.CaseManagement.Persistence.Tables.Authentication;
using Noxy.NET.CaseManagement.Persistence.Tables.Data;
using Noxy.NET.CaseManagement.Persistence.Tables.Data.Discriminators;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Associations;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Persistence;

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

    public DbSet<TableDataSystemParameter> DataSystemParameter { get; set; } = null!;
    public DbSet<TableDataTextParameter> DataTextParameter { get; set; } = null!;

    public DbSet<TableSchema> Schema { get; set; } = null!;
    public DbSet<TableSchemaAction> SchemaAction { get; set; } = null!;
    public DbSet<TableSchemaActionInput> SchemaActionInput { get; set; } = null!;
    public DbSet<TableSchemaActionStep> SchemaActionStep { get; set; } = null!;
    public DbSet<TableSchemaAttribute> SchemaAttribute { get; set; } = null!;
    public DbSet<TableSchemaContext> SchemaContext { get; set; } = null!;
    public DbSet<TableSchemaDynamicValue> SchemaDynamicValue { get; set; } = null!;
    public DbSet<TableSchemaDynamicValueCode> SchemaDynamicValueCode { get; set; } = null!;
    public DbSet<TableSchemaDynamicValueSystemParameter> SchemaDynamicValueSystemParameter { get; set; } = null!;
    public DbSet<TableSchemaDynamicValueStyleParameter> SchemaDynamicValueStyleParameter { get; set; } = null!;
    public DbSet<TableSchemaDynamicValueTextParameter> SchemaDynamicValueTextParameter { get; set; } = null!;
    public DbSet<TableSchemaElement> SchemaElement { get; set; } = null!;
    public DbSet<TableSchemaInput> SchemaInput { get; set; } = null!;
    public DbSet<TableSchemaProperty> SchemaProperty { get; set; } = null!;
    public DbSet<TableSchemaPropertyBoolean> SchemaPropertyBoolean { get; set; } = null!;
    public DbSet<TableSchemaPropertyDateTime> SchemaPropertyDateTime { get; set; } = null!;
    public DbSet<TableSchemaPropertyString> SchemaPropertyString { get; set; } = null!;

    public DbSet<TableJunctionSchemaActionHasActionStep> SchemaActionHasActionStep { get; set; } = null!;
    public DbSet<TableJunctionSchemaActionHasDynamicValueCode> SchemaActionHasDynamicValueCode { get; set; } = null!;
    public DbSet<TableJunctionSchemaActionStepHasActionInput> SchemaActionStepHasActionInput { get; set; } = null!;
    public DbSet<TableAssociationSchemaActionInputHasAttribute> SchemaActionInputHasAttribute { get; set; } = null!;
    public DbSet<TableAssociationSchemaActionInputHasAttributeDynamicValue> SchemaActionInputHasAttributeDynamicValue { get; set; } = null!;
    public DbSet<TableAssociationSchemaActionInputHasAttributeInteger> SchemaActionInputHasAttributeInteger { get; set; } = null!;
    public DbSet<TableAssociationSchemaActionInputHasAttributeString> SchemaActionInputHasAttributeString { get; set; } = null!;
    public DbSet<TableJunctionSchemaContextHasAction> SchemaContextHasAction { get; set; } = null!;
    public DbSet<TableJunctionSchemaContextHasElement> SchemaContextHasElement { get; set; } = null!;
    public DbSet<TableJunctionSchemaElementHasAction> SchemaElementHasAction { get; set; } = null!;
    public DbSet<TableJunctionSchemaElementHasProperty> SchemaElementHasProperty { get; set; } = null!;
    public DbSet<TableJunctionSchemaInputHasAttribute> SchemaInputHasAttribute { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TableDataProperty>().UseTpcMappingStrategy();
        modelBuilder.Entity<TableDataProperty>().Property(e => e.ID).ValueGeneratedNever();

        modelBuilder.Entity<TableSchemaProperty>().UseTpcMappingStrategy();
        modelBuilder.Entity<TableSchemaProperty>().Property(e => e.ID).ValueGeneratedNever();

        modelBuilder.Entity<TableSchemaDynamicValue>().UseTpcMappingStrategy();
        modelBuilder.Entity<TableSchemaDynamicValue>().Property(e => e.ID).ValueGeneratedNever();

        modelBuilder.Entity<TableAssociationSchemaActionInputHasAttribute>().UseTpcMappingStrategy();
        modelBuilder.Entity<TableAssociationSchemaActionInputHasAttribute>().Property(e => e.ID).ValueGeneratedNever();

        modelBuilder.Entity<TableUser>()
            .HasOne(e => e.Authentication)
            .WithOne(e => e.User)
            .HasForeignKey<TableUser>(x => x.AuthenticationID);

        #region -- Schema --

        modelBuilder.Entity<TableSchema>()
            .HasMany(e => e.ActionList)
            .WithOne(e => e.Schema)
            .HasForeignKey(x => x.SchemaID);

        modelBuilder.Entity<TableSchema>()
            .HasMany(e => e.ActionInputList)
            .WithOne(e => e.Schema)
            .HasForeignKey(x => x.SchemaID);

        modelBuilder.Entity<TableSchema>()
            .HasMany(e => e.ActionStepList)
            .WithOne(e => e.Schema)
            .HasForeignKey(x => x.SchemaID);

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
