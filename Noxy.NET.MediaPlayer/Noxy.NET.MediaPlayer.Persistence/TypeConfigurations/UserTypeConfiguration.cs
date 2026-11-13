using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

namespace Noxy.NET.MediaPlayer.Persistence.TypeConfigurations;

public class UserTypeConfiguration : IEntityTypeConfiguration<TableUser>
{
    public void Configure(EntityTypeBuilder<TableUser> builder)
    {
        builder.Property(e => e.ID).ValueGeneratedNever();

        builder.HasMany(e => e.ExternalLoginList)
            .WithOne(e => e.User)
            .HasForeignKey(x => x.UserID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
