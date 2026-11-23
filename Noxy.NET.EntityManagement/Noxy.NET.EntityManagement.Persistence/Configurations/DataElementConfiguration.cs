using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities;

namespace Noxy.NET.EntityManagement.Persistence.Configurations;

internal sealed class DataElementConfiguration : IEntityTypeConfiguration<DataElement>
{
    public void Configure(EntityTypeBuilder<DataElement> builder)
    {
        builder.ToTable(nameof(DataElement));
        builder.HasKey(x => x.ID);
        builder.Property(x => x.ID).ValueGeneratedNever();
        builder.Property(x => x.SchemaIdentifier).IsRequired().HasMaxLength(BaseData.IdentifierMaxLength);

        // The elements of one kind, in the order they were made.
        builder.HasIndex(x => new { x.SchemaIdentifier, x.TimeCreated });
    }
}

internal sealed class HistoryElementConfiguration : IEntityTypeConfiguration<HistoryElement>
{
    public void Configure(EntityTypeBuilder<HistoryElement> builder)
    {
        builder.ToTable(nameof(HistoryElement));
        builder.HasKey(x => x.ID);
        builder.Property(x => x.ID).ValueGeneratedNever();
        builder.Property(x => x.SchemaIdentifier).IsRequired().HasMaxLength(BaseData.IdentifierMaxLength);

        builder.HasIndex(x => new { x.SchemaIdentifier, x.TimeEnded });
    }
}
