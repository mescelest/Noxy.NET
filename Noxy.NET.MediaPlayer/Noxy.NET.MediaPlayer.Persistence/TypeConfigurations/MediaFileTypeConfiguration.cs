using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;

namespace Noxy.NET.MediaPlayer.Persistence.TypeConfigurations;

public class MediaFileTypeConfiguration : IEntityTypeConfiguration<TableMediaFile>
{
    public void Configure(EntityTypeBuilder<TableMediaFile> builder)
    {
        builder.Property(e => e.ID).ValueGeneratedNever();

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Metadata)
            .WithOne(e => e.MediaFile)
            .HasForeignKey<TableMediaFileMetadata>(x => x.MediaFileID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Album)
            .WithMany()
            .HasForeignKey(x => x.AlbumID)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.ArtistLinkList)
            .WithOne(e => e.MediaFile)
            .HasForeignKey(x => x.MediaFileID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.GenreLinkList)
            .WithOne(e => e.MediaFile)
            .HasForeignKey(x => x.MediaFileID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
