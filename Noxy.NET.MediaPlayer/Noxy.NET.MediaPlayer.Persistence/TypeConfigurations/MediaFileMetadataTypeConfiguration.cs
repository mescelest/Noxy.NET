using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;

namespace Noxy.NET.MediaPlayer.Persistence.TypeConfigurations;

public class MediaFileMetadataTypeConfiguration : IEntityTypeConfiguration<TableMediaFileMetadata>
{
    public void Configure(EntityTypeBuilder<TableMediaFileMetadata> builder)
    {
        builder.Property(e => e.ID).ValueGeneratedNever();
    }
}
