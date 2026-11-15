using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;
using Noxy.NET.MediaPlayer.Persistence.Tables.Metadata;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playback;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;
using Noxy.NET.MediaPlayer.Persistence.Tables.Setting;

namespace Noxy.NET.MediaPlayer.Persistence;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<TableUser> User { get; set; } = null!;
    public DbSet<TableExternalLogin> ExternalLogin { get; set; } = null!;
    public DbSet<TableRefreshToken> RefreshToken { get; set; } = null!;
    public DbSet<TableMediaFile> MediaFile { get; set; } = null!;
    public DbSet<TableMediaFileMetadata> MediaFileMetadata { get; set; } = null!;
    public DbSet<TableMediaUploadJob> MediaUploadJob { get; set; } = null!;
    public DbSet<TablePlaybackSession> PlaybackSession { get; set; } = null!;
    public DbSet<TablePlaylist> Playlist { get; set; } = null!;
    public DbSet<TablePlaylistEntry> PlaylistEntry { get; set; } = null!;
    public DbSet<TableArtist> Artist { get; set; } = null!;
    public DbSet<TableGenre> Genre { get; set; } = null!;
    public DbSet<TableAlbum> Album { get; set; } = null!;
    public DbSet<TableAlbumArtist> AlbumArtist { get; set; } = null!;
    public DbSet<TableMediaFileArtist> MediaFileArtist { get; set; } = null!;
    public DbSet<TableMediaFileGenre> MediaFileGenre { get; set; } = null!;
    public DbSet<TableUserSetting> UserSetting { get; set; } = null!;
    public DbSet<TableUserSettingBoolean> UserSettingBoolean { get; set; } = null!;
    public DbSet<TableUserSettingInteger> UserSettingInteger { get; set; } = null!;
    public DbSet<TableUserSettingEnum> UserSettingEnum { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}
