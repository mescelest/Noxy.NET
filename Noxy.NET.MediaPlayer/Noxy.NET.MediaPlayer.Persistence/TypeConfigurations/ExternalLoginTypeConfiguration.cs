using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

namespace Noxy.NET.MediaPlayer.Persistence.TypeConfigurations;

public class ExternalLoginTypeConfiguration : IEntityTypeConfiguration<TableExternalLogin>
{
    public void Configure(EntityTypeBuilder<TableExternalLogin> builder)
    {
        builder.Property(e => e.ID).ValueGeneratedNever();

        builder.Property(e => e.Provider).HasConversion<string>().HasMaxLength(32);
    }
}
