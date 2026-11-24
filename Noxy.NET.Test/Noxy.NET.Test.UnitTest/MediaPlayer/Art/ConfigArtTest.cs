using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Configurations;

namespace Noxy.NET.Test.MediaPlayer.Art;

public class ConfigArtTest
{
    private static List<ValidationResult> Validate(ConfigArt config)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(config, new(config), results, true);
        return results;
    }

    [Fact]
    public void Defaults_AreValid_AndAre300Pixels()
    {
        ConfigArt config = new();

        Assert.Empty(Validate(config));
        Assert.Equal(300, config.ThumbnailMaxPixels);
    }

    [Theory]
    [InlineData(32)]
    [InlineData(300)]
    [InlineData(2048)]
    public void ThumbnailMaxPixels_WithinRange_IsValid(int pixels)
    {
        Assert.Empty(Validate(new() { ThumbnailMaxPixels = pixels }));
    }

    [Theory]
    [InlineData(31)]
    [InlineData(2049)]
    public void ThumbnailMaxPixels_OutsideRange_IsInvalid(int pixels)
    {
        Assert.NotEmpty(Validate(new() { ThumbnailMaxPixels = pixels }));
    }
}
