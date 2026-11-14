using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;

namespace Noxy.NET.MediaPlayer.Persistence.TypeConfigurations;

public class PlaylistTypeConfiguration : IEntityTypeConfiguration<TablePlaylist>
{
    public void Configure(EntityTypeBuilder<TablePlaylist> builder)
    {
        builder.Property(e => e.ID).ValueGeneratedNever();

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.EntryList)
            .WithOne(e => e.Playlist)
            .HasForeignKey(x => x.PlaylistID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
