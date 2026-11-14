using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;

namespace Noxy.NET.MediaPlayer.Persistence;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<TableUser> User { get; set; } = null!;
    public DbSet<TableExternalLogin> ExternalLogin { get; set; } = null!;
    public DbSet<TableRefreshToken> RefreshToken { get; set; } = null!;
    public DbSet<TableMediaFile> MediaFile { get; set; } = null!;
    public DbSet<TableMediaFileMetadata> MediaFileMetadata { get; set; } = null!;
    public DbSet<TableMediaUploadJob> MediaUploadJob { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}
