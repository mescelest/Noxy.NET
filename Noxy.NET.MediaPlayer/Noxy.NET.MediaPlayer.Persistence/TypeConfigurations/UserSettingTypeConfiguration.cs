using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noxy.NET.MediaPlayer.Persistence.Tables.Setting;

namespace Noxy.NET.MediaPlayer.Persistence.TypeConfigurations;

public class UserSettingTypeConfiguration : IEntityTypeConfiguration<TableUserSetting>
{
    public void Configure(EntityTypeBuilder<TableUserSetting> builder)
    {
        builder.Property(e => e.ID).ValueGeneratedNever();

        builder.UseTptMappingStrategy();

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
