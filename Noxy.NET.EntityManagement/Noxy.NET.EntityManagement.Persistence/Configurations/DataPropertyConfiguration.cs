using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities;

namespace Noxy.NET.EntityManagement.Persistence.Configurations;

// The property types are one hierarchy with a table for each type (table per concrete type), so a value is stored in a column of its own type, and a query on the base class reads all of the tables.
// The history is a hierarchy of its own, with the twin of each table.
internal static class DataPropertyConfiguration
{
    public static void Apply(ModelBuilder builder)
    {
        builder.Entity<DataProperty>().UseTpcMappingStrategy();
        builder.Entity<DataProperty>().HasKey(x => x.ID);
        builder.Entity<DataProperty>().Property(x => x.ID).ValueGeneratedNever();
        builder.Entity<DataProperty>().Property(x => x.PropertyIdentifier).IsRequired().HasMaxLength(BaseData.IdentifierMaxLength);

        builder.Entity<HistoryProperty>().UseTpcMappingStrategy();
        builder.Entity<HistoryProperty>().HasKey(x => x.ID);
        builder.Entity<HistoryProperty>().Property(x => x.ID).ValueGeneratedNever();
        builder.Entity<HistoryProperty>().Property(x => x.PropertyIdentifier).IsRequired().HasMaxLength(BaseData.IdentifierMaxLength);

        Type<DataPropertyBoolean, HistoryPropertyBoolean>(builder);
        Type<DataPropertyDateTime, HistoryPropertyDateTime>(builder);
        Type<DataPropertyDecimal, HistoryPropertyDecimal>(builder);
        Type<DataPropertyInteger, HistoryPropertyInteger>(builder);

        TypeOfText<DataPropertyImage, HistoryPropertyImage>(builder);
        TypeOfText<DataPropertyString, HistoryPropertyString>(builder);
    }

    private static EntityTypeBuilder<TProperty> Type<TProperty, THistory>(ModelBuilder builder) where TProperty : DataProperty where THistory : HistoryProperty
    {
        EntityTypeBuilder<TProperty> current = builder.Entity<TProperty>();
        current.ToTable(typeof(TProperty).Name);

        // One current value of a property for each owner.
        current.HasIndex(x => new { x.OwnerID, x.PropertyIdentifier }).IsUnique();

        EntityTypeBuilder<THistory> history = builder.Entity<THistory>();
        history.ToTable(typeof(THistory).Name);
        history.HasIndex(x => new { x.OwnerID, x.TimeEnded });

        return current;
    }

    private static void TypeOfText<TProperty, THistory>(ModelBuilder builder) where TProperty : DataProperty<string, THistory> where THistory : HistoryProperty<string>, new()
    {
        Type<TProperty, THistory>(builder).Property(x => x.Value).HasMaxLength(BaseData.TextMaxLength);
        builder.Entity<THistory>().Property(x => x.Value).HasMaxLength(BaseData.TextMaxLength);
    }
}
