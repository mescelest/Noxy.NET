using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Persistence.Tables.Metadata;

namespace Noxy.NET.MediaPlayer.Persistence.TypeConfigurations;

public class ArtistTypeConfiguration : IEntityTypeConfiguration<TableArtist>
{
    public void Configure(EntityTypeBuilder<TableArtist> builder)
    {
        builder.Property(e => e.ID).ValueGeneratedNever();

        builder.Property(e => e.Weight).HasDefaultValue(MetadataWeight.Default);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
