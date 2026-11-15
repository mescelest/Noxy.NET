using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noxy.NET.MediaPlayer.Persistence.Tables.Metadata;

namespace Noxy.NET.MediaPlayer.Persistence.TypeConfigurations;

public class MediaFileArtistTypeConfiguration : IEntityTypeConfiguration<TableMediaFileArtist>
{
    public void Configure(EntityTypeBuilder<TableMediaFileArtist> builder)
    {
        builder.Property(e => e.ID).ValueGeneratedNever();

        builder.HasOne(e => e.Artist)
            .WithMany()
            .HasForeignKey(x => x.ArtistID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
