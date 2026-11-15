using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noxy.NET.MediaPlayer.Persistence.Tables.Metadata;

namespace Noxy.NET.MediaPlayer.Persistence.TypeConfigurations;

public class AlbumTypeConfiguration : IEntityTypeConfiguration<TableAlbum>
{
    public void Configure(EntityTypeBuilder<TableAlbum> builder)
    {
        builder.Property(e => e.ID).ValueGeneratedNever();

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.ArtistLinkList)
            .WithOne(e => e.Album)
            .HasForeignKey(x => x.AlbumID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
