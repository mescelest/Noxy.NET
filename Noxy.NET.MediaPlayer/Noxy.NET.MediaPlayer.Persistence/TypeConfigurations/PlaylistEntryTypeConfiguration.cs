using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;

namespace Noxy.NET.MediaPlayer.Persistence.TypeConfigurations;

public class PlaylistEntryTypeConfiguration : IEntityTypeConfiguration<TablePlaylistEntry>
{
    public void Configure(EntityTypeBuilder<TablePlaylistEntry> builder)
    {
        builder.Property(e => e.ID).ValueGeneratedNever();

        builder.HasOne(e => e.MediaFile)
            .WithMany()
            .HasForeignKey(x => x.MediaFileID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
