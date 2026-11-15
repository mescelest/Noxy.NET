using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noxy.NET.MediaPlayer.Persistence.Tables.Metadata;

namespace Noxy.NET.MediaPlayer.Persistence.TypeConfigurations;

public class AlbumArtistTypeConfiguration : IEntityTypeConfiguration<TableAlbumArtist>
{
    public void Configure(EntityTypeBuilder<TableAlbumArtist> builder)
    {
        builder.Property(e => e.ID).ValueGeneratedNever();

        builder.HasOne(e => e.Artist)
            .WithMany()
            .HasForeignKey(x => x.ArtistID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
