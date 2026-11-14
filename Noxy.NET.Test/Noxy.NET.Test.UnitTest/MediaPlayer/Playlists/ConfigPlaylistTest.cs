using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Configurations;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class ConfigPlaylistTest
{
    private static List<ValidationResult> Validate(ConfigPlaylist config)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(config, new(config), results, true);
        return results;
    }

    [Fact]
    public void Defaults_AreValid_AndAre100PlaylistsWith1000EntriesEach()
    {
        ConfigPlaylist config = new();

        Assert.Empty(Validate(config));
        Assert.Equal(100, config.MaxPlaylistsPerUser);
        Assert.Equal(1000, config.MaxEntriesPerPlaylist);
    }

    [Theory]
    [InlineData(0, 1000)]
    [InlineData(100, 0)]
    [InlineData(-1, 1000)]
    public void ZeroOrNegativeLimits_AreInvalid(int playlists, int entries)
    {
        Assert.NotEmpty(Validate(new() { MaxPlaylistsPerUser = playlists, MaxEntriesPerPlaylist = entries }));
    }
}
