using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.API.Configurations;

namespace Noxy.NET.Test.MediaPlayer.Api;

public class ConfigRateLimitDownloadTest
{
    private static List<ValidationResult> Validate(ConfigRateLimitDownload config)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(config, new(config), results, true);
        return results;
    }

    [Fact]
    public void Defaults_AreValid_AndAreLow()
    {
        ConfigRateLimitDownload config = new();

        Assert.Empty(Validate(config));
        Assert.Equal(10, config.PermitLimit);
        Assert.Equal(60, config.WindowSeconds);
    }

    [Theory]
    [InlineData(0, 60)]
    [InlineData(10, 0)]
    public void ZeroLimitOrWindow_IsInvalid(int permitLimit, int windowSeconds)
    {
        Assert.NotEmpty(Validate(new() { PermitLimit = permitLimit, WindowSeconds = windowSeconds }));
    }
}
