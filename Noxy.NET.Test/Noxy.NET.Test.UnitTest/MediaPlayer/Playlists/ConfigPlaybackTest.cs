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
    public void PlayedWindowSeconds_KeepsItsRangeAndDefault()
    {
        ConfigPlayback config = new();

        Assert.Empty(Validate(config));
        Assert.Equal(10, config.PlayedWindowSeconds);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3601)]
    public void OutOfRangeWindows_AreInvalid(int window)
    {
        Assert.NotEmpty(Validate(new() { PlayedWindowSeconds = window }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3600)]
    public void TheEdgesOfTheRange_AreValid(int window)
    {
        Assert.Empty(Validate(new() { PlayedWindowSeconds = window }));
    }

    [Fact]
    public void MaxQueueLength_IsGone()
    {
        Assert.Null(typeof(ConfigPlayback).GetProperty("MaxQueueLength"));
    }
}
