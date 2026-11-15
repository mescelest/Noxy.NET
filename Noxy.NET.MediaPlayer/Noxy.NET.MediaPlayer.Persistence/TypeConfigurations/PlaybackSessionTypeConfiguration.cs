using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playback;

namespace Noxy.NET.MediaPlayer.Persistence.TypeConfigurations;

public class PlaybackSessionTypeConfiguration : IEntityTypeConfiguration<TablePlaybackSession>
{
    public void Configure(EntityTypeBuilder<TablePlaybackSession> builder)
    {
        builder.Property(e => e.ID).ValueGeneratedNever();

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.QueueMediaFileIDList)
            .HasConversion(
                queue => JsonSerializer.Serialize(queue, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<List<Guid>>(json, (JsonSerializerOptions?)null) ?? new List<Guid>(),
                new ValueComparer<List<Guid>>(
                    (left, right) => left != null && right != null && left.SequenceEqual(right),
                    queue => queue.Aggregate(0, (hash, id) => HashCode.Combine(hash, id.GetHashCode())),
                    queue => queue.ToList()));
    }
}
