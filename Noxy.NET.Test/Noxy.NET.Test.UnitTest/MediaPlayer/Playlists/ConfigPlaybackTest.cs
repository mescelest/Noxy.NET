using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Configurations;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class ConfigPlaybackTest
{
    private static List<ValidationResult> Validate(ConfigPlayback config)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(config, new(config), results, true);
        return results;
    }

    [Fact]
    public void Defaults_AreValid_AQueueOf1000AndAPlayedWindowOf10Seconds()
    {
        ConfigPlayback config = new();

        Assert.Empty(Validate(config));
        Assert.Equal(1000, config.MaxQueueLength);
        Assert.Equal(10, config.PlayedWindowSeconds);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(10001, 10)]
    [InlineData(1000, -1)]
    [InlineData(1000, 3601)]
    public void OutOfRangeValues_AreInvalid(int queue, int window)
    {
        Assert.NotEmpty(Validate(new() { MaxQueueLength = queue, PlayedWindowSeconds = window }));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(10000, 3600)]
    public void TheEdgesOfTheRanges_AreValid(int queue, int window)
    {
        Assert.Empty(Validate(new() { MaxQueueLength = queue, PlayedWindowSeconds = window }));
    }
}
