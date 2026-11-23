using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities;

namespace Noxy.NET.EntityManagement.Persistence.Configurations;

internal sealed class DataRelationConfiguration : IEntityTypeConfiguration<DataRelation>
{
    public void Configure(EntityTypeBuilder<DataRelation> builder)
    {
        builder.ToTable(nameof(DataRelation));
        builder.HasKey(x => x.ID);
        builder.Property(x => x.ID).ValueGeneratedNever();
        builder.Property(x => x.RelationIdentifier).IsRequired().HasMaxLength(BaseData.IdentifierMaxLength);

        // One current relation of a kind between two elements.
        builder.HasIndex(x => new { x.RelationIdentifier, x.FromElementID, x.ToElementID }).IsUnique();

        // The relations of an element, read from either end.
        builder.HasIndex(x => new { x.FromElementID, x.RelationIdentifier });
        builder.HasIndex(x => new { x.ToElementID, x.RelationIdentifier });
    }
}

internal sealed class HistoryRelationConfiguration : IEntityTypeConfiguration<HistoryRelation>
{
    public void Configure(EntityTypeBuilder<HistoryRelation> builder)
    {
        builder.ToTable(nameof(HistoryRelation));
        builder.HasKey(x => x.ID);
        builder.Property(x => x.ID).ValueGeneratedNever();
        builder.Property(x => x.RelationIdentifier).IsRequired().HasMaxLength(BaseData.IdentifierMaxLength);

        builder.HasIndex(x => new { x.FromElementID, x.TimeEnded });
        builder.HasIndex(x => new { x.ToElementID, x.TimeEnded });
    }
}
