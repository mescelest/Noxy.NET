using Microsoft.EntityFrameworkCore;
using Noxy.NET.EntityManagement.Persistence.Seeds;
using Noxy.NET.EntityManagement.Persistence.Tables.Authentication;
using Noxy.NET.EntityManagement.Persistence.Tables.Data;
using Noxy.NET.EntityManagement.Persistence.Tables.Data.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Persistence.Tables.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Persistence;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<TableAuthentication> Authentication { get; set; } = null!;
    public DbSet<TableIdentity> Identity { get; set; } = null!;
    public DbSet<TableUser> User { get; set; } = null!;

    public DbSet<TableDataElement> DataElement { get; set; } = null!;

    public DbSet<TableDataProperty> DataProperty { get; set; } = null!;
    public DbSet<TableDataPropertyBoolean> DataPropertyBoolean { get; set; } = null!;
    public DbSet<TableDataPropertyDateTime> DataPropertyDateTime { get; set; } = null!;
    public DbSet<TableDataPropertyString> DataPropertyString { get; set; } = null!;

    public DbSet<TableDataParameter> DataParameter { get; set; } = null!;
    public DbSet<TableDataParameterStyle> DataParameterStyle { get; set; } = null!;
    public DbSet<TableDataParameterSystem> DataParameterSystem { get; set; } = null!;
    public DbSet<TableDataParameterText> DataParameterText { get; set; } = null!;

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

    public DbSet<TableSchemaContextHasElement> SchemaContextHasElement { get; set; } = null!;
    public DbSet<TableSchemaElementHasProperty> SchemaElementHasProperty { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TableDataProperty>().UseTpcMappingStrategy();
        builder.Entity<TableDataProperty>().Property(e => e.ID).ValueGeneratedNever();
        builder.Entity<TableDataPropertyBoolean>().ToTable("TableDataPropertyBoolean").HasBaseType<TableDataProperty>();
        builder.Entity<TableDataPropertyDateTime>().ToTable("TableDataPropertyDateTime").HasBaseType<TableDataProperty>();
        builder.Entity<TableDataPropertyString>().ToTable("TableDataPropertyString").HasBaseType<TableDataProperty>();

        builder.Entity<TableDataParameter>().UseTpcMappingStrategy();
        builder.Entity<TableDataParameter>().Property(e => e.ID).ValueGeneratedNever();
        builder.Entity<TableDataParameterStyle>().ToTable("TableDataParameterStyle").HasBaseType<TableDataParameter>();
        builder.Entity<TableDataParameterSystem>().ToTable("TableDataParameterSystem").HasBaseType<TableDataParameter>();
        builder.Entity<TableDataParameterText>().ToTable("TableDataParameterText").HasBaseType<TableDataParameter>();

        builder.Entity<TableSchemaProperty>().UseTpcMappingStrategy();
        builder.Entity<TableSchemaProperty>().Property(e => e.ID).ValueGeneratedNever();
        builder.Entity<TableSchemaPropertyBoolean>().ToTable("TableSchemaPropertyBoolean").HasBaseType<TableSchemaProperty>();
        builder.Entity<TableSchemaPropertyCollection>().ToTable("TableSchemaPropertyCollection").HasBaseType<TableSchemaProperty>();
        builder.Entity<TableSchemaPropertyDateTime>().ToTable("TableSchemaPropertyDateTime").HasBaseType<TableSchemaProperty>();
        builder.Entity<TableSchemaPropertyDecimal>().ToTable("TableSchemaPropertyDecimal").HasBaseType<TableSchemaProperty>();
        builder.Entity<TableSchemaPropertyImage>().ToTable("TableSchemaPropertyImage").HasBaseType<TableSchemaProperty>();
        builder.Entity<TableSchemaPropertyInteger>().ToTable("TableSchemaPropertyInteger").HasBaseType<TableSchemaProperty>();
        builder.Entity<TableSchemaPropertyString>().ToTable("TableSchemaPropertyString").HasBaseType<TableSchemaProperty>();
        builder.Entity<TableSchemaPropertyTable>().ToTable("TableSchemaPropertyTable").HasBaseType<TableSchemaProperty>();

        builder.Entity<TableSchemaParameter>().UseTpcMappingStrategy();
        builder.Entity<TableSchemaParameter>().Property(e => e.ID).ValueGeneratedNever();
        builder.Entity<TableSchemaParameterStyle>().ToTable("TableSchemaParameterStyle").HasBaseType<TableSchemaParameter>();
        builder.Entity<TableSchemaParameterSystem>().ToTable("TableSchemaParameterSystem").HasBaseType<TableSchemaParameter>();
        builder.Entity<TableSchemaParameterText>().ToTable("TableSchemaParameterText").HasBaseType<TableSchemaParameter>();

        builder.Entity<TableUser>()
            .HasOne(e => e.Authentication)
            .WithOne(e => e.User)
            .HasForeignKey<TableUser>(x => x.AuthenticationID);

        #region -- Schema --

        builder.Entity<TableSchema>()
            .HasMany(e => e.ContextList)
            .WithOne(e => e.Schema)
            .HasForeignKey(x => x.SchemaID);

        builder.Entity<TableSchema>()
            .HasMany(e => e.ElementList)
            .WithOne(e => e.Schema)
            .HasForeignKey(x => x.SchemaID);

        builder.Entity<TableSchema>()
            .HasMany(e => e.ParameterList)
            .WithOne(e => e.Schema)
            .HasForeignKey(x => x.SchemaID);

        builder.Entity<TableSchema>()
            .HasMany(e => e.PropertyList)
            .WithOne(e => e.Schema)
            .HasForeignKey(x => x.SchemaID);

        #endregion -- Schema --

        #region -- Junctions --

        builder.Entity<TableSchemaPropertyCollectionHasProperty>()
            .HasOne(e => e.Entity)
            .WithMany(e => e.PropertyList)
            .HasForeignKey(x => x.EntityID);

        builder.Entity<TableSchemaPropertyCollectionHasProperty>()
            .HasOne(e => e.Relation)
            .WithMany(e => e.RelationPropertyCollectionList)
            .HasForeignKey(x => x.RelationID);

        builder.Entity<TableSchemaPropertyTableHasProperty>()
            .HasOne(e => e.Entity)
            .WithMany(e => e.PropertyList)
            .HasForeignKey(x => x.EntityID);

        builder.Entity<TableSchemaPropertyTableHasProperty>()
            .HasOne(e => e.Relation)
            .WithMany(e => e.RelationPropertyTableList)
            .HasForeignKey(x => x.RelationID);

        #endregion -- Junctions --

        TableSchema schema = new TableSchema
        {
            ID = Guid.Parse("12345678-1234-1234-1234-1234567890ab"),
            Name = "Base schema",
            Note = "This is a base schema intended to be cloned and extended.",
            IsActive = true,
            TimeCreated = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TimeActivated = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        builder.Entity<TableSchema>().HasData(schema);

        new SchemaParameterTextSeed(builder, schema).Apply();
        new SchemaParameterSystemSeed(builder, schema).Apply();
    }
}
