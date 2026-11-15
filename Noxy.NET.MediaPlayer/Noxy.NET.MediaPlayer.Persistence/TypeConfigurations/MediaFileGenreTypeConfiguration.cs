using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noxy.NET.MediaPlayer.Persistence.Tables.Metadata;

namespace Noxy.NET.MediaPlayer.Persistence.TypeConfigurations;

public class MediaFileGenreTypeConfiguration : IEntityTypeConfiguration<TableMediaFileGenre>
{
    public void Configure(EntityTypeBuilder<TableMediaFileGenre> builder)
    {
        builder.Property(e => e.ID).ValueGeneratedNever();

        builder.HasOne(e => e.Genre)
            .WithMany()
            .HasForeignKey(x => x.GenreID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
