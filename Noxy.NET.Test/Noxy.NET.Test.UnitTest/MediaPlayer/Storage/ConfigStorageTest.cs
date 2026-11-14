using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Configurations;

namespace Noxy.NET.Test.MediaPlayer.Storage;

public class ConfigStorageTest
{
    private static ConfigStorage Valid()
    {
        return new() { RootPath = "Media", AllowedExtensions = ["mp3"] };
    }

    private static bool IsValid(ConfigStorage config)
    {
        return Validator.TryValidateObject(config, new(config), [], true);
    }

    [Fact]
    public void Valid_Passes()
    {
        Assert.True(IsValid(Valid()));
    }

    [Fact]
    public void DefaultMaxSize_Is100Megabytes()
    {
        Assert.Equal(100L * 1024 * 1024, Valid().MaxSizeBytes);
    }

    [Fact]
    public void EmptyRootPath_Fails()
    {
        ConfigStorage config = Valid();
        config.RootPath = "";

        Assert.False(IsValid(config));
    }

    [Fact]
    public void NoAllowedExtensions_Fails()
    {
        ConfigStorage config = Valid();
        config.AllowedExtensions = [];

        Assert.False(IsValid(config));
    }

    [Fact]
    public void NonPositiveMaxSize_Fails()
    {
        ConfigStorage config = Valid();
        config.MaxSizeBytes = 0;

        Assert.False(IsValid(config));
    }
}
