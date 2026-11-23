using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities;

namespace Noxy.NET.EntityManagement.Persistence.Configurations;

internal sealed class DataTextParameterConfiguration : IEntityTypeConfiguration<DataTextParameter>
{
    // The culture is compared without regard to case, in the database too, so "da-DK" and "da-dk" are one culture and cannot both have a value.
    public const string CultureCollation = "NOCASE";

    public void Configure(EntityTypeBuilder<DataTextParameter> builder)
    {
        builder.ToTable(nameof(DataTextParameter));
        builder.HasKey(x => x.ID);
        builder.Property(x => x.ID).ValueGeneratedNever();
        builder.Property(x => x.TextIdentifier).IsRequired().HasMaxLength(BaseData.IdentifierMaxLength);
        builder.Property(x => x.Culture).IsRequired().HasMaxLength(DataTextParameter.CultureMaxLength).UseCollation(CultureCollation);
        builder.Property(x => x.Value).IsRequired().HasMaxLength(BaseData.TextMaxLength);

        // One current value of a text in a culture.
        builder.HasIndex(x => new { x.TextIdentifier, x.Culture }).IsUnique();

        // Every text of a culture.
        builder.HasIndex(x => x.Culture);
    }
}

internal sealed class HistoryTextParameterConfiguration : IEntityTypeConfiguration<HistoryTextParameter>
{
    public void Configure(EntityTypeBuilder<HistoryTextParameter> builder)
    {
        builder.ToTable(nameof(HistoryTextParameter));
        builder.HasKey(x => x.ID);
        builder.Property(x => x.ID).ValueGeneratedNever();
        builder.Property(x => x.TextIdentifier).IsRequired().HasMaxLength(BaseData.IdentifierMaxLength);
        builder.Property(x => x.Culture).IsRequired().HasMaxLength(DataTextParameter.CultureMaxLength).UseCollation(DataTextParameterConfiguration.CultureCollation);
        builder.Property(x => x.Value).IsRequired().HasMaxLength(BaseData.TextMaxLength);

        builder.HasIndex(x => new { x.TextIdentifier, x.Culture, x.TimeEnded });
    }
}
